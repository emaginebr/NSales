# Implementation Plan: Marketplace category mode per tenant

**Branch**: `001-marketplace-categories` | **Date**: 2026-04-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-marketplace-categories/spec.md`

## Summary

Introduce a per-tenant `Marketplace` boolean flag (lives in `appsettings.Tenants:{slug}:Marketplace`). When `true`, categories become tenant-global (only system admins — `IsAdmin = true` — can manage them) and every product within the tenant must reference one of those global categories. When `false`, today's per-store category management is preserved unchanged. The feature ships with a new dedicated REST surface for global-category management, a tenant-aware authorization gate on every category mutation, tenant-wide slug uniqueness enforcement (with grandfathered legacy data), and adjusted product validation in both REST and GraphQL paths. No automatic data migration runs on flag flip — admins handle data lifecycle outside the application.

## Technical Context

**Language/Version**: C# / .NET 8.0
**Primary Dependencies**: ASP.NET Core 8, EF Core 9 (Npgsql + lazy loading proxies), HotChocolate GraphQL 14.3, FluentValidation 12, NAuth 0.5.5 (auth + `IsAdmin` claim), zTools (file/string clients), Serilog 9
**Storage**: PostgreSQL per tenant (one DB per tenant resolved by `ITenantResolver.ConnectionString` from `appsettings.Tenants:{tenantId}:ConnectionString`)
**Testing**: xUnit (unit tests in `Lofn.Tests`) + xUnit/Flurl/FluentAssertions (integration in `Lofn.ApiTests`)
**Target Platform**: Linux x64 server, deployed as Docker container (`Lofn.API/Dockerfile`) behind nginx-proxy; PostgreSQL 17-alpine
**Project Type**: Multi-project .NET solution following Clean Architecture — DTO / Domain / Infra.Interfaces / Infra / Application / API / GraphQL
**Performance Goals**: SC-005 — listing categories at p95 < 1s regardless of mode and tenant size; SC-006 — flag value consistent for the first request after restart (no warm-up cache)
**Constraints**: Multi-tenant isolation via per-tenant DB; `IsAdmin` is the sole role gating global category management (no new role system); slug uniqueness scoped to tenant (FR-015); no automatic data migration on flag flip (FR-010 + Q5)
**Scale/Scope**: Existing tenants (currently `emagine`, `monexup` per `appsettings.Production.json`); existing public storefront unchanged (anonymous shoppers); admin surface is the only client needing UI updates

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project's `.specify/memory/constitution.md` is still a placeholder — no principles have been ratified. There are therefore no constitutional gates to evaluate for this feature. **Result: PASS (no gates configured)**. Recommend running `/speckit.constitution` before the next feature so that future plans have hard checkpoints (Test-First, Library-First, Observability, etc.).

## Project Structure

### Documentation (this feature)

```text
specs/001-marketplace-categories/
├── plan.md              # This file
├── spec.md              # Functional specification (already authored)
├── research.md          # Phase 0 output — decisions backing this plan
├── data-model.md        # Phase 1 output — entity changes & invariants
├── quickstart.md        # Phase 1 output — manual smoke-test recipe
├── contracts/
│   ├── rest-category-global.md   # REST surface for global categories
│   ├── rest-category-store.md    # REST behaviour deltas in marketplace mode
│   ├── rest-product.md           # REST product validation deltas
│   └── graphql-schema.md         # GraphQL public/admin schema deltas
├── checklists/
│   └── requirements.md           # Already created in /speckit.specify
└── tasks.md             # Created later by /speckit.tasks
```

### Source Code (repository root)

```text
Lofn/                                     # DTO + ACL shared library (project name = "Lofn")
├── DTO/
│   └── Category/
│       ├── CategoryInfo.cs               # NEW field: bool IsGlobal
│       ├── CategoryGlobalInsertInfo.cs   # NEW DTO for /category-global/insert
│       └── CategoryGlobalUpdateInfo.cs   # NEW DTO for /category-global/update

Lofn.Domain/                              # Domain models, services, validators, interfaces
├── Interfaces/
│   ├── ITenantResolver.cs                # MODIFIED: adds bool Marketplace property
│   └── ICategoryService.cs               # MODIFIED: new global-category methods
├── Models/
│   └── CategoryModel.cs                  # MODIFIED: StoreId already nullable; document NULL = global
├── Services/
│   └── CategoryService.cs                # MODIFIED: branches by Marketplace flag; new global flow
└── Validators/
    ├── CategoryGlobalInsertInfoValidator.cs   # NEW
    └── CategoryGlobalUpdateInfoValidator.cs   # NEW

Lofn.Infra.Interfaces/
└── Repository/
    └── ICategoryRepository.cs            # MODIFIED: ListGlobalAsync, ExistSlugInTenantAsync

Lofn.Infra/
├── Context/
│   ├── Category.cs                       # No structural change (StoreId already nullable)
│   └── LofnContext.cs                    # MODIFIED: index/unique constraint on (Slug) for globals
├── Repository/
│   └── CategoryRepository.cs             # MODIFIED: new query methods
└── Migrations/
    └── 20260428_AddGlobalCategoryUniqueIndex.cs  # NEW EF Core migration

Lofn.Application/
├── Startup.cs                             # MODIFIED: register new validators
├── TenantResolver.cs                     # MODIFIED: read Marketplace from config
└── Authorization/
    └── MarketplaceAdminRequirement.cs    # NEW authorization handler/filter

Lofn.API/
├── Controllers/
│   ├── CategoryController.cs             # MODIFIED: reject writes when Marketplace = true
│   ├── CategoryGlobalController.cs       # NEW: dedicated surface (no storeSlug)
│   └── ProductController.cs              # No code change — service-layer validation handles it
└── appsettings.*.json                    # MODIFIED: Tenants:{slug}:Marketplace = false (default)

Lofn.GraphQL/
├── Public/PublicQuery.cs                 # MODIFIED: GetCategories splits by tenant Marketplace
└── Admin/AdminQuery.cs                   # MODIFIED: myCategories returns globals when Marketplace

Lofn.Tests/                                # Unit tests
├── Domain/Services/CategoryServiceTest.cs                # MODIFIED: new branches
├── Domain/Services/CategoryServiceMarketplaceTest.cs     # NEW
└── Domain/Validators/CategoryGlobalValidatorTest.cs      # NEW

Lofn.ApiTests/                             # Integration tests
└── Controllers/
    ├── CategoryControllerTests.cs        # MODIFIED: reject 403 when Marketplace = true
    └── CategoryGlobalControllerTests.cs  # NEW: full CRUD for global surface
```

**Structure Decision**: Standard multi-project .NET Clean Architecture, already in place. The feature adds a small, additive surface (one new controller, one new authorization filter, two new DTOs, two new validators, one EF migration, modifications to four existing services) and modifies tenant resolution to expose the new flag. No project boundaries change.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |

The feature is additive — no architectural pattern is broken, no new project is introduced, no new external dependency is added. The single non-trivial decision is using the existing nullable `Category.StoreId` column (`StoreId IS NULL` ⇔ tenant-global) instead of introducing a `Scope` enum column; this avoids a destructive schema change at the cost of a slightly less explicit data model. Documented in `data-model.md`.
