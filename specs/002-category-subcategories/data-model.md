# Phase 1 — Data Model

**Feature**: 002-category-subcategories
**Spec**: [spec.md](./spec.md) · **Research**: [research.md](./research.md)

This document is the canonical reference for entities, fields, relationships, validation, and state transitions introduced or modified by this feature. All paths below are relative to the repo root.

---

## Entity: Category (modified)

`Lofn.Infra.Context.Category` (EF entity), `Lofn.Domain.Models.CategoryModel` (domain), `Lofn.DTO.Category.CategoryInfo` (DTO).

### Fields

| Field | Type | Existing? | Nullable | Notes |
|------|------|-----------|----------|-------|
| `CategoryId` | `bigint` (`long`) | ✅ | NO | PK, unchanged |
| `Slug` | `varchar(512)` | ✅ (was 120) | NO | **Widened from 120 → 512** to fit full ancestor path |
| `Name` | `varchar(120)` | ✅ | NO | Unchanged |
| `StoreId` | `bigint` (`long?`) | ✅ | YES | Existing scope marker — NULL ⇒ global |
| **`ParentId`** | `bigint` (`long?`) | NEW | YES | Self-FK to `CategoryId`. NULL ⇒ root |

### Computed/derived

| Member | Where | Logic |
|--------|-------|-------|
| `IsGlobal` | `CategoryModel`, `CategoryInfo`, GraphQL `CategoryTypeExtension` | `StoreId == null` (unchanged) |
| `Depth` (internal, no field) | `CategoryService` only | Walk `Parent.Parent...` until null; root = 1, max enforced = 5 |
| Tree position | `CategoryTreeNodeInfo.Children[]` | Built in service, returned via GraphQL |

### Relationships

| Relation | Target | Cardinality | Cascade |
|---------|--------|-------------|---------|
| `Parent` | `Category` (self) | 0..1 | `ON DELETE RESTRICT` (DB enforces hard fallback; service layer rejects first per FR-010) |
| `Children` | `Category` (self) | 0..N | Inverse of `Parent` |
| `Store` | `Store` | 0..1 (NULL when global) | Existing — `ON DELETE NO ACTION` (unchanged) |
| `Products` | `Product` | 0..N | Existing — service forbids delete when non-empty (unchanged) |

### Constraints

| Constraint | Form | Enforcement |
|------------|------|-------------|
| **PK** | `category_id` | Existing |
| **FK self-ref** | `parent_id → category_id` `ON DELETE RESTRICT` | New, DB |
| **Global slug uniqueness** | `UNIQUE INDEX (slug) WHERE store_id IS NULL` | Existing (feature 001) |
| **Per-store slug uniqueness** | Service-layer via `ExistSlugInTenantAsync` | Existing |
| **Sibling-name uniqueness** | `UNIQUE INDEX ((COALESCE(parent_id, 0)), (COALESCE(store_id, 0)), lower(name))` | New, DB |
| **Slug max length** | `varchar(512)` | New, DB |

### Validation rules (service layer + FluentValidation)

| Rule | FR | Where enforced |
|------|----|----------------|
| Parent must exist | FR-001 | `CategoryService` resolves parent before save; throws `ValidationException` if missing |
| Parent must share scope | FR-002 | `CategoryService.AssertScopeMatchAsync(parent, child)` |
| No cycles | FR-003 | `CategoryService.AssertNoCycleAsync(categoryId, prospectiveParentId)` walks ancestors |
| Max depth 5 | FR-004 | Walk returns hop count; reject if `count + 1 > 5` |
| Sibling name unique within parent | FR-009 | DB index + service pre-check for friendly error |
| Cannot delete with children | FR-010 | `CategoryService.DeleteAsync*` calls `_categoryRepository.HasChildrenAsync(categoryId)` first |
| Cannot delete with products | (existing) | Pre-existing rule, unchanged |

### State transitions

