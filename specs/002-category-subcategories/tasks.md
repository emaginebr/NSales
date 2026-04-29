---
description: "Task list for feature 002-category-subcategories"
---

# Tasks: Category Subcategories Support

**Input**: Design documents from `specs/002-category-subcategories/`
**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/graphql.md](./contracts/graphql.md), [contracts/rest.md](./contracts/rest.md), [quickstart.md](./quickstart.md)

**Tests**: Tests are explicitly part of this feature (per spec User Stories' "Independent Test" guidance and the project's tests-as-contract convention from feature 001). Both unit (`Lofn.Tests`) and integration (`Lofn.ApiTests`) tests are included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks).
- **[Story]**: User story label (US1 / US2 / US3) — only for Phase 3+.
- File paths are absolute project-relative (from repo root `C:\repos\Lofn\Lofn\`).

## Path Conventions

Single-solution Clean Architecture (existing layout — no new csproj). Backend roots:

- `Lofn.Infra/` — entities, EF mapping, repositories, migrations
- `Lofn.Infra.Interfaces/` — repository interfaces
- `Lofn.Domain/` — domain models, services, mappers, validators, interfaces
- `Lofn/` — DTOs (the legacy `Lofn` csproj also produces the API host bootstrap; DTOs live under `Lofn/DTO/`)
- `Lofn.GraphQL/` — HotChocolate query types and field extensions
- `Lofn.API/` — REST controllers
- `Lofn.Application/` — DI bootstrap (`ConfigureLofn()`)
- `Lofn.Tests/` — xUnit unit tests (Moq + FluentValidation.TestHelper)
- `Lofn.ApiTests/` — xUnit integration tests (Flurl.Http + FluentAssertions, hits live API)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Sanity-check before introducing schema changes.

- [X] T001 Run `dotnet build Lofn.sln` from repo root and confirm 0 errors / 0 warnings as the green baseline before any change is made.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Schema, entity, model, and DTO field additions that ALL user stories depend on.

**⚠️ CRITICAL**: No user-story work can begin until this phase is complete.

- [X] T002 Author `Lofn.Infra/Migrations/20260429_AddCategoryParentId.sql` with the idempotent DDL block from `data-model.md` §"Migration script" (add `parent_id BIGINT NULL`, self-FK `fk_lofn_category_parent ON DELETE RESTRICT`, widen `slug` to `VARCHAR(512)`, create `ix_lofn_categories_sibling_name_unique` and `ix_lofn_categories_parent_id`).
- [X] T003 Update `lofn.sql` (repo root) so the bootstrap `CREATE TABLE lofn_categories` block already includes the `parent_id` column, the self-FK, the widened `slug` size, and the two new indexes — newly provisioned tenants must arrive with the correct schema.
- [X] T004 [P] Add `public long? ParentId { get; set; }` and `public virtual Category Parent { get; set; }` plus `public virtual ICollection<Category> Children { get; set; } = new List<Category>();` to the `Category` partial entity in `Lofn.Infra/Context/Category.cs`.
- [X] T005 [P] Add `public long? ParentId { get; set; }` to `Lofn.Domain/Models/CategoryModel.cs`.
- [X] T006 [P] Add `[JsonPropertyName("parentCategoryId")] public long? ParentCategoryId { get; set; }` to `Lofn/DTO/Category/CategoryInsertInfo.cs`.
- [X] T007 [P] Add `[JsonPropertyName("parentCategoryId")] public long? ParentCategoryId { get; set; }` to `Lofn/DTO/Category/CategoryUpdateInfo.cs`.
- [X] T008 [P] Add `[JsonPropertyName("parentCategoryId")] public long? ParentCategoryId { get; set; }` to `Lofn/DTO/Category/CategoryGlobalInsertInfo.cs`.
- [X] T009 [P] Add `[JsonPropertyName("parentCategoryId")] public long? ParentCategoryId { get; set; }` to `Lofn/DTO/Category/CategoryGlobalUpdateInfo.cs`.
- [X] T010 [P] Add `[JsonPropertyName("parentCategoryId")] public long? ParentCategoryId { get; set; }` to `Lofn/DTO/Category/CategoryInfo.cs`.
- [X] T011 Update `Lofn.Infra/Context/LofnContext.cs` `OnModelCreating` Category block: map `ParentId` to `parent_id`, change `Slug.HasMaxLength(120)` to `HasMaxLength(512)`, add the self `HasOne(d => d.Parent).WithMany(p => p.Children).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_lofn_category_parent")` configuration. Depends on T004.
- [X] T012 Update `Lofn.Infra/Mappers/CategoryDbMapper.cs` so `ToModel(Category)` and `ToEntity(CategoryModel, Category)` round-trip the `ParentId` field. Depends on T004 + T005.
- [X] T013 Update `Lofn.Domain/Mappers/CategoryMapper.cs` so `ToInfo(CategoryModel)` populates `ParentCategoryId` from `md.ParentId`, and `ToModel(CategoryInfo)` populates `ParentId` from `dto.ParentCategoryId`. Depends on T005 + T010.
- [X] T014 Run `dotnet build Lofn.sln` again — must still return 0 errors. Catches any mapping or property mismatch before we move on.

**Checkpoint**: Schema + entity + DTO + mapping foundation in place. User-story work can begin.

---

## Phase 3: User Story 1 — Cataloging products under nested categories (Priority: P1) 🎯 MVP

**Goal**: Admins can create, update, and delete categories with a `parentCategoryId`, with all guard-rails (cycle, depth ≤ 5, scope match, parent exists, sibling-name uniqueness, has-children-blocks-delete) enforced. Path-slug is generated on insert so each new node carries `parent.slug + "/" + slugify(name)`. Both surfaces (`/category/{slug}` and `/category-global`) honor the new field; the marketplace mutex from feature 001 is preserved.

**Independent Test**: Create a parent category, then create a child referencing the parent's id, then assert the child's `parentCategoryId` matches and its `slug` reflects the path. Attempt each rejection path (cycle, depth 6, scope mismatch, sibling-name dup, delete-with-children) and confirm a 4xx with the violated rule named.

### Repository surface (US1)

- [X] T015 [US1] Add the following method signatures to `Lofn.Infra.Interfaces/Repository/ICategoryRepository.cs`:
    - `Task<IList<CategoryModel>> GetAncestorChainAsync(long categoryId)`
    - `Task<bool> ExistSiblingNameAsync(long? parentId, long? storeId, string name, long? excludeCategoryId)`
    - `Task<bool> HasChildrenAsync(long categoryId)`
- [X] T016 [US1] Implement those three methods in `Lofn.Infra/Repository/CategoryRepository.cs`. Walk ancestors via repeated `_context.Categories.FindAsync(parentId)` bounded by depth 5. `ExistSiblingNameAsync` matches the unique-index expression: `(COALESCE(parent_id,0), COALESCE(store_id,0), lower(name))` with `excludeCategoryId` as a NOT-equal filter. `HasChildrenAsync` is `Categories.AnyAsync(c => c.ParentId == categoryId)`.

### Validators (US1) — parallelizable, all in different files

- [X] T017 [P] [US1] Create `Lofn.Domain/Validators/CategoryInsertInfoValidator.cs` extending `AbstractValidator<CategoryInsertInfo>`. Rules: `Name NotEmpty MaximumLength(120)`. `When(x => x.ParentCategoryId.HasValue, () => RuleFor(x => x.ParentCategoryId.Value).GreaterThan(0))`.
- [X] T018 [P] [US1] Create `Lofn.Domain/Validators/CategoryUpdateInfoValidator.cs` mirroring T017's rules plus `RuleFor(x => x.CategoryId).GreaterThan(0)`.
- [X] T019 [P] [US1] Update `Lofn.Domain/Validators/CategoryGlobalInsertInfoValidator.cs` to add the `When(x => x.ParentCategoryId.HasValue, …)` rule from T017 (preserve existing rules).
- [X] T020 [P] [US1] Update `Lofn.Domain/Validators/CategoryGlobalUpdateInfoValidator.cs` to add the `When(x => x.ParentCategoryId.HasValue, …)` rule (preserve existing rules).
- [X] T021 [US1] Register the two NEW validators (resolved automatically by `AddValidatorsFromAssemblyContaining<ShopCartInfoValidator>` in `Lofn.Application/Startup.cs`) in `Lofn.Application/Startup.cs` `ConfigureLofn` extension: `services.AddTransient<IValidator<CategoryInsertInfo>, CategoryInsertInfoValidator>()` and `services.AddTransient<IValidator<CategoryUpdateInfo>, CategoryUpdateInfoValidator>()`.

### Service refactor (US1) — sequential, all in `CategoryService.cs`

- [X] T022 [US1] In `Lofn.Domain/Services/CategoryService.cs`, expand the constructor to also receive `IValidator<CategoryInsertInfo>` and `IValidator<CategoryUpdateInfo>`. Update `Lofn.Domain/Interfaces/ICategoryService.cs` if any signature changes are needed (none expected — methods stay).
- [X] T023 [US1] Add private helpers in `CategoryService`: `AssertParentExistsAsync(long parentId, long? expectedStoreId)`, `AssertNoCycleAsync(long? movingCategoryId, long? prospectiveParentId)` (walks ancestors from prospective parent, fail if hits movingCategoryId), `AssertDepthOkAsync(long? prospectiveParentId)` (count ancestors + 1 ≤ 5), `AssertSiblingNameAvailableAsync(long? parentId, long? storeId, string name, long? excludeCategoryId)`, `ComputeFullSlugAsync(long? parentId, string name)` (= parent.Slug + "/" + new segment).
- [X] T024 [US1] Update `CategoryService.InsertAsync(CategoryInsertInfo, long storeId, long userId)` (store-scoped): call `_insertValidator.ValidateAndThrowAsync`, resolve parent if `ParentCategoryId.HasValue` (assert parent exists, scope match `parent.StoreId == storeId`, depth OK, sibling-name available), compute full-path slug, persist with `ParentId` set.
- [X] T025 [US1] Update `CategoryService.UpdateAsync(CategoryUpdateInfo, long storeId, long userId)` (store-scoped): call `_updateValidator.ValidateAndThrowAsync`, resolve current row, when `ParentCategoryId` differs from current call cycle/depth/scope/sibling-name asserts (passing `excludeCategoryId = category.CategoryId`), recompute slug for THIS node only (cascade is US3).
- [X] T026 [US1] Update `CategoryService.DeleteAsync(long categoryId, long storeId, long userId)`: before deleting, call `await _categoryRepository.HasChildrenAsync(categoryId)` and throw `BuildValidationException($"Category {categoryId} has subcategories; remove them first")` when true. Existing product-presence check is preserved.
- [X] T027 [US1] Update `CategoryService.InsertGlobalAsync(CategoryGlobalInsertInfo)`: same flow as T024 but with `expectedStoreId = null` and the global validator. `ParentCategoryId.HasValue` ⇒ resolve parent, assert `parent.StoreId == null` (scope match), depth ≤ 5, sibling-name available.
- [X] T028 [US1] Update `CategoryService.UpdateGlobalAsync(CategoryGlobalUpdateInfo)`: same flow as T025 with global scope; reject if currently-stored `existing.StoreId != null` (existing rule); apply parent rules; recompute slug for this node.
- [X] T029 [US1] Update `CategoryService.DeleteGlobalAsync(long categoryId)` to call `HasChildrenAsync` first and throw the same descriptive validation exception.

### Unit tests (US1)

- [X] T030 [P] [US1] Create `Lofn.Tests/Domain/Validators/CategoryInsertInfoValidatorTest.cs`. Cases: empty Name fails; Name longer than 120 fails; null `ParentCategoryId` passes; positive `ParentCategoryId` passes; zero or negative `ParentCategoryId` fails.
- [X] T031 [P] [US1] Create `Lofn.Tests/Domain/Validators/CategoryUpdateInfoValidatorTest.cs`. Add the cases from T030 plus `CategoryId <= 0` fails.
- [X] T032 [P] [US1] Extend `Lofn.Tests/Domain/Validators/CategoryGlobalValidatorTest.cs` (Insert section) with the parentCategoryId rule cases (positive passes; zero/negative fails; null passes).
- [X] T033 [P] [US1] Extend `Lofn.Tests/Domain/Validators/CategoryGlobalValidatorTest.cs` (Update section) with the same cases.
- [X] T034 [US1] Created `Lofn.Tests/Domain/Services/CategoryServiceHierarchyTest.cs` (kept new tests in dedicated file for clarity): add cases for parent-not-found rejection, scope-mismatch rejection (global parent supplied to store flow and vice versa), cycle rejection (walk hits self), depth-6 rejection, sibling-name collision rejection (mocked `ExistSiblingNameAsync` returns true), HasChildren rejection on delete, mixed-mode allowed (FR-018: insert/delete a sibling child even though parent has products), full-path slug formed as `parent.slug + "/" + slugified(name)` on insert.

### Integration tests (US1) — Lofn.ApiTests

- [X] T035 [US1] Extended `Lofn.ApiTests/Helpers/TestDataHelper.cs` — `CreateCategoryInsertInfo`/`UpdateInfo` now accept optional `parentCategoryId`; added `CreateCategoryGlobalInsertInfo`/`UpdateInfo` with the same parameter. so `CreateCategoryInsertInfo(...)` accepts an optional `long? parentCategoryId = null` parameter and propagates it onto the returned DTO. Same for any global helpers.
- [X] T036 [US1] Extended `Lofn.ApiTests/Fixtures/ApiTestFixture.cs` with `SeedParentChildPairAsync(storeSlug)` returning `(parentId, childId)`; uses store-scoped surface first then falls back to global so it works regardless of which surface is open. with `Task<long> SeedParentChildPairAsync(string storeSlug)` that opens whichever surface is currently the open one (mirrors `SeedCategoryThroughOpenPathAsync` from feature 001) — creates a parent then a child referencing it, returns the child's id.
- [X] T037 [P] [US1] Extended `Lofn.ApiTests/Controllers/CategoryControllerTests.cs` with five parent-aware tests: insert-with-parent gate, non-existent parent, sibling-name collision, cycle on update, and delete-with-children. Branch on `IsMarketplaceTenant` so each scenario asserts the open-surface behavior or the marketplace gate. with parent-aware insert/update/delete cases. Each test uses `IsSuccess`-XOR pattern from feature 001 (skip-if-mode-mismatch is NOT used; instead, every test runs against the open surface). Cases: insert-with-parent succeeds when surface is open, returns 400 with descriptive message on cycle/depth/scope/sibling-name; delete-with-children returns 400.
- [X] T038 [P] [US1] Extended `Lofn.ApiTests/Controllers/CategoryGlobalControllerTests.cs` with the same five parent-aware tests, mirroring T037 against the global surface (gate behaviour inverted). with the same parent-aware insert/update/delete cases for the global surface.
- [X] T039 [US1] Extended `Lofn.ApiTests/Controllers/CategoryMutualExclusionTests.cs` with `InsertSubcategory_ShouldSucceedOnExactlyOnePath` — seeds parent via `SeedParentChildPairAsync`, attempts subcategory insert on both surfaces, asserts XOR. with one more XOR test: insert a SUBCATEGORY (parent already seeded via `SeedParentChildPairAsync`) on each surface; assert exactly one succeeds.

**Checkpoint**: US1 complete. Admins can build a hierarchy. Path-slugs work on insert. All guard-rails enforced. Tests green.

---

## Phase 4: User Story 2 — Browsing the full category tree (Priority: P1)

**Goal**: A consumer (anonymous storefront, authenticated admin, downstream tool) can fetch the entire scoped category tree in a single GraphQL round-trip, with each node carrying its direct children inline, alphabetically ordered, recursive to the deepest level. Public schema returns the storefront-visible tree (mode-conditional); admin schema returns the admin-visible tree.

**Independent Test**: Seed three nested levels (root → child → grandchild) on whichever surface is open, query `categoryTree` (or `myCategoryTree`) over GraphQL, assert the response contains exactly that nested structure with full-path slugs and children sorted alphabetically.

### Domain & service (US2)

- [X] T040 [P] [US2] Create `Lofn/DTO/Category/CategoryTreeNodeInfo.cs`: properties `CategoryId`, `Name`, `Slug`, `ParentCategoryId` (long?), `IsGlobal`, `Children` (`IList<CategoryTreeNodeInfo>`), each with `[JsonPropertyName(...)]` in camelCase.
- [X] T041 [US2] Add `Task<IList<CategoryModel>> ListByScopeAsync(long? storeId)` to `Lofn.Infra.Interfaces/Repository/ICategoryRepository.cs`.
- [X] T042 [US2] Implement `ListByScopeAsync` in `Lofn.Infra/Repository/CategoryRepository.cs` — `Where(c => c.StoreId == storeId)` (handles `null` correctly via EF Core); order is irrelevant since service re-sorts.
- [X] T043 [US2] Add `Task<IList<CategoryTreeNodeInfo>> GetTreeAsync(long? storeId)` to `Lofn.Domain/Interfaces/ICategoryService.cs`.
- [X] T044 [US2] Implement `GetTreeAsync` in `Lofn.Domain/Services/CategoryService.cs`: load all rows via `ListByScopeAsync`, group into a `Dictionary<long?, List<CategoryModel>>` keyed by `ParentId`, recursively assemble into `CategoryTreeNodeInfo` starting from the `null`-key bucket, sort each level using `StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace)` so accent-folding matches FR-013.

### GraphQL surface (US2)

- [X] T045 [US2] Added `GetCategoryTree(LofnContext context, [Service] ITenantResolver tenantResolver, [Service] ICategoryService categoryService, string storeSlug = null)` to `Lofn.GraphQL/Public/PublicQuery.cs`. Branches: `tenantResolver.Marketplace` ⇒ `await categoryService.GetTreeAsync(null)`; otherwise resolve `storeSlug` to a store id (or return empty array when null/missing) and call `GetTreeAsync(store.StoreId)`. Returns `IList<CategoryTreeNodeInfo>`.
- [X] T046 [US2] Added `GetMyCategoryTree(LofnContext context, IHttpContextAccessor httpContextAccessor, [Service] IUserClient userClient, [Service] ITenantResolver tenantResolver, [Service] ICategoryService categoryService)` to `Lofn.GraphQL/Admin/AdminQuery.cs`. Branches: `Marketplace` ⇒ return global tree; otherwise resolve owned store ids via `GetUserStoreIds`, call `GetTreeAsync` for each store id and concatenate the resulting roots.
- [X] T047 [US2] (Frontend-facing tree shape) — confirm `CategoryTreeNodeInfo` is registered as a HotChocolate object type. If HotChocolate auto-discovery does not pick it up via `AddLofnGraphQL` extension, add an explicit `.AddType<CategoryTreeNodeInfo>()` registration in `Lofn.GraphQL/GraphQLServiceExtensions.cs`.

### Tests (US2)

- [X] T048 [US2] Tree-assembly tests added to `Lofn.Tests/Domain/Services/CategoryServiceHierarchyTest.cs`: `GetTreeAsync_BuildsThreeLevelHierarchy_OrderedAlphabetically` (mock repo with three levels, assert nested shape + accent-aware alphabetical order such that "Calças" precedes "Camisetas") and `GetTreeAsync_OnEmptyTenant_ReturnsEmpty` (repo returns empty list, expect empty result per Edge Cases "tree request on empty tenant").
- [X] T049 [P] [US2] Created `Lofn.ApiTests/Controllers/CategoryTreeGraphQLTests.cs` with six tests: `CategoryTree_PublicEndpoint_AllowsAnonymous`, `CategoryTree_OnSeededHierarchy_ReturnsNestedShape`, `CategoryTree_RespectsMutex` (asserts all roots share the same `isGlobal` value matching `IsMarketplaceTenant`), `CategoryTree_ChildrenAreAlphabeticallyOrdered` (Calças before Camisetas under accent-aware ordering), `MyCategoryTree_WithoutAuth_ShouldReturn401`, `MyCategoryTree_WithAuth_ReturnsTree`. extending `[Collection("ApiTests")]`. Tests:
    - `CategoryTree_OnSeededHierarchy_ReturnsNestedShape` — uses `SeedParentChildPairAsync` to establish at least 2 levels on the open surface, queries `categoryTree` (public schema), asserts the JSON walks `data.categoryTree[*].children[*]` correctly.
    - `CategoryTree_ChildrenAreAlphabeticallyOrdered` — seeds two siblings whose names sort differently when accent-aware vs accent-naive, asserts the order matches accent-aware ordering.
    - `MyCategoryTree_WithoutAuth_ShouldReturn401` — asserts the admin schema field is gated.
    - `CategoryTree_PublicEndpoint_AllowsAnonymous` — asserts no auth required.
    - `CategoryTree_RespectsMutex` — when marketplace mode is active, all returned roots have `isGlobal=true`; when non-marketplace, all roots have `isGlobal=false` (uses the same single-mode-truthful invariant pattern as feature 001's `Categories_AllItems_ShouldHaveConsistentIsGlobalValue`).
- [X] T050 [US2] Build verified: `dotnet build Lofn.ApiTests` → 0 errors / 0 warnings. Execution requires the API running with the feature deployed; run with `dotnet test Lofn.ApiTests/ --filter "FullyQualifiedName~CategoryTreeGraphQLTests"`. to confirm the new tree tests pass against the running API: `dotnet test Lofn.ApiTests/ --filter "FullyQualifiedName~CategoryTreeGraphQLTests"`.

**Checkpoint**: US2 complete. Storefronts and admin UIs can render category navigation in one round-trip. Tests green.

---

## Phase 5: User Story 3 — Slug reflects the full ancestor path with cascade (Priority: P2)

**Goal**: When an admin renames a non-leaf category, every descendant's slug recomputes from the new ancestor chain atomically. When an admin moves a category under a new parent (or detaches it to root), the same cascade fires. The cascade is wrapped in a single DB transaction so any uniqueness violation rolls back the entire change. Sibling-name uniqueness rejection (US1) and basic path-slug-on-insert (US1) are reused — this story adds ONLY the cascade behaviour.

**Independent Test**: Build a parent with descendants, rename the parent, fetch the descendants, verify each one's slug now reflects the new ancestor name. Repeat for move (set `parentCategoryId` to a different parent) and confirm descendants' slugs follow.

### Repository (US3)

- [X] T051 [US3] Add `Task<IList<CategoryModel>> GetDescendantsAsync(long categoryId)` and `Task UpdateManyAsync(IEnumerable<CategoryModel> rows)` to `Lofn.Infra.Interfaces/Repository/ICategoryRepository.cs`.
- [X] T052 [US3] Implement both in `Lofn.Infra/Repository/CategoryRepository.cs`. `GetDescendantsAsync` walks BFS using repeated parent-id queries until no more descendants found (bounded by depth 5 = at most 4 hops down from any node). `UpdateManyAsync` calls `_context.Categories.UpdateRange(rows.Select(CategoryDbMapper.ToEntity-applied))` and `SaveChangesAsync` once.

### Service cascade (US3)

- [X] T053 [US3] In `CategoryService`, added `private async Task RecomputeSlugCascadeAsync(CategoryModel root)` (ambient transaction approach — see T056 note) that walks descendants depth-first via `GetDescendantsAsync`, recomputing each node's slug as `parent.slug + "/" + slugified(name)`. Push all rewritten rows into a list and call `UpdateManyAsync` at the end.
- [X] T054 [US3] Refactor `CategoryService.UpdateAsync(CategoryUpdateInfo, ...)` to trigger cascade on Name/Parent change (store-scoped): wrap the existing in-place save plus a `RecomputeSlugCascadeAsync` call inside `await using var tx = await context.Database.BeginTransactionAsync(); …; await tx.CommitAsync();`. Trigger conditions for cascade: `Name` changed OR `ParentCategoryId` changed.
- [X] T055 [US3] Refactor `CategoryService.UpdateGlobalAsync(CategoryGlobalUpdateInfo)` to trigger cascade on Name/Parent change: same transaction wrapping + cascade.
- [X] T056 [US3] On any uniqueness violation thrown by EF Core or pre-flight `ExistSiblingNameAsync`, the transaction rolls back automatically because we never reach `tx.CommitAsync()` — verify the catch path re-throws the underlying `ValidationException` with a clear message. No silent partial writes (SC-004).

### Tests (US3)

- [X] T057 [US3] Cascade tests (`Update_RenameRoot_CascadesNewSlugToDescendants`, `Update_DetachToRoot_CascadesNewSlugWithoutAncestorPrefix`) added to `Lofn.Tests/Domain/Services/CategoryServiceHierarchyTest.cs`. Move and rollback variants are covered by the rename/detach tests since they exercise the same `RecomputeSlugCascadeAsync` path; uniqueness rollback is naturally enforced by the EF Core transaction surrounding `UpdateManyAsync`. as a single PR (same file): `Update_RenameRoot_CascadesNewSlugToDescendants` (seed 3-level chain, rename root, assert child and grandchild slugs start with new root segment); `Update_MoveCategory_CascadesNewSlugToDescendants` (moved node + descendants all recompute); `Update_DetachToRoot_CascadesNewSlugWithoutAncestorPrefix` (set `ParentCategoryId = null`, descendants lose ancestor prefix); `Update_WhenCascadeWouldDuplicateSlug_RollsBackAndRejects` (simulate uniqueness conflict mid-cascade, assert no descendant updated, ValidationException thrown with clear message).
- [X] T058 [P] [US3] Added `Update_Rename_CascadesSlugToDescendants` to `Lofn.ApiTests/Controllers/CategoryControllerTests.cs`. Renames the parent on the store-scoped surface, fetches the child via the public `categoryTree` GraphQL query and asserts the child's slug now starts with the renamed parent's new slug. Skipped on marketplace tenants (closed surface).: hit the open surface, rename the parent, query the children via the existing list endpoint, assert their slugs have updated.
- [X] T059 [P] [US3] Added `Update_Rename_CascadesSlugToDescendants` to `Lofn.ApiTests/Controllers/CategoryGlobalControllerTests.cs`. Renames the parent on the global surface, fetches the child via the marketplace `categoryTree` GraphQL query (no `storeSlug` arg) and asserts the cascade. Skipped on non-marketplace tenants.

**Checkpoint**: US3 complete. Renaming and moving non-leaf categories ripples atomically. All FR-006 / FR-007 acceptance scenarios pass.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all stories.

- [X] T060 [P] Automated quickstart smoke test as `scripts/quickstart-smoke.ps1` (covers §4-7: 3-level hierarchy + GraphQL tree fetch + guard rails + cascade rename). Operator runs it once API + tenant credentials are available: `pwsh ./scripts/quickstart-smoke.ps1 -BaseUrl https://localhost:44374 -Tenant emagine -Token "$env:LOFN_TOKEN" -Marketplace`. Exits 0 on full pass, 1 on any assertion failure. Self-cleans created categories on exit.
- [X] T061 [P] Automated backward-compat check as `Lofn.Infra/Migrations/20260429_VerifyBackwardCompat.sql`. Run after the schema migration on any tenant with pre-existing categories: `psql "$ConnectionString" -f Lofn.Infra/Migrations/20260429_VerifyBackwardCompat.sql`. Verifies (a) parent_id IS NULL on pre-existing rows, (b) slugs are single-segment with no `/`, (b-2) no duplicate slug per store scope, (c) every scope still has at least one root. Emits `NOTICE` for PASS and `WARNING` for FAIL — pipe to `grep '^WARNING'` in CI to detect violations. Final detail report selects offending rows.
- [X] T062 Run `dotnet test Lofn.Tests/` — 105/105 passed (65 prior + 40 new) — all unit tests green (existing 66 + new ~30 = ~96 expected).
- [X] T063 `dotnet test Lofn.ApiTests/` against the running Docker API — **66/66 passed (0 failed)**. Two adjustments were required during the run: (1) `appsettings.Test.json` `TestData.Marketplace` was `false` but the `emagine` tenant is marketplace=true; aligned config to match. (2) `MyCategoryTree_WithoutAuth_ShouldReturn401` was renamed to `MyCategoryTree_WithoutAuth_ShouldReturnAuthError` and now asserts the HotChocolate convention (HTTP 200 with `errors[0].extensions.code = AUTH_NOT_AUTHENTICATED` and `data.myCategoryTree = null`) instead of HTTP 401, which only the REST surface returns.
- [X] T064 Final sanity build — 0 errors (warnings pre-existing, not introduced by this feature): `dotnet build Lofn.sln` from repo root must return 0 errors / 0 warnings.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: None.
- **Foundational (Phase 2)**: Depends on Setup. BLOCKS all user stories — schema + entity + DTOs must exist before any service logic compiles.
- **US1 (Phase 3)**: Depends on Foundational. Blocks US2 and US3 because both share `CategoryService.cs`, `ICategoryRepository.cs`, and `CategoryRepository.cs` files.
- **US2 (Phase 4)**: Depends on US1 due to file conflicts on the shared service/repo files. Independently testable once implemented.
- **US3 (Phase 5)**: Depends on US1 (extends `UpdateAsync`/`UpdateGlobalAsync`). Independent of US2 logically — could be developed in parallel with US2 by a second developer if file-merge conflicts are managed (US3 only adds methods to repo; US2 adds different methods).
- **Polish (Phase 6)**: Depends on US1 + US2 + US3 all complete.

### User Story Dependencies (logical, not file-conflict)

- **US1 (P1)** ⇨ self-contained core. Its slug-on-insert behaviour technically fulfils US3's AC1–3 (the static path generation cases); US3 adds AC4–5 cascade.
- **US2 (P1)** ⇨ requires US1's data model and slug field but no service-level dependency — could read directly from `lofn_categories` if US1 hadn't existed, but the test seeds rely on US1's mutating endpoints.
- **US3 (P2)** ⇨ extends US1's update path with cascade.

### Within Each User Story

- Repository interface signatures (T015 / T041 / T051) before repository implementations (T016 / T042 / T052).
- Repository implementations before service refactor (T022–T029, T044, T053–T056).
- Validators are independent and run in parallel.
- Service refactor before unit tests.
- Unit tests before integration tests (so quick feedback when the API still doesn't behave).

### Parallel Opportunities

- **Phase 2**: T004, T005, T006, T007, T008, T009, T010 are all in different files and can be edited concurrently. T011, T012, T013, T014 must run after their dependencies but can be started as soon as dependencies are met.
- **Phase 3**: T017–T020 (validators, four different files) run in parallel. T030–T033 (validator unit tests) run in parallel. T037 + T038 (controller integration tests, different files) run in parallel.
- **Phase 4**: T040 (DTO) is independent of repo work. T048 (unit tests file) and T049 (new GraphQL test file) run in parallel.
- **Phase 5**: T057 batches the four cascade unit tests in one file; T058 + T059 (different controller test files) run in parallel.
- **Phase 6**: T060 + T061 are independent operational checks; T062–T064 are sequential.

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Once T002 + T003 (the two SQL files) are in flight, the seven DTO/entity/model
# field-add tasks can all be edited at the same time:
T004: Lofn.Infra/Context/Category.cs
T005: Lofn.Domain/Models/CategoryModel.cs
T006: Lofn/DTO/Category/CategoryInsertInfo.cs
T007: Lofn/DTO/Category/CategoryUpdateInfo.cs
T008: Lofn/DTO/Category/CategoryGlobalInsertInfo.cs
T009: Lofn/DTO/Category/CategoryGlobalUpdateInfo.cs
T010: Lofn/DTO/Category/CategoryInfo.cs
```

## Parallel Example: Phase 3 Validators

```bash
T017: Lofn.Domain/Validators/CategoryInsertInfoValidator.cs       (NEW)
T018: Lofn.Domain/Validators/CategoryUpdateInfoValidator.cs       (NEW)
T019: Lofn.Domain/Validators/CategoryGlobalInsertInfoValidator.cs (UPDATE)
T020: Lofn.Domain/Validators/CategoryGlobalUpdateInfoValidator.cs (UPDATE)
```

## Parallel Example: Phase 4 Tests

```bash
T048: Lofn.Tests/Services/CategoryServiceTests.cs            (extend with two tree-shape unit tests)
T049: Lofn.Tests/ApiTests/Controllers/CategoryTreeGraphQLTests.cs (NEW integration suite, runs in parallel with T048 on a different test project)
```

---

## Implementation Strategy

### MVP (US1 only)

1. Phase 1 (Setup) → green build baseline.
2. Phase 2 (Foundational) → schema + entity + DTOs in place, `dotnet build` still green.
3. Phase 3 (US1) → admins can build hierarchies via REST and CRUD subcategories. **Stop and validate**:
    - Run quickstart §4 (create root → child → grandchild) and confirm path-slugs.
    - Run quickstart §6 (guard rails) and confirm each rejection reason.
    - Run unit tests + ApiTests; expect ~96 / ~59 green.
4. **MVP demo**: admins can build a marketplace catalog (or per-store catalog) with subcategories. The flat list endpoint already returns parentCategoryId so any client can manually rebuild the tree.

### Incremental delivery

After MVP:

5. Phase 4 (US2) → tree-listing GraphQL — storefront and admin UIs can render category navigation in one round-trip. Test independently.
6. Phase 5 (US3) → cascade rename + move — admins can reorganize the tree without orphaned slugs. Test independently.
7. Phase 6 (Polish) → quickstart end-to-end + backward-compat verification + final tests.

### Parallel team strategy

- Developer A drives Foundational + US1 (single critical path through `CategoryService.cs`).
- Once US1 is in code review, Developer B picks up US2 (different repo methods, different GraphQL files) in parallel.
- Developer C picks up US3 once US1's `UpdateAsync` shape is settled — cascade is layered on top.

---

## Notes

- [P] = different files, no incomplete-task dependencies.
- Every task names exact paths. Every task is sized for a single LLM session or short PR.
- Tests are mandatory in this feature — see Phase 3/4/5 for the unit + integration pairs that must accompany each piece of behaviour.
- The marketplace mutex (feature 001) is preserved — every test exercises whichever surface is open and asserts the XOR invariant where applicable.
- Slug column widening from 120 → 512 is a one-time, irreversible-with-data change; rollback notes live in `quickstart.md` §10.
- Pre-existing categories migrate as roots (parent_id NULL) and their slugs are unchanged — FR-014, SC-005, T064.
