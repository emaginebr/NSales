# Implementation Plan: Category Subcategories Support

**Branch**: `002-category-subcategories` | **Date**: 2026-04-29 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/002-category-subcategories/spec.md`

## Summary

Add a self-referencing parent on `Category`, change slugs to full hierarchical paths (`vestuario/camisetas/vintage`), and expose a recursive tree-listing GraphQL field on both the public and admin schemas. The marketplace-vs-store mutex from feature 001 is preserved verbatim — the tree returned by each schema follows whichever surface is currently open. Renaming or moving a category atomically recomputes its slug and all descendant slugs in a single transaction. Maximum nesting depth is capped at 5 levels, sibling ordering is alphabetical (case-insensitive, accent-normalized), and product search remains direct (no transitive matching across descendants).

Approach is conservative: extend existing `Category` entity, `CategoryModel`, `CategoryService`, both REST controllers, both GraphQL queries, and the existing FluentValidation validators. No new project, no new pattern. One DDL migration adds `parent_id` + a self-FK + a sibling-name unique index + slug column widening. Pre-existing categories migrate as roots (parent_id NULL) and their slugs remain unchanged because path == name for roots.

## Technical Context

**Language/Version**: C# 12 / .NET 8
**Primary Dependencies**: ASP.NET Core 8, EF Core 9 (Npgsql), HotChocolate 14.3, FluentValidation 12, AutoMapper, NAuth.ACL (auth)
**Storage**: PostgreSQL (per-tenant DB resolved via `TenantDbContextFactory`)
**Testing**: xUnit + Moq + FluentValidation.TestHelper for unit tests (`Lofn.Tests`); xUnit + Flurl.Http 4.0 + FluentAssertions 7.0 for integration tests against a running API (`Lofn.ApiTests`)
**Target Platform**: Linux server (Docker), nginx-proxy in front of API on port 8081
**Project Type**: Web service (.NET) + React frontend (frontend not changed by this feature)
**Performance Goals**: Tree query returns in a single round-trip for ≤500 categories spread across ≤5 levels (SC-002, SC-006). Cascade slug recompute completes within one DB transaction even for the deepest pathological case (worst case ≈ 500 rows updated atomically).
**Constraints**: Slug column currently `varchar(120)`. With 5 levels × 24 chars + 4 slashes the worst path is ~124 chars — column MUST be widened. Slugs must remain URL-safe and the existing slug normalization (`IStringClient.GenerateSlugAsync`) is reused per segment. Marketplace mutex (feature 001) is non-negotiable.
**Scale/Scope**: Single tenant typically holds <100 categories today; design target is 500 categories × 5 levels per tenant (SC-002). Beyond 500 the server still answers correctly but performance is not guaranteed.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

`.specify/memory/constitution.md` is the unmodified template (no ratified principles). No formal constitutional gates apply. Implicit project conventions inferred from the codebase and `CLAUDE.md` (which DO apply):

- **Clean Architecture layering** — DTO/Domain/Infra.Interfaces/Infra/Application/API/GraphQL. New code respects existing dependency direction. ✅
- **Repository + Unit of Work** in `Lofn.Infra` — extend `ICategoryRepository` with new methods, do not bypass. ✅
- **FluentValidation per DTO** — extend existing validators (CategoryInsert/Update/GlobalInsert/GlobalUpdate). ✅
- **Multi-tenant via `ITenantResolver`** — every read/write goes through tenant-scoped DbContext via `TenantDbContextFactory` already in DI. ✅
- **Marketplace mutex (feature 001)** — preserved unchanged: `Marketplace=true` → only `/category-global` mutates and only the global tree returns; `Marketplace=false` → only `/category/{slug}` mutates and only the store tree returns. ✅
- **Tests-as-contract** — both unit (in `Lofn.Tests`) and integration (in `Lofn.ApiTests` against the live API) are mandatory; the XOR mutual-exclusion pattern from feature 001 is reused for the tree query so tests work regardless of tenant config. ✅

No violations to track. `## Complexity Tracking` left empty.

## Project Structure

### Documentation (this feature)

```text
specs/002-category-subcategories/
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   ├── graphql.md       # New tree query SDL + payload shape
│   └── rest.md          # CategoryInsertInfo / UpdateInfo / Global variants additions
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT this command)
```

### Source Code (repository root)

The feature touches the following existing locations (no new project added):