```text
                +-------------+        rename / move        +-------------+
                |  Persisted  | ─────── (with cascade ──── │  Persisted  |
   create ───>  |  (root or   |   slug recompute on        │  (new slug, |
                |   nested)   |   self + descendants) ───> │  same id)   |
                +-------------+                            +-------------+
                       │                                          │
                       │   delete (only if no children            │
                       └─── and no direct products) ─────────────►  Deleted
```

Move semantics:
- `ParentCategoryId = newParent.Id` ⇒ category becomes child of `newParent` if scope matches.
- `ParentCategoryId = null` ⇒ category detaches to root.
- Both trigger slug cascade on the moved category and all descendants atomically (one transaction).

---

## Entity: CategoryTreeNodeInfo (new, projection-only)

`Lofn.DTO.Category.CategoryTreeNodeInfo`. Read-only DTO returned by `GetCategoryTree*` GraphQL fields.

### Fields

| Field | Type | Notes |
|------|------|-------|
| `CategoryId` | `long` | Identifier |
| `Name` | `string` | Display name |
| `Slug` | `string` | Full ancestor path |
| `ParentCategoryId` | `long?` | NULL for roots |
| `IsGlobal` | `bool` | Mirror of `StoreId == null` |
| `Children` | `IList<CategoryTreeNodeInfo>` | Recursive — alphabetically ordered by `Name` (case-insensitive, accent-normalized) |

### Construction

Built by `CategoryService.GetTreeAsync(scope)`:
1. Repository fetches all rows for the scope (`ListByScopeAsync`).
2. Service groups by `ParentId` into a dictionary.
3. Service emits roots (`ParentId == null`), recursively attaches children.
4. Each level is sorted by `IStringClient.NormalizeForCompareAsync(name)` (or equivalent invariant-culture comparer with accent-strip).

---

## DTOs added/modified

| DTO | Change | New field |
|-----|--------|-----------|
| `CategoryInsertInfo` | ADD | `ParentCategoryId` (nullable `long?`) |
| `CategoryUpdateInfo` | ADD | `ParentCategoryId` (nullable `long?`) |
| `CategoryGlobalInsertInfo` | ADD | `ParentCategoryId` (nullable `long?`) |
| `CategoryGlobalUpdateInfo` | ADD | `ParentCategoryId` (nullable `long?`) |
| `CategoryInfo` (response) | ADD | `ParentCategoryId` (nullable `long?`) |
| `CategoryTreeNodeInfo` | NEW | (entire DTO above) |

All new fields are JSON-serialized as `parentCategoryId` (camelCase, matching existing convention in `Lofn.DTO`).

---

## Validators (FluentValidation)

For each of the four DTOs, the validator MUST enforce:

```text
RuleFor(x => x.Name)
    .NotEmpty()
    .MaximumLength(120);

When(x => x.ParentCategoryId.HasValue, () =>
{
    RuleFor(x => x.ParentCategoryId.Value)
        .GreaterThan(0)
        .WithMessage("ParentCategoryId must be positive when provided.");
});
```

Cross-row checks (parent exists, scope match, no cycle, depth ≤ 5) are NOT in the validator — they require DB access and live in `CategoryService` so they remain testable with Moq.

---

## Repository surface (`ICategoryRepository<CategoryModel>`)

| Method | Purpose | New? |
|--------|---------|------|
| `ListByScopeAsync(long? storeId)` | Tree feed: returns all categories with `StoreId == storeId` (or `IS NULL` when arg is null) | NEW |
| `GetDescendantsAsync(long categoryId)` | Returns all descendants for cascade slug update | NEW |
| `GetAncestorChainAsync(long categoryId)` | Returns ancestors from category up to root | NEW |
| `ExistSiblingNameAsync(long? parentId, long? storeId, string name, long? excludeId)` | Pre-check before DB insert; matches the unique index | NEW |
| `HasChildrenAsync(long categoryId)` | True iff at least one row has `parent_id == categoryId` | NEW |
| `UpdateManyAsync(IEnumerable<CategoryModel> rows)` | Bulk update used by cascade slug rewriting | NEW |
| (all existing methods remain unchanged) |  |  |

