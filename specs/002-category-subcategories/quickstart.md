# Quickstart — Category Subcategories Support

**Feature**: 002-category-subcategories · **Phase 1 output**

This guide walks a developer through setting up, validating, and exercising the feature locally. It assumes the existing Lofn dev environment is up (per `CLAUDE.md`).

---

## 1. Prerequisites

- .NET 8 SDK installed.
- PostgreSQL running locally (default Lofn dev config).
- Node 20+ for the React frontend (not exercised by this feature).
- Docker Compose (optional, for full nginx-proxy setup).
- A tenant configured in `appsettings.Development.json`. For testing both modes, two tenants are recommended:
    - `emagine` — `Marketplace=true` (global category surface).
    - `monexup` — `Marketplace=false` (store-scoped surface).

---

## 2. Apply the schema migration

For each existing tenant database, apply the migration once:

```bash
psql "$ConnectionString" -f Lofn.Infra/Migrations/20260429_AddCategoryParentId.sql
```

For freshly provisioned tenants the same DDL is already in `lofn.sql` — no separate step needed.

**Pre-flight dedupe (only if upgrading an existing tenant)**: if the new sibling-name unique index fails to create, two pre-existing categories share a name within the same store. Run:

```sql
SELECT
    COALESCE(parent_id, 0) AS parent_bucket,
    COALESCE(store_id, 0) AS store_bucket,
    lower(name) AS norm_name,
    array_agg(category_id) AS dupes
FROM lofn_categories
GROUP BY parent_bucket, store_bucket, norm_name
HAVING COUNT(*) > 1;
```

Resolve the dupes (rename one, or merge), then retry.

---

## 3. Build & run

```bash
dotnet build Lofn.sln
dotnet run --project Lofn.API
```

API listens on `https://localhost:44374` (existing). GraphQL playground at `/graphql` (public) and `/graphql/admin` (admin).

---

## 4. Smoke test — create a 3-level hierarchy

Using a marketplace tenant (`emagine`, `Marketplace=true`):

### 4.1 Create the root category (admin REST)

```bash
TOKEN=...   # obtain via NAuth login
curl -X POST https://localhost:44374/category-global/insert \
    -H "X-Tenant-Id: emagine" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{ "name": "Vestuário" }'
```

Expected response:

```json
{
    "categoryId": 12,
    "name": "Vestuário",
    "slug": "vestuario",
    "isGlobal": true,
    "parentCategoryId": null,
    "productCount": 0
}
```

### 4.2 Create a child under it

```bash
curl -X POST https://localhost:44374/category-global/insert \
    -H "X-Tenant-Id: emagine" -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{ "name": "Camisetas", "parentCategoryId": 12 }'
```

Expected `slug`: `vestuario/camisetas`. Expected `parentCategoryId`: `12`.

### 4.3 Create a grandchild

```bash
curl -X POST https://localhost:44374/category-global/insert \
    -H "X-Tenant-Id: emagine" -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{ "name": "Vintage", "parentCategoryId": 17 }'
```

Expected `slug`: `vestuario/camisetas/vintage`.

### 4.4 Fetch the tree via GraphQL (anonymous, public schema)

```bash
curl -X POST https://localhost:44374/graphql \
    -H "X-Tenant-Id: emagine" \
    -H "Content-Type: application/json" \
    -d '{
        "query": "{ categoryTree { categoryId name slug children { categoryId name slug children { categoryId name slug } } } }"
    }'
```

Expected: a single root (`Vestuário`), one child (`Camisetas`), one grandchild (`Vintage`), all with full-path slugs and recursive `children` arrays.

---

## 5. Smoke test — store-scoped flow

Switch to `monexup` (`Marketplace=false`) and follow the same flow but against `/category/{storeSlug}/insert` instead of `/category-global/insert`. The tree query becomes:

```graphql
query {
    categoryTree(storeSlug: "minha-loja") {
        categoryId name slug
        children { categoryId name slug }
    }
}
```

