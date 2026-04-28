# Data Model — Marketplace category mode per tenant

**Feature**: `001-marketplace-categories`
**Date**: 2026-04-28

This document captures the entity-level changes the feature introduces. It is the contract between the spec/research and the EF migrations / repository layer.

## Entities

### Tenant (logical, no row in any DB)

A tenant is represented by a key in `appsettings.Tenants:{tenantId}`. The feature adds one optional property:

| Field | Type | Default | Notes |
|---|---|---|---|
| `Marketplace` | bool | `false` | When `true`, categories within this tenant are tenant-global and only `IsAdmin = true` users may manage them. |

Persisted in `appsettings.json` (`appsettings.Production.json`, `appsettings.Docker.json`, `appsettings.Development.json`). Surfaced to the application via `ITenantResolver.Marketplace`.

### Category (existing entity, schema clarified)

Existing table: `lofn_categories`. Existing columns kept; **no destructive change**.

| Column | Type | Nullable | New? | Notes |
|---|---|---|---|---|
| `category_id` | bigserial PK | no | no | unchanged |
| `slug` | varchar | no | no | unchanged. **New uniqueness rule** — see *Indexes & Constraints*. |
| `name` | varchar | no | no | unchanged |
| `store_id` | bigint FK → `lofn_stores(store_id)` | **yes** | no | **Semantic change**: `NULL` now means tenant-global; non-null means store-scoped (existing behaviour). |

### Product (existing entity, validation clarified)

Existing table: `lofn_products`. No column added. Validation logic changes only:

- `category_id` (nullable FK → `lofn_categories(category_id)`) MUST resolve to a category whose `store_id` matches the rule below:
  - In a tenant with `Marketplace = true`: target row MUST have `store_id IS NULL`.
  - In a tenant with `Marketplace = false`: target row MUST have `store_id = lofn_products.store_id` (current behaviour).
  - `category_id IS NULL` is allowed in either mode.

### Store (existing entity, unchanged)

No fields added or removed. Stores are agnostic to whether the tenant they belong to is a marketplace.

### User (existing entity in NAuth, unchanged)

No fields added. `IsAdmin` (already provided by the NAuth `UserInfo`) is the only attribute consulted.

## Indexes & Constraints

### New: unique index on global slugs

```sql
CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_categories_slug_global
    ON lofn_categories (slug)
    WHERE store_id IS NULL;
```

- Enforces FR-015 across globals (no two tenant-global categories with the same slug).
- Combined with the existing per-store uniqueness (already enforced by `CategoryService.GenerateSlugAsync` and the existing schema), this gives database-level tenant-wide uniqueness for new rows. Tenant-wide uniqueness reduces to DB-wide uniqueness because each tenant has its own database (DB-per-tenant).

### Grandfathered legacy data

- Pre-existing rows in any tenant DB are not migrated.
- If a tenant's DB already contains two store-scoped categories with the same slug (legitimate today), they remain valid; the new index does not affect them because both have non-null `store_id`.
- If a tenant's DB already contains two rows with `store_id IS NULL` and the same slug (rare, but possible from manual seed scripts), the migration MUST be run with the unique index conditional. Operators are expected to either deduplicate first or fall back to a non-unique index — see `quickstart.md` migration playbook.

## State Transitions

The `Category` lifecycle is unchanged: rows are inserted, updated (slug regenerated from new name), and hard-deleted (with products' `category_id` nulled by EF's `OnDelete = ClientSetNull`).

The new state to model is the **tenant-level transition** of `Marketplace`:

```
              flag flips false → true                      flag flips true → false
                    (deploy + restart)                          (deploy + restart)
   ┌────────────────────┐                            ┌────────────────────┐
   │ Marketplace = false│ ◄────────────────────────► │ Marketplace = true │
   └────────────────────┘                            └────────────────────┘
        │                                                        │
        │ Stores' admins manage their store-scoped               │ Only IsAdmin manages tenant-global
        │ categories. Legacy data unchanged.                     │ categories. Legacy store-scoped
        │                                                        │ categories invisible from API.
```

No automatic data-rewrite ever runs on either transition (FR-010 + Q5). Admins are expected to nullify legacy `Product.CategoryId` themselves, off-band, when entering marketplace mode.

## Validation Rules (recap)

| Rule | Source | Enforced where |
|---|---|---|
| `Marketplace` flag default = `false` | FR-001, R1 | `TenantResolver.Marketplace` getter |
| Only `IsAdmin = true` can mutate global categories | FR-002, FR-013, R3 | `MarketplaceAdminRequirement` filter on `CategoryGlobalController` |
| Store-scoped writes rejected when `Marketplace = true` | FR-014, R4 | early `403` check in `CategoryController` |
| Slug unique across categories of the same tenant (db-per-tenant) | FR-015, R6 | service-layer pre-check + DB unique partial index |
| Product category must match tenant mode | FR-006, FR-007, R7 | `ProductService.AssertCategoryAllowedAsync` helper called on insert/update |
| Listing rules per mode | R5 | `CategoryService` and GraphQL resolvers branch on `Marketplace` |

## Domain glossary

- **Marketplace tenant**: a tenant whose `Tenants:{slug}:Marketplace` is `true`.
- **Global category** (a.k.a. *tenant-global category*): a `Category` row with `StoreId IS NULL`.
- **Store-scoped category** (a.k.a. *legacy* in the context of a marketplace transition): a `Category` row with `StoreId IS NOT NULL`.
- **Platform admin**: a user whose NAuth `UserInfo.IsAdmin = true`.
- **Store admin**: a user authenticated against the store, without `IsAdmin`. Their authority is store ownership (`StoreService.GetByIdAsync` + `OwnerId == userId`), unchanged.