---

## EF Core mapping changes (`LofnContext.OnModelCreating`)

```csharp
modelBuilder.Entity<Category>(entity =>
{
    // existing mappings preserved verbatim, then:

    entity.Property(e => e.ParentId).HasColumnName("parent_id");
    entity.Property(e => e.Slug)
        .IsRequired()
        .HasMaxLength(512)        // was 120
        .HasColumnName("slug");

    entity.HasOne(d => d.Parent)
        .WithMany(p => p.Children)
        .HasForeignKey(d => d.ParentId)
        .OnDelete(DeleteBehavior.Restrict)
        .HasConstraintName("fk_lofn_category_parent");

    entity.HasIndex(
            "store_id_coalesced",   // expression-index — see migration SQL for the actual DDL
            "parent_id_coalesced",
            "name_lower")
        .IsUnique()
        .HasDatabaseName("ix_lofn_categories_sibling_name_unique");
});
```

Note: the expression-based unique index `UNIQUE ((COALESCE(parent_id,0)), (COALESCE(store_id,0)), lower(name))` is created by raw SQL in the migration (EF Core 9 cannot model expression indexes through the fluent API on Postgres without the `Npgsql` extension `HasMethod`/raw `HasIndex` syntax). The mapping above is illustrative; the migration SQL is the source of truth.

---

## Migration script: `Lofn.Infra/Migrations/20260429_AddCategoryParentId.sql`

Idempotent DDL applied per tenant DB. Mirrors the pattern of `20260428_AddGlobalCategoryUniqueIndex.sql`.

```sql
-- Migration: 20260429_AddCategoryParentId
-- Feature: 002-category-subcategories
-- Apply per tenant DB:
--   psql "$ConnectionString" -f 20260429_AddCategoryParentId.sql

-- 1. Add parent_id column (NULL ⇒ root, preserving pre-rollout categories as roots).
ALTER TABLE lofn_categories
    ADD COLUMN IF NOT EXISTS parent_id BIGINT NULL;

-- 2. Self-FK with ON DELETE RESTRICT — DB enforces hard fallback for FR-010.
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_category_parent'
    ) THEN
        ALTER TABLE lofn_categories
            ADD CONSTRAINT fk_lofn_category_parent
            FOREIGN KEY (parent_id)
            REFERENCES lofn_categories (category_id)
            ON DELETE RESTRICT;
    END IF;
END $$;

-- 3. Widen slug to fit full ancestor path (5 levels × 100 chars + 4 slashes).
ALTER TABLE lofn_categories
    ALTER COLUMN slug TYPE VARCHAR(512);

-- 4. Sibling-name uniqueness — see Research §R5.
CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_categories_sibling_name_unique
    ON lofn_categories ((COALESCE(parent_id, 0)), (COALESCE(store_id, 0)), lower(name));

-- 5. Helper index for cascade descendant walks (parent_id is queried frequently).
CREATE INDEX IF NOT EXISTS ix_lofn_categories_parent_id
    ON lofn_categories (parent_id) WHERE parent_id IS NOT NULL;

-- Rollout note:
-- Pre-existing rows have parent_id = NULL ⇒ treated as roots.
-- Their slugs are unchanged (single-segment paths still satisfy the new format).
-- If two pre-existing rows in the same store share a name (case-insensitive),
-- the unique index in step 4 will FAIL — operator must dedupe first.
```

`lofn.sql` (bootstrap for fresh tenants) is updated to inline the same DDL inside the `CREATE TABLE lofn_categories` block, so newly provisioned tenants get the schema right the first time.

---

## Backward compatibility

- Pre-rollout categories: `parent_id = NULL`, slugs unchanged. URLs that worked before continue to work (FR-014, SC-005).
- Existing flat list endpoints continue to return the same JSON shape, with one extra field (`parentCategoryId`) — additive, non-breaking (R7).
- Existing GraphQL `categories` and `myCategories` queries return the same shape — `parentCategoryId` is exposed on `Category` as a new optional field.