In marketplace mode the same query (without `storeSlug`) returns the global tree; passing `storeSlug` is ignored. In non-marketplace mode, omitting `storeSlug` returns an empty array (per Research §R6).

---

## 6. Verify guard rails

Try and confirm each of these is **rejected** with a 400 / GraphQL error message:

| Action | Expected reject reason |
|--------|------------------------|
| Insert child with `parentCategoryId: 9999` (non-existent) | `Parent category 9999 not found` |
| Insert child of a global parent via `/category/{slug}/insert` (cross-scope) | `Parent and child must share the same scope` |
| Update a parent so its parent becomes its own child (cycle) | `Setting parent {id} would create a cycle` |
| Insert at depth 6 (root + 5 nested) | `Maximum nesting depth (5) would be exceeded` |
| Insert two siblings with same name under same parent | `A category named "X" already exists under this parent` |
| Delete a category that has subcategories | `Category {id} has subcategories; remove them first` |
| Anonymous `myCategoryTree` | `AUTH_NOT_AUTHENTICATED` |

---

## 7. Verify the cascade rename

```bash
# Rename the root from "Vestuário" to "Roupas"
curl -X POST https://localhost:44374/category-global/update \
    -H "X-Tenant-Id: emagine" -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{ "categoryId": 12, "name": "Roupas" }'
```

Re-fetch the tree. Expected: every descendant's slug now starts with `roupas/` instead of `vestuario/`.

---

## 8. Run the test suites

```bash
# Unit tests (fast, no DB)
dotnet test Lofn.Tests/

# Integration tests (require API running on localhost)
# 1) Start API in another terminal
# 2) Then:
dotnet test Lofn.ApiTests/
```

Expected new tests:

- `CategoryServiceTests` — cycle/depth/scope/cascade-slug/tree-shape/sibling-name (≈25 cases).
- `CategoryInsertInfoValidatorTests`, `CategoryUpdateInfoValidatorTests` — parent rules per validator (≈8 cases each).
- `CategoryGlobalInsertInfoValidatorTests`, `CategoryGlobalUpdateInfoValidatorTests` — parent rules added (≈4 new cases each).
- `CategoryMutualExclusionTests` — XOR mutex now also exercises the parent field (1 new test per surface).
- `CategoryTreeGraphQLTests` — tree shape + alphabetical order + mutex + auth (≈6 cases).

All previous tests continue to pass — nothing in the existing surface is removed.

---

## 9. Known limitations

- **>500 categories per tenant**: SC-002 latency is not guaranteed beyond 500 categories. The query still returns correctly, but the in-memory tree-build cost grows linearly. Monitor and revisit if real tenants approach this scale.
- **Concurrent edits**: two admins simultaneously moving overlapping subtrees may produce a "last writer wins" outcome. Out of scope for this feature; covered in Research §R1's deferred items.
- **Audit trail**: who-moved-what is not recorded. Not covered.

---

## 10. Rollback

If the feature must be rolled back at the schema level (rare, since the changes are additive):

```sql
-- Drop the new constraints/indexes
DROP INDEX IF EXISTS ix_lofn_categories_sibling_name_unique;
DROP INDEX IF EXISTS ix_lofn_categories_parent_id;
ALTER TABLE lofn_categories DROP CONSTRAINT IF EXISTS fk_lofn_category_parent;

-- Drop the column (loses any nesting data — irreversible from data POV)
ALTER TABLE lofn_categories DROP COLUMN IF EXISTS parent_id;

-- Optionally narrow slug back (only if no path-slugs were saved)
-- Caution: will fail if any row contains a slug longer than 120 chars.
-- ALTER TABLE lofn_categories ALTER COLUMN slug TYPE VARCHAR(120);
```

In application code, revert the relevant feature commit and rebuild. The system returns to the feature-001 behaviour.