```text
Lofn/
├── Lofn.sln
├── Lofn.Infra/
│   ├── Context/
│   │   ├── Category.cs                            # ADD: ParentId, Parent navigation, Children collection
│   │   └── LofnContext.cs                         # ADD: parent_id mapping, sibling-name unique index, FK self-ref
│   ├── Migrations/
│   │   └── 20260429_AddCategoryParentId.sql       # NEW: parent_id + FK + indexes + slug widening
│   ├── Mappers/
│   │   └── CategoryDbMapper.cs                    # ADD: ParentId round-trip
│   └── Repository/
│       └── CategoryRepository.cs                  # ADD: ListByScopeAsync, GetDescendantsAsync, GetAncestorChainAsync, ExistSiblingNameAsync
├── Lofn.Infra.Interfaces/
│   └── Repository/
│       └── ICategoryRepository.cs                 # ADD: corresponding new method signatures
├── Lofn.Domain/
│   ├── Models/
│   │   └── CategoryModel.cs                       # ADD: ParentId
│   ├── Mappers/
│   │   └── CategoryMapper.cs                      # ADD: ParentId in DTO ↔ Model
│   ├── Validators/
│   │   ├── CategoryInsertInfoValidator.cs         # NEW
│   │   ├── CategoryUpdateInfoValidator.cs         # NEW
│   │   ├── CategoryGlobalInsertInfoValidator.cs   # UPDATE: parent rules
│   │   └── CategoryGlobalUpdateInfoValidator.cs   # UPDATE: parent rules
│   ├── Interfaces/
│   │   └── ICategoryService.cs                    # ADD: GetTreeAsync(scope), parent-aware overloads
│   └── Services/
│       └── CategoryService.cs                     # CORE: cycle/depth/scope checks, full-path slug, cascade recompute, tree assembly
├── Lofn/
│   └── DTO/
│       └── Category/
│           ├── CategoryInsertInfo.cs              # ADD: ParentCategoryId (nullable)
│           ├── CategoryUpdateInfo.cs              # ADD: ParentCategoryId (nullable)
│           ├── CategoryGlobalInsertInfo.cs        # ADD: ParentCategoryId (nullable)
│           ├── CategoryGlobalUpdateInfo.cs        # ADD: ParentCategoryId (nullable)
│           ├── CategoryInfo.cs                    # ADD: ParentCategoryId (nullable) — flat consumers see parent
│           └── CategoryTreeNodeInfo.cs            # NEW: id, name, slug, parentCategoryId, isGlobal, children[]
├── Lofn.GraphQL/
│   ├── Public/
│   │   └── PublicQuery.cs                         # ADD: GetCategoryTree(storeSlug?)
│   ├── Admin/
│   │   └── AdminQuery.cs                          # ADD: GetMyCategoryTree
│   └── Types/
│       └── CategoryTypeExtension.cs               # ADD: GetParent / GetChildren resolvers backing the recursive shape
├── Lofn.API/
│   └── Controllers/
│       ├── CategoryController.cs                  # No code change beyond DTOs (controller forwards)
│       └── CategoryGlobalController.cs            # Same
├── Lofn.Tests/
│   ├── Validators/
│   │   ├── CategoryInsertInfoValidatorTests.cs    # NEW
│   │   ├── CategoryUpdateInfoValidatorTests.cs    # NEW
│   │   ├── CategoryGlobalInsertInfoValidatorTests.cs   # UPDATE: parent cases
│   │   └── CategoryGlobalUpdateInfoValidatorTests.cs   # UPDATE: parent cases
│   └── Services/
│       └── CategoryServiceTests.cs                # NEW: cycle, depth, scope, cascade slug, tree shape, sibling name
├── Lofn.ApiTests/
│   ├── Controllers/
│   │   ├── CategoryControllerTests.cs             # ADD: parent-aware insert/update tests
│   │   ├── CategoryGlobalControllerTests.cs       # ADD: parent-aware insert/update tests
│   │   ├── CategoryMutualExclusionTests.cs        # ADD: parent-aware mutex (set parent on each surface)
│   │   └── CategoryTreeGraphQLTests.cs            # NEW: tree shape, depth, alphabetical order, mutex
│   ├── Fixtures/
│   │   └── ApiTestFixture.cs                      # ADD: helper that seeds a parent+child pair on demand
│   └── Helpers/
│       └── TestDataHelper.cs                      # ADD: factories accept parentCategoryId
└── lofn.sql                                       # UPDATE: append parent_id column + FK + indexes + slug widening
```

**Structure Decision**: Single-solution Clean Architecture (existing layout). No new csproj. New files appended into existing folders following established naming. The only schema change is one SQL file in `Lofn.Infra/Migrations/`, mirroring the pattern from feature 001.

## Complexity Tracking

> No constitutional violations to track — table omitted intentionally.
