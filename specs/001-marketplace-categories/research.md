# Research — Marketplace category mode per tenant

**Feature**: `001-marketplace-categories`
**Date**: 2026-04-28
**Sources**: `spec.md` clarifications (Q1–Q5), existing source under `Lofn.*`, project conventions captured in `CLAUDE.md`

This document resolves every "how" left open after Phase 0 of the plan, with one *Decision / Rationale / Alternatives* entry per topic.

---

## R1. How is the `Marketplace` flag exposed to the application?

**Decision**: Extend `ITenantResolver` (in `Lofn.Domain.Interfaces`) with a new boolean property `Marketplace`. Implement it in `TenantResolver` (in `Lofn.Application`) by reading `Tenants:{TenantId}:Marketplace` from `IConfiguration` exactly the same way `ConnectionString`, `JwtSecret` and `BucketName` are already read. Default to `false` when the key is absent or unparsable.

**Rationale**:
- The clarification (Q3) committed the flag's home to `appsettings`; the existing `TenantResolver` already serves that block, so the feature follows the path of least surprise.
- Any consumer that wants the flag (`CategoryService`, `ProductService`, controllers, GraphQL queries, authorization filter) already injects `ITenantResolver` — no new DI wiring needed.
- `bool.TryParse` over `_configuration[$"Tenants:{TenantId}:Marketplace"]` gives us the safe default required by the malformed-value edge case.

**Alternatives considered**:
- *New `IMarketplaceContext` service.* Rejected — duplicates tenant-resolution responsibility; would need its own DI registration without adding clarity.
- *Strongly-typed `TenantSetting` POCO bound via `IOptionsSnapshot<>`.* Rejected — would force a refactor across `JwtSecret`/`ConnectionString` callsites that is out of scope.

---

## R2. How is "global" represented on the `Category` entity?

**Decision**: Use the **existing** nullable `Category.StoreId` column. `StoreId IS NULL` means tenant-global; `StoreId IS NOT NULL` means store-scoped. Add a unique partial index on `(Slug)` filtered by `StoreId IS NULL`, plus the existing per-store uniqueness, to enforce R6.

**Rationale**:
- `Category.StoreId` is already nullable in the schema (`Lofn.Infra/Context/Category.cs`) and in EF Core's model-building. No destructive migration required.
- A nullable FK is the canonical relational way to express "this row belongs to all stores within the tenant".
- Only one EF Core migration is needed (the unique index for slug uniqueness across globals).

**Alternatives considered**:
- *New `Scope` enum column (`Global` / `Store`).* Rejected — redundant with `StoreId IS NULL`, creates two sources of truth (a row with `Scope = Global` but `StoreId != NULL` would be an inconsistency the application would have to police).
- *Separate `tenant_categories` table.* Rejected — doubles every read query (`UNION` over two tables), forces every consumer to know which table to read.

---

## R3. How does the application authorize global-category mutations?

**Decision**: Implement an authorization filter — `MarketplaceAdminRequirement` (or simply an action filter / minimal endpoint policy) — applied to every action of `CategoryGlobalController` (the new dedicated controller). The filter rejects any request that does not satisfy **both**:

1. `userSession.IsAdmin == true` — extracted from the existing NAuth `IUserClient.GetUserInSession(HttpContext)`.
2. `tenantResolver.Marketplace == true` — read from the resolved tenant.

A failed check returns `403 Forbidden`. Anonymous requests still hit the existing `[Authorize]` first → `401 Unauthorized`.

**Rationale**:
- Centralizes the gate in one place; controllers stay free of inline `if (!isAdmin) return Forbid()` boilerplate.
- The filter receives both `IUserClient` and `ITenantResolver` via constructor injection, so it composes with the existing DI graph without changes.
- Differentiates 401 (unauthenticated) from 403 (authenticated but not authorised) — important for client UX and the corresponding API tests.

**Alternatives considered**:
- *Push the check inside `CategoryService` (`InsertGlobalAsync`, `UpdateGlobalAsync`, `DeleteGlobalAsync`).* Rejected — it works but mixes concerns; service layer would need direct access to `IUserClient` (it currently does not), expanding its surface.
- *ASP.NET `[Authorize(Policy = "MarketplaceAdmin")]` with a `IAuthorizationRequirement` + `AuthorizationHandler`.* Acceptable alternative; chosen approach is essentially the same pattern phrased without the policy abstraction. Either implementation is valid — picking whichever the existing codebase uses elsewhere reduces the learning curve. As of today, the codebase does not register any policies; we stay consistent with that and use a simple custom action filter.

---

## R4. How does the existing `CategoryController` behave when the tenant is in marketplace mode?

