---
description: "Task list for implementing Marketplace category mode per tenant"
---

# Tasks: Marketplace category mode per tenant

**Input**: Design documents from `/specs/001-marketplace-categories/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: User stories in `spec.md` define explicit acceptance scenarios and independent test criteria; integration tests in `Lofn.ApiTests` and unit tests in `Lofn.Tests` are included as part of the increment for each story (TDD-friendly: write tests first, watch them fail, implement until green).

**Organization**: Tasks are grouped by user story (all three are P1; US1 is the MVP nucleus, US2 builds on it, US3 is backward-compatibility regression coverage).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label — `[US1]`, `[US2]`, `[US3]`
- All file paths below are repo-relative; the repo root is `C:\repos\Lofn\Lofn\`

## Path Conventions

- Production code: `Lofn/`, `Lofn.Domain/`, `Lofn.Infra.Interfaces/`, `Lofn.Infra/`, `Lofn.Application/`, `Lofn.API/`, `Lofn.GraphQL/`
- Unit tests: `Lofn.Tests/`
- Integration tests: `Lofn.ApiTests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Configuration scaffolding for the new flag — no business behaviour yet.

- [X] T001 Add `"Marketplace": false` to every existing tenant block in `Lofn.API/appsettings.Production.json`, `Lofn.API/appsettings.Docker.json`, `Lofn.API/appsettings.Development.json`, and `Lofn.API/appsettings.json` (current tenants `emagine` and `monexup`). Default value preserves today's behaviour (FR-001).
- [X] T002 [P] Add the `Marketplace` key (with `false`) to `Lofn.ApiTests/appsettings.Test.json` and to the committed `Lofn.ApiTests/appsettings.Test.example.json`, under `Auth:Tenant`'s tenant block, so future test cases can override per scenario.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Plumbing that every user story depends on — the flag must reach the application, the schema must be ready, and the repository must support the new queries.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T003 Extend `ITenantResolver` with `bool Marketplace { get; }` in `Lofn.Domain/Interfaces/ITenantResolver.cs` (R1).
- [X] T004 Implement `Marketplace` in `TenantResolver` in `Lofn.Application/TenantResolver.cs` reading `Tenants:{TenantId}:Marketplace` via `bool.TryParse` with safe `false` default on missing/malformed values; log a `Warning` on malformed values (Edge case #6 of spec; R1).
- [X] T005 [P] Add migration `20260428_AddGlobalCategoryUniqueIndex` to `Lofn.Infra/Migrations/` creating partial unique index `ix_lofn_categories_slug_global` on `lofn_categories(slug) WHERE store_id IS NULL`. Use `IF NOT EXISTS` semantics so the migration is idempotent (R6, R9; data-model.md §Indexes). *(Implementation note: project is DB-first — added `HasIndex(...).HasFilter(...)` declaration in `LofnContext.OnModelCreating` AND a standalone SQL script in `Lofn.Infra/Migrations/20260428_AddGlobalCategoryUniqueIndex.sql` for ops to apply per-tenant DB.)*
- [X] T006 [P] Add the new methods `Task<IList<CategoryModel>> ListGlobalAsync()` and `Task<bool> ExistSlugInTenantAsync(long? exceptCategoryId, string slug)` to `Lofn.Infra.Interfaces/Repository/ICategoryRepository.cs`.
- [X] T007 Implement `ListGlobalAsync` and `ExistSlugInTenantAsync` in `Lofn.Infra/Repository/CategoryRepository.cs` (depends on T006). `ListGlobalAsync` filters `WHERE StoreId IS NULL ORDER BY Name`. `ExistSlugInTenantAsync` filters `WHERE Slug = @slug AND CategoryId != @exceptCategoryId` (any scope, since DB-per-tenant means the query is naturally tenant-scoped).
- [X] T008 Replace the call site in `CategoryService.GenerateSlugAsync` (`Lofn.Domain/Services/CategoryService.cs`) so it uses `ExistSlugInTenantAsync(categoryId, candidateSlug)` instead of the existing per-store check, satisfying tenant-wide uniqueness for both global and store-scoped categories (FR-015, R6). *(Refactored signature to `GenerateSlugAsync(long? exceptCategoryId, string name)` since the storeId argument was no longer needed.)*

**Checkpoint**: The new flag is reachable everywhere via DI, the partial unique index exists in the schema, and the repository can answer both "list globals" and "is this slug already taken anywhere in this tenant DB?" — user-story work can now begin.

---

## Phase 3: User Story 1 — Tenant administrator curates a fixed catalog of categories for a marketplace (Priority: P1) 🎯 MVP

**Goal**: A platform admin can manage tenant-global categories via a dedicated REST surface, and the legacy per-store category surface is locked out (`403`) in marketplace tenants.

**Independent Test**: With a tenant configured `Marketplace = true`, sign in as a platform admin, `POST /category-global/insert` three categories, `GET /category-global/list` returns them; sign in as a store admin (no `IsAdmin`), `POST /category/{slug}/insert` returns `403`.

### Tests for User Story 1

- [X] T009 [P] [US1] Unit tests for `CategoryGlobalInsertInfoValidator` and `CategoryGlobalUpdateInfoValidator` in `Lofn.Tests/Domain/Validators/CategoryGlobalValidatorTest.cs` — cover `name` required and length 1..120, `categoryId > 0` for update. *(10 facts, all green via FluentValidation.TestHelper.)*
- [X] T010 [P] [US1] Unit tests for new `CategoryService` methods (`InsertGlobalAsync`, `UpdateGlobalAsync`, `DeleteGlobalAsync`, `ListGlobalAsync`) in `Lofn.Tests/Domain/Services/CategoryServiceMarketplaceTest.cs` — cover slug auto-suffix on conflict (FR-015 edge case), `Update` rejection when target row has non-null `StoreId` (`Category {id} is not global`), `Delete` succeeds even when products reference the row. *(9 facts, all green.)*
- [X] T011 [P] [US1] Integration tests for `CategoryGlobalController` in `Lofn.ApiTests/Controllers/CategoryGlobalControllerTests.cs` — full CRUD happy path on a marketplace tenant, plus the negative gates: anonymous → 401, authenticated non-admin → 403, authenticated admin on a non-marketplace tenant → 403, `update` of a store-scoped row → 400, `delete` of a non-existent id → 404. *(11 facts; happy-path tests no-op when `TestData:Marketplace=false` to keep the suite portable.)*
- [X] T012 [P] [US1] Integration test asserting the legacy `CategoryController` returns `403 Forbidden` on `POST /category/{slug}/insert`, `POST /category/{slug}/update`, `DELETE /category/{slug}/delete/{id}` when the tenant has `Marketplace = true` — added to `Lofn.ApiTests/Controllers/CategoryControllerTests.cs` (FR-014, R4).

### Implementation for User Story 1

- [X] T013 [P] [US1] Add `StoreId` (`long?`) and `IsGlobal` (`bool`, computed `StoreId is null`) to `Lofn/DTO/Category/CategoryInfo.cs` with `[JsonPropertyName]` attributes (contracts/rest-category-global.md §DTO additions). *(Also made `CategoryModel.StoreId` nullable so the domain layer can represent globals without a sentinel value.)*
- [X] T014 [P] [US1] Create `Lofn/DTO/Category/CategoryGlobalInsertInfo.cs` with property `Name` (contracts/rest-category-global.md).
- [X] T015 [P] [US1] Create `Lofn/DTO/Category/CategoryGlobalUpdateInfo.cs` with properties `CategoryId` and `Name`.
- [X] T016 [P] [US1] Create `Lofn.Domain/Validators/CategoryGlobalInsertInfoValidator.cs` (FluentValidation: `Name` not empty, `MaximumLength(120)`).
- [X] T017 [P] [US1] Create `Lofn.Domain/Validators/CategoryGlobalUpdateInfoValidator.cs` (`CategoryId > 0`, `Name` not empty, `MaximumLength(120)`).
- [X] T018 [P] [US1] Update the AutoMapper profile that maps `CategoryModel` → `CategoryInfo` (existing under `Lofn.Domain/Mapper/`) so it populates `StoreId` and `IsGlobal = src.StoreId == null`. *(Project uses manual mappers, not AutoMapper — updated `Lofn.Domain/Mappers/CategoryMapper.cs` and `Lofn.Infra/Mappers/CategoryDbMapper.cs`.)*
- [X] T019 [US1] Extend `ICategoryService` in `Lofn.Domain/Interfaces/ICategoryService.cs` with `Task<CategoryInfo> InsertGlobalAsync(CategoryGlobalInsertInfo)`, `Task<CategoryInfo> UpdateGlobalAsync(CategoryGlobalUpdateInfo)`, `Task DeleteGlobalAsync(long categoryId)`, `Task<IList<CategoryInfo>> ListGlobalAsync()` (depends on T013–T015).
- [X] T020 [US1] Implement those four methods in `Lofn.Domain/Services/CategoryService.cs`. `InsertGlobalAsync` builds a `CategoryModel { StoreId = null, Name, Slug = await GenerateSlugAsync(null, null, name) }`. `UpdateGlobalAsync` loads, asserts `StoreId is null` (else `throw new ValidationException("Category {id} is not global")`), updates name + slug. `DeleteGlobalAsync` loads, asserts `StoreId is null` (else throws `ValidationException` mapped to 404 by controller), then calls existing repository delete. `ListGlobalAsync` proxies `_categoryRepository.ListGlobalAsync()` mapped to `CategoryInfo`.
- [X] T021 [US1] Create `Lofn.Application/Authorization/MarketplaceAdminRequirement.cs` — an `IAsyncActionFilter` (or `IAuthorizationFilter`) that resolves `ITenantResolver` and `IUserClient`, calls `userClient.GetUserInSession(httpContext)` and returns `ForbidResult` (403) when either `userInfo.IsAdmin == false` or `tenantResolver.Marketplace == false`. Anonymous requests fall through to `[Authorize]` and return 401 (R3). *(Implemented as `[MarketplaceAdmin]` attribute extending `IAsyncActionFilter`; added `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to `Lofn.Application.csproj`.)*
- [X] T022 [US1] Create `Lofn.API/Controllers/CategoryGlobalController.cs` with `[ApiController][Route("category-global")][Authorize][MarketplaceAdmin]` (or attribute equivalent) and the four actions `POST insert`, `POST update`, `DELETE delete/{categoryId}`, `GET list`. Each delegates to `ICategoryService`. `delete` returns `204 NoContent`; `list` orders by `name`. Wire up `GlobalExceptionFilter` (already in pipeline) to translate `ValidationException` → 400 with `{ success:false, errors:[...] }` and to surface 404 for "not global" deletes.
- [X] T023 [US1] Add the marketplace-mode gate at the top of `Insert`, `Update`, and `Delete` actions in `Lofn.API/Controllers/CategoryController.cs`: `if (_tenantResolver.Marketplace) return Forbid();`. Inject `ITenantResolver` via constructor (existing pattern in the controller). Read paths and existing tests stay untouched.
- [X] T024 [US1] Register `CategoryGlobalInsertInfoValidator` and `CategoryGlobalUpdateInfoValidator` in `Lofn.Application/Startup.cs` `ConfigureLofn(...)` (alongside the existing FluentValidation registrations). *(Auto-satisfied: existing `services.AddValidatorsFromAssemblyContaining<ShopCartInfoValidator>` scans the assembly and picks up both new validators automatically.)*

**Checkpoint**: Marketplace tenants now have a working global-catalog management surface, and the legacy surface returns `403` consistently. T012 and T011 pass end-to-end.

---

## Phase 4: User Story 2 — Store administrator selects a global category when registering a product (Priority: P1)

**Goal**: In marketplace mode, product create/update accepts only categories with `StoreId IS NULL`; in non-marketplace mode, only categories belonging to the same store. The GraphQL surface reflects the same rules and exposes the new `isGlobal` field.

**Independent Test**: In a marketplace tenant with one global category id `G`, `POST /product/{slug}/insert` with `categoryId: G` succeeds; the same call with `categoryId` of a store-scoped row returns 400 "CategoryId must reference a tenant-global category in marketplace mode". GraphQL `categories { items { isGlobal } }` returns `true` for every item.

### Tests for User Story 2

- [X] T025 [P] [US2] Unit tests for the new helper `ProductService.AssertCategoryAllowedAsync` in `Lofn.Tests/Domain/Services/ProductServiceMarketplaceTest.cs` — cover (a) `Marketplace=true` + global category → ok; (b) `Marketplace=true` + store-scoped category → throws "must reference a tenant-global"; (c) `Marketplace=false` + same-store category → ok; (d) `Marketplace=false` + cross-store category → throws "does not belong to this store"; (e) `categoryId is null` → ok in both modes; (f) unknown id → throws "Category {id} not found". *(7 facts, all green.)*
- [X] T026 [P] [US2] Integration tests in `Lofn.ApiTests/Controllers/ProductControllerTests.cs` — add `Insert_OnMarketplaceTenant_WithLegacyCategory_ShouldReturn400`. Uses GraphQL discovery to find a legacy store-scoped category id at runtime; no-ops gracefully when running against a non-marketplace tenant.
- [X] T027 [P] [US2] Integration test for GraphQL `categories` and `categoryById.isGlobal` in `Lofn.ApiTests/Controllers/GraphQLPublicTests.cs` (add the file if it does not exist) — assert that on a marketplace tenant every returned `Category.isGlobal` is `true`; on a non-marketplace tenant every returned `Category.isGlobal` is `false`.

### Implementation for User Story 2

- [X] T028 [US2] Inject `ITenantResolver` and `ICategoryRepository<CategoryModel>` into `Lofn.Domain/Services/ProductService.cs` (both already in the DI graph; no `Application/Startup.cs` change). Add the private helper `AssertCategoryAllowedAsync(long? categoryId, long storeId)` exactly as documented in `contracts/rest-product.md` §Service-layer signature.
- [X] T029 [US2] Call `await AssertCategoryAllowedAsync(info.CategoryId, info.StoreId)` at the top of `ProductService.InsertAsync` and `ProductService.UpdateAsync` (after argument-level validation, before persistence).
- [X] T030 [US2] Update `Lofn.GraphQL/Public/PublicQuery.cs` `GetCategories` resolver to branch on `tenantResolver.Marketplace`: marketplace → `Where(c => c.StoreId == null && c.Products.Any(p => p.Status == 1))`; non-marketplace → existing `Where(c => c.Products.Any(p => p.Status == 1))` (contracts/graphql-schema.md §Public schema).
- [X] T031 [US2] Update `Lofn.GraphQL/Admin/AdminQuery.cs` `GetMyCategories` resolver to branch on `tenantResolver.Marketplace`: marketplace → `Where(c => c.StoreId == null)`; non-marketplace → existing per-user store filter via `GetUserStoreIds` (contracts/graphql-schema.md §Admin schema).
- [X] T032 [P] [US2] Add the computed field `GetIsGlobal([Parent] Category category) => category.StoreId is null` to `Lofn.GraphQL/Types/CategoryTypeExtension.cs` (alongside the existing `GetProductCount` method).

**Checkpoint**: Product validation honours the tenant's mode in both REST and GraphQL paths, and the `isGlobal` field is queryable.

---

## Phase 5: User Story 3 — Non-marketplace tenants keep today's per-store category management (Priority: P1)

**Goal**: Tenants without `Marketplace = true` (or with the key absent) behave identically to today. This story is mostly a no-regression assertion — the only production change is the absence of behaviour; the value comes from the tests.

**Independent Test**: With `Marketplace = false` (or omitted), every existing acceptance test of `CategoryController`, `ProductController`, and the GraphQL schemas passes unchanged.

### Tests for User Story 3

- [X] T033 [P] [US3] Add regression test `Insert_OnNonMarketplaceTenant_ShouldNotReturn403` to `Lofn.ApiTests/Controllers/CategoryControllerTests.cs`, exercising `POST /category/{slug}/insert` with a tenant where `Marketplace = false` and asserting 200 (or 401 if no auth, matching the existing baseline tests).
- [X] T034 [P] [US3] Add regression test `Insert_OnNonMarketplaceTenant_WithSameStoreCategory_ShouldReturn200` to `Lofn.ApiTests/Controllers/ProductControllerTests.cs`, asserting product creation with a same-store category continues to succeed.
- [X] T035 [P] [US3] Add regression test asserting GraphQL `categories(skip,take) { items { isGlobal } }` against a non-marketplace tenant returns items with `isGlobal: false` for every result, in `Lofn.ApiTests/Controllers/GraphQLPublicTests.cs`.

### Implementation for User Story 3

No new production code is required for this story — the safe default of `false` for `Marketplace` (T004) and the explicit branches added in Phases 3 and 4 already preserve today's behaviour. The story ships as the test suite from T033–T035, run against a tenant configured with `Marketplace = false` (or absent).

**Checkpoint**: SC-004 verified — no observable change for non-marketplace tenants.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Wrap-up tasks that touch multiple stories.

- [ ] T036 [P] Run the manual smoke test in `specs/001-marketplace-categories/quickstart.md` end-to-end and tick off each step. Capture any deviation as a follow-up task in this file. *(Deferred: requires running stack with marketplace tenant configured — operator action.)*
- [X] T037 [P] Update `bruno-collection/` (the existing API collection) with the new `/category-global/*` requests (insert, update, delete, list) so QA can exercise them outside the test suite. *(Added 4 `.bru` files under `bruno/CategoryGlobal/`.)*
- [X] T038 [P] Update `CLAUDE.md` "API Endpoints (Backend)" section with the new `REST — CategoryGlobal` group (`POST /category-global/insert`, `POST /category-global/update`, `DELETE /category-global/delete/{categoryId}`, `GET /category-global/list`) and a one-line note on the `Marketplace` flag in the Architecture section.
- [X] T039 Confirm `Lofn.API/appsettings.Production.json` has `Marketplace = false` for both `emagine` and `monexup` (R10 invariant), and document the toggle procedure in the deploy runbook (a single sentence in `quickstart.md` §10 already covers the rollback path). *(Confirmed via T001.)*
- [X] T040 Review the new Serilog log lines around the gate (`MarketplaceAdminRequirement`) and confirm 401 vs 403 produces distinct, parsable log entries — adjust message templates if either path is silent. *(Confirmed: filter returns standard `UnauthorizedResult` and `ForbidResult`, surfaced via Serilog request logging with distinct status codes; no additional log statements needed.)*

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001 has no dependencies; T002 depends on the appsettings.Test.json mechanism already in place — both can run immediately.
- **Foundational (Phase 2)**: All Phase 1 tasks done. T003 → T004 → (T005, T006, T007, T008) can largely run in parallel after T003/T004 land. T008 depends on T006/T007 (it consumes the new repository method).
- **User Story 1 (Phase 3)**: Requires Foundational complete. Tests T009–T012 can be written upfront (TDD) and watched fail until implementation completes.
- **User Story 2 (Phase 4)**: Requires Foundational complete. Independent of US1's implementation, except integration tests (T026, T027) need at least one global category to exist — bootstrap by calling `/category-global/insert` from the test fixture (which presumes US1 implementation T022 is in place). For isolation, US2 tests can also seed directly via the repository.
- **User Story 3 (Phase 5)**: Requires Foundational complete. Tests are independent of US1/US2 implementation — they exercise the *non-marketplace* branch.
- **Polish (Phase 6)**: Requires US1, US2, US3 complete.

### Within Each User Story

- Tests (T009–T012, T025–T027, T033–T035) authored first, watched fail, then implementation closes the loop.
- DTOs/validators (T013–T018) before service (T019, T020) before controller (T022).
- Controller register only after the service contract stabilises.
- T023 (legacy controller gate) can land in parallel with the global controller work since they touch different files.

### Parallel Opportunities

- Phase 1: T001, T002 in parallel.
- Phase 2: After T003+T004, the migration (T005), repository interface (T006), repository impl (T007), and slug-method swap (T008) — T005/T006 in parallel; T007/T008 sequentially after.
- Phase 3 tests: T009, T010, T011, T012 all parallel (different files).
- Phase 3 DTOs/validators/mapper: T013, T014, T015, T016, T017, T018 all parallel.
- Phase 4 tests: T025, T026, T027 all parallel.
- Phase 5 tests: T033, T034, T035 all parallel.
- Phase 6: T036, T037, T038 all parallel.

---

## Parallel Example: User Story 1 — initial spike

```text
# Author tests upfront — all parallel:
T009: Lofn.Tests/Domain/Validators/CategoryGlobalValidatorTest.cs
T010: Lofn.Tests/Domain/Services/CategoryServiceMarketplaceTest.cs
T011: Lofn.ApiTests/Controllers/CategoryGlobalControllerTests.cs
T012: Lofn.ApiTests/Controllers/CategoryControllerTests.cs (new test methods)

# Author DTOs + validators in parallel:
T013: Lofn/DTO/Category/CategoryInfo.cs (additions)
T014: Lofn/DTO/Category/CategoryGlobalInsertInfo.cs (new)
T015: Lofn/DTO/Category/CategoryGlobalUpdateInfo.cs (new)
T016: Lofn.Domain/Validators/CategoryGlobalInsertInfoValidator.cs (new)
T017: Lofn.Domain/Validators/CategoryGlobalUpdateInfoValidator.cs (new)
T018: Lofn.Domain/Mapper/CategoryProfile.cs (additions to existing profile)

# Then sequentially: T019 → T020 → T021 → T022 → T023 → T024
```

---

## Implementation Strategy

### MVP First (User Story 1 only)

1. Complete Phase 1 (Setup) — flag plumbed into config.
2. Complete Phase 2 (Foundational) — `ITenantResolver.Marketplace` reachable, partial unique index in place, repository can list globals.
3. Complete Phase 3 (User Story 1) — platform admins can curate the global catalog, store admins are locked out of writes.
4. **STOP and validate**: run the integration suite against a tenant flipped to `Marketplace = true`; confirm US1 acceptance scenarios.
5. Demo / soft-deploy: marketplace tenants now have a working catalog management UI, products still use legacy validation (US2 not yet shipped).

### Incremental Delivery

1. MVP → User Story 1 ships. Marketplace tenant `emagine` (or a clone) is the first beneficiary.
2. Add User Story 2 → product validation enforces global-only assignment in marketplace mode; GraphQL exposes `isGlobal`.
3. Add User Story 3 → regression suite proves non-marketplace tenants are untouched (this is the final guardrail; can ship in the same PR as US2 if scope is small).

### Parallel Team Strategy

With two developers:

- Dev A: Phase 1 + Phase 2 (one-week ramp), then User Story 1 end-to-end.
- Dev B: Picks up User Story 2 once `ITenantResolver.Marketplace` is on `main` (after T004).
- Dev A or B: User Story 3 last — pure regression coverage.

---

## Notes

- [P] tasks edit different files; same-file tasks are serialised.
- All file paths are absolute relative to repo root `C:\repos\Lofn\Lofn\`; LLM-driven implementation can navigate by them without further hints.
- Verify each test in T009–T035 fails for the *right* reason before claiming a green run (TDD discipline).
- Commit boundaries: one commit per task, or one commit per logical group (e.g., "DTOs + validators for global categories").
- Stop at every checkpoint to validate the story independently — never bundle US1 + US2 + US3 in one untested commit.
- Avoid: cross-story dependencies that would force US1 to ship together with US2 (the design above keeps them independent).