**Decision**: `POST /category/{storeSlug}/insert`, `POST /category/{storeSlug}/update`, and `DELETE /category/{storeSlug}/delete/{categoryId}` MUST return `403 Forbidden` whenever `tenantResolver.Marketplace == true`. The check happens at the start of each action (before any service call), so the user receives a fast, consistent error. Read paths exposed by GraphQL/REST for "list categories of a store" still respond, but the result set is filtered as described in R5.

**Rationale**:
- FR-014 explicitly requires writes on the store-scoped surface to be rejected in marketplace mode.
- Returning `403` (instead of `404` or `400`) keeps the behaviour symmetric with the new global controller — same semantics, opposite role gate.

**Alternatives considered**:
- *Return `404` to "hide" the endpoints in marketplace mode.* Rejected — confuses clients who legitimately discovered the URL; `403` is more honest.
- *Silently route to the global flow.* Rejected — would cross-pollute writes into the global namespace and bypass IsAdmin authorization.

---

## R5. What does "list categories" return in each mode?

**Decision**:

- **REST/GraphQL "list categories for store X" in non-marketplace tenant**: unchanged — categories `WHERE StoreId = X.StoreId`.
- **REST/GraphQL "list categories for store X" in marketplace tenant**: returns categories `WHERE StoreId IS NULL` (i.e. the tenant-global catalog). Pre-existing store-scoped categories of X are **not** returned.
- **GraphQL public `categories` root query**: unchanged signature; resolver decides via `ITenantResolver.Marketplace`. In marketplace mode it returns globals filtered by `Products.Any(p => p.Status == 1)` (preserves the current "category in use" filter); in non-marketplace it keeps today's behaviour.
- **No "merged" list is offered.** A marketplace tenant exposes globals only; legacy store-scoped categories become unreachable through the API (consistent with US1 acceptance #3 and Q1's outcome).

**Rationale**:
- Keeps each list response a single coherent answer to "what can a product in this store be classified as?". Mixing globals with legacy store categories in marketplace mode would violate FR-006 (the validator would still reject the legacy ones).
- Legacy data stays in the database (per FR-010) but is hidden from the API surface, in line with the spec's edge case.

**Alternatives considered**:
- *Return both globals and legacy store-scoped in a merged list with a flag.* Rejected — confusing UX, encourages clients to display invalid choices.
- *Return globals plus only those legacy categories that still have at least one product attached.* Rejected — operates at odds with FR-006, which would refuse those legacy ids on subsequent product write.

---

## R6. How is tenant-wide slug uniqueness enforced?

**Decision**: Two layers, defence-in-depth.

1. **Database**: PostgreSQL unique partial indexes on `lofn_categories(slug)` — one filtered `WHERE store_id IS NOT NULL` (already implicit per existing per-store uniqueness, kept) and one filtered `WHERE store_id IS NULL` (new, introduced by the EF migration). Together they prevent two globals with the same slug, and prevent two store-scoped categories within the same store from clashing — but they **do not** by themselves enforce "unique across globals + every store of the same tenant" because each tenant already owns its own database (multi-tenant DB-per-tenant), which means uniqueness within the tenant boundary is automatically equivalent to uniqueness within the database. Tenant-wide uniqueness therefore reduces to **database-wide uniqueness** of the `slug` column for new rows.
2. **Service**: `CategoryService.GenerateSlugAsync` — currently checks `_categoryRepository.ExistSlugAsync(storeId, categoryId, newSlug)` scoped to the store. Replaced with `ExistSlugInTenantAsync(categoryId, newSlug)` that looks up *any* category record (global or store-scoped) with the same slug and a different `CategoryId`. The service auto-suffixes (`eletronicos`, `eletronicos1`, …) until a free slug is found, exactly as today.

The "grandfathered" clause of FR-015 is satisfied at the DB layer too: the new partial unique index is created `IF NOT EXISTS` style and does **not** touch existing rows. If pre-existing data has duplicates, the index creation will fail; the migration script SHOULD therefore use `CREATE UNIQUE INDEX` only when feasible and fall back to `CREATE INDEX` (non-unique) when duplicates exist, leaving service-layer validation as the only enforcement point. The migration documentation explicitly tells operators to either accept the non-unique fallback or pre-deduplicate before running.

**Rationale**:
- DB-per-tenant means the natural per-DB unique index already gives us tenant-wide uniqueness "for free" — no special composite key is needed.
- Catching the conflict in the service layer surfaces a clear validation error to the user (FR-015 edge case) instead of a raw `DbUpdateException`.

**Alternatives considered**:
- *Single unique index on `(slug)` across the whole table.* Rejected — would fail on legacy data with duplicates between stores; the partial-index approach is the safe upgrade path.
- *Check uniqueness only at the service layer, no DB constraint.* Rejected — leaves data integrity to application code that can be bypassed (e.g., direct SQL during admin migration).

---

## R7. How does product validation know which categories are valid?

**Decision**: `ProductService.InsertAsync` and `UpdateAsync` resolve the tenant's mode via `ITenantResolver.Marketplace` and apply a different category check:

- `Marketplace == true`: the requested `CategoryId`, if non-null, MUST resolve to a category with `StoreId IS NULL`.
- `Marketplace == false`: the requested `CategoryId`, if non-null, MUST resolve to a category with `StoreId == product.StoreId` (the current behaviour, made explicit).

A `null` `CategoryId` remains valid in either mode (products may exist without a category, as today, and as required by the post-flip transition described in spec edge case 1).

**Rationale**:
- Centralises the validation in the service layer where business rules already live, avoiding duplication between REST and GraphQL paths.
- A single helper (`AssertCategoryAllowedAsync(categoryId, storeId)`) is enough; both Insert and Update call it.

**Alternatives considered**:
- *Implement the check in a FluentValidation validator on `ProductInsertInfo`/`ProductUpdateInfo`.* Rejected — the validator would need to inject `ITenantResolver` and `ICategoryRepository`, breaking the project's convention that validators are pure (no DB access). The existing validators today live alongside service logic for richer rules; we follow that line.
- *Database CHECK constraint correlating `Product.StoreId` and `Category.StoreId`.* Rejected — Postgres CHECK constraints can't reference rows of another table; would require a trigger, adding maintenance cost.

---

## R8. Where do tests for the new flow live?

**Decision**:

- **Unit tests** in `Lofn.Tests` (xUnit) cover service-layer branches of `CategoryService` (insert / update / delete in marketplace mode, slug generation across both scopes) and the new product-validation branch in `ProductService`.
- **Integration tests** in `Lofn.ApiTests` cover the new `CategoryGlobalController` (CRUD happy-path + 401/403 gates) and the rejection of writes on the legacy `CategoryController` when `Marketplace = true`.
- **GraphQL integration coverage** is added to `Lofn.ApiTests` as a small fixture call that runs each public/admin query and asserts that `categories` reflects the active mode.

**Rationale**:
- The skill documents already split unit (`dotnet-test`) and external API tests (`dotnet-test-api`); we follow that boundary.
- The current `Lofn.ApiTests/Controllers/CategoryControllerTests.cs` only covers 401 gates; adding marketplace-mode 403 assertions extends what is already there.

**Alternatives considered**:
- *In-process WebApplicationFactory tests instead of external HTTP tests.* Rejected — the project already standardises on external HTTP tests for integration; in-process tests would split the suite without benefit.

---

## R9. Migration script naming and workflow

**Decision**:

- One EF Core migration named `20260428_AddGlobalCategoryUniqueIndex`. It runs `CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_categories_slug_global ON lofn_categories (slug) WHERE store_id IS NULL;`.
- No data-rewrite migration (per Q5 — admins handle that).
- Migrations are applied per-tenant DB through the existing deploy process. Operators read the *Migration playbook* in `quickstart.md` before running.

**Rationale**:
- Keeps schema changes minimal and explicit.
- Consistent with the existing migration approach in `Lofn.Infra/Migrations/`.

**Alternatives considered**:
- *Auto-apply migrations at startup with `dbContext.Database.Migrate()`.* Rejected — multi-tenant means the API would need to iterate every tenant DB on startup; current deployment doesn't do this and the feature shouldn't introduce that pattern.

---

## R10. Backward compatibility checklist

**Decision**: The following invariants must hold after the feature ships, verified by automated tests:

| Invariant | How verified |
|---|---|
| Tenant `emagine` and `monexup` keep `Marketplace = false` (existing behaviour) | `appsettings.*` PRs; integration test against `emagine` tenant in CI |
| Anonymous `POST /graphql` returns categories of a given store unchanged | `Lofn.ApiTests` GraphQL smoke test |
| `POST /category/{storeSlug}/insert` continues to work for store admins on non-marketplace tenants | Existing test passes unchanged |
| Existing `Category.Slug` rows with same slug across two stores in the same DB stay readable | Migration uses `IF NOT EXISTS` and unique index with partial filter — pre-existing rows are unaffected |
| Existing products with non-null `CategoryId` keep their category in non-marketplace tenants | Smoke read on production-like seed data |

**Rationale**: The feature spec's SC-004 demands no observable change for non-marketplace tenants. This table is the explicit acceptance check.

**Alternatives considered**: None — backward compatibility is a hard requirement.

---

## Open questions (none)

All clarifications from `/speckit.clarify` are resolved. No items carried into the planning phase.
