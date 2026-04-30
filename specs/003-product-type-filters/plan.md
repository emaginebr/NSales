# Implementation Plan: Tipo de Produto, filtros e customizações

**Branch**: `003-product-type-filters` | **Date**: 2026-04-29 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/003-product-type-filters/spec.md`

## Summary

Introduzir o conceito de **Tipo de Produto** como classificador tenant-scoped (ex.: Calçado, Roupa, Carro, Comida, Equipamento), declarado e gerenciado exclusivamente por usuários `IsAdmin = true`. Cada Tipo carrega dois esquemas:

1. **Esquema de Filtros** — atributos discretos (texto, inteiro, decimal, booleano, enum) que produtos da categoria tipada precisam ou podem preencher; usados para validação no cadastro do produto e como facetas pesquisáveis no storefront público.
2. **Esquema de Customizações** — grupos de opções (single/multi-seleção) com `price_delta` (signed, em centavos), Type-only (sem override por produto), exibidos no detalhe do produto com cálculo de preço dinâmico.

A binding entre Tipo e produto é indireta: cada **Categoria** ganha um vínculo opcional `0..1` com um Tipo. Categorias sem vínculo direto herdam o Tipo do ancestral mais próximo (closest-ancestor wins), reaproveitando a árvore da feature 002.

A integração com pedido/carrinho está **explicitamente fora do escopo** desta release (Q2:B em Clarifications) — o storefront exibe customizações com aviso de que o carrinho ainda usa preço-base. Persistência da escolha em pedido fica para feature seguinte.

Abordagem conservadora: estender entidades/serviços/controllers existentes, adicionar 5 novas entidades de domínio, 4 novas tabelas + uma coluna em `lofn_categories`, novos validators FluentValidation, novos endpoints REST sob `/producttype` e `/category/{categoryId}/producttype`, novas queries GraphQL nos schemas público e admin. Nenhum projeto novo.

## Technical Context

**Language/Version**: C# 12 / .NET 8
**Primary Dependencies**: ASP.NET Core 8, EF Core 9 (Npgsql), HotChocolate 14.3, FluentValidation 12, AutoMapper, NAuth.ACL (auth), `Lofn.Domain.Core.SlugGenerator` (já existente da feature 002)
**Storage**: PostgreSQL (per-tenant DB resolved via `TenantDbContextFactory`)
**Testing**: xUnit + Moq + FluentValidation.TestHelper para unit tests (`Lofn.Tests`); xUnit + Flurl.Http 4.0 + FluentAssertions 7.0 para integration tests contra a API live (`Lofn.ApiTests`)
**Target Platform**: Linux server (Docker), nginx-proxy em frente da API na porta 8081
**Project Type**: Web service (.NET) + frontend React (frontend NÃO é alterado por esta feature — endpoints existentes do produto retornam novos campos opcionalmente populados)
**Performance Goals**: Listagem filtrada paginada (`POST /product/search-filtered`) responde em ≤500 ms p95 para catálogos até 10.000 produtos por tenant com até 4 filtros simultâneos (SC-002). Cálculo de preço (`POST /product/{id}/price`) responde em <100ms p95 (cálculo puro em memória, sem I/O de pedido).
**Constraints**: Filtros são armazenados como `(filter_id, value)` polimórfico em coluna text — interpretação por `data_type` declarado no filtro. Index composto `(filter_id, value)` cobre o caso de igualdade simples. Ranges/numéricos ordenados são fora de escopo. Cada filtro tem `internal_key` estável (UUID/long) independente do `label` para suportar renomeação sem perder valores históricos. Auditoria reusa `LogCore` existente.
**Scale/Scope**: Cada tenant typically: 5–20 Tipos, 5–10 filtros por tipo, 0–5 grupos de customização por tipo, 2–10 opções por grupo, ≤10.000 produtos. O design alvo cobre 10× esse volume sem alteração estrutural. Worst-case de listagem filtrada: 4 filtros AND sobre catálogo de 10.000 produtos com 5 valores por filtro — coberto pelo índice composto.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

`.specify/memory/constitution.md` é o template não-customizado (sem princípios ratificados). Não há gates constitucionais formais. As convenções implícitas inferidas de `CLAUDE.md` e do código (que SE APLICAM):

- **Clean Architecture layering** — DTO/Domain/Infra.Interfaces/Infra/Application/API/GraphQL. Novo código respeita o sentido único da dependência. ✅
- **Repository + Unit of Work** em `Lofn.Infra` — adicionar `IProductTypeRepository`, `IProductFilterValueRepository`, estender `ICategoryRepository` (linkar tipo) e `IProductRepository` (search filtrado). Não bypassar UoW. ✅
- **FluentValidation por DTO** — todos os 8+ DTOs novos ganham validators dedicados em `Lofn.Domain/Validators/`. ✅
- **Multi-tenant via `ITenantResolver`** — todas as leituras/escritas vão pelo DbContext tenant-scoped resolvido pelo `TenantDbContextFactory`. ✅
- **Marketplace mutex (feature 001)** — Tipo de Produto é tenant-global em ambos os modos. Em modo marketplace o admin é o único, em modo loja-única o admin do tenant idem. Tipos NÃO são por loja, então não há conflito com a mutex; categorias seguem regendo o escopo das atribuições de produto. ✅
- **Hierarquia de categoria (feature 002)** — preservada. Vínculo categoria↔tipo respeita parent_id; resolução closest-ancestor reusa `GetAncestorChainAsync` já existente. ✅
- **Slug generation** — não aplicável a Tipo (não tem slug). Categoria não muda. ✅
- **Tests-as-contract** — unit tests em `Lofn.Tests` cobrem regras de validação, resolução closest-ancestor e cálculo de preço; integration tests em `Lofn.ApiTests` cobrem todos os endpoints REST + queries GraphQL. ✅
- **Permission gate** — novo atributo `[TenantAdmin]` (apenas `IsAdmin = true`, sem mutex marketplace) reutilizado em todos os controllers de Tipo. Distinção do `[MarketplaceAdmin]` existente é deliberada (este último exige também `Marketplace = true`). ✅

Sem violações. `## Complexity Tracking` deixada vazia.

## Project Structure

### Documentation (this feature)

```text
specs/003-product-type-filters/
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   ├── rest.md          # Novos endpoints sob /producttype + extensões em /product, /category
│   └── graphql.md       # Novos campos/queries nos schemas Public e Admin
└── tasks.md             # Phase 2 output (/speckit.tasks — NÃO criada por este comando)
```

### Source Code (repository root)

A feature toca os seguintes locais existentes (sem novos csproj):

```text
Lofn/
├── Lofn.sln
├── Lofn.Infra/
│   ├── Context/
│   │   ├── ProductType.cs                              # NEW: lofn_product_types
│   │   ├── ProductTypeFilter.cs                        # NEW: lofn_product_type_filters
│   │   ├── ProductTypeFilterAllowedValue.cs            # NEW: lofn_product_type_filter_allowed_values (enum values)
│   │   ├── ProductTypeCustomizationGroup.cs            # NEW: lofn_product_type_customization_groups
│   │   ├── ProductTypeCustomizationOption.cs           # NEW: lofn_product_type_customization_options
│   │   ├── ProductFilterValue.cs                       # NEW: lofn_product_filter_values
│   │   ├── Category.cs                                 # ADD: ProductTypeId (nullable FK), ProductType navigation
│   │   ├── Product.cs                                  # ADD: FilterValues navigation collection
│   │   └── LofnContext.cs                              # ADD: 6 new DbSets, FK config, indexes
│   ├── Migrations/
│   │   └── 20260430_AddProductTypes.sql                # NEW: 6 tables + FK in lofn_categories + indexes
│   ├── Mappers/
│   │   ├── ProductTypeDbMapper.cs                      # NEW
│   │   ├── ProductFilterValueDbMapper.cs               # NEW
│   │   └── CategoryDbMapper.cs                         # ADD: ProductTypeId round-trip
│   └── Repository/
│       ├── ProductTypeRepository.cs                    # NEW: CRUD + by-tenant + filter/group/option child ops
│       ├── ProductFilterValueRepository.cs             # NEW: bulk-set per product, filtered search
│       ├── CategoryRepository.cs                       # ADD: GetAppliedProductTypeAsync (closest-ancestor walk)
│       └── ProductRepository.cs                        # ADD: SearchByFilterValuesAsync (filter + category rollup)
├── Lofn.Infra.Interfaces/
│   └── Repository/
│       ├── IProductTypeRepository.cs                   # NEW
│       ├── IProductFilterValueRepository.cs            # NEW
│       ├── ICategoryRepository.cs                      # ADD: GetAppliedProductTypeAsync
│       └── IProductRepository.cs                       # ADD: SearchByFilterValuesAsync
├── Lofn.Domain/
│   ├── Models/
│   │   ├── ProductTypeModel.cs                         # NEW
│   │   ├── ProductTypeFilterModel.cs                   # NEW
│   │   ├── ProductTypeCustomizationGroupModel.cs       # NEW
│   │   ├── ProductTypeCustomizationOptionModel.cs      # NEW
│   │   ├── ProductFilterValueModel.cs                  # NEW
│   │   ├── CategoryModel.cs                            # ADD: ProductTypeId
│   │   └── ProductModel.cs                             # ADD: FilterValues collection
│   ├── Mappers/
│   │   ├── ProductTypeMapper.cs                        # NEW: model ↔ DTO
│   │   ├── ProductFilterValueMapper.cs                 # NEW
│   │   └── ProductMapper.cs                            # ADD: FilterValues, AppliedProductType in DTO
│   ├── Validators/
│   │   ├── ProductTypeInsertInfoValidator.cs           # NEW
│   │   ├── ProductTypeUpdateInfoValidator.cs           # NEW
│   │   ├── ProductTypeFilterInsertInfoValidator.cs     # NEW
│   │   ├── ProductTypeFilterUpdateInfoValidator.cs     # NEW
│   │   ├── CustomizationGroupInsertInfoValidator.cs    # NEW
│   │   ├── CustomizationGroupUpdateInfoValidator.cs    # NEW
│   │   ├── CustomizationOptionInsertInfoValidator.cs   # NEW
│   │   ├── CustomizationOptionUpdateInfoValidator.cs   # NEW
│   │   └── CategoryProductTypeLinkValidator.cs         # NEW: link/unlink validations
│   ├── Interfaces/
│   │   ├── IProductTypeService.cs                      # NEW
│   │   └── ICategoryService.cs                         # ADD: LinkProductTypeAsync, UnlinkProductTypeAsync, GetAppliedProductTypeAsync
│   └── Services/
│       ├── ProductTypeService.cs                       # CORE: CRUD + filter/group/option mgmt + dedup of filter labels
│       ├── ProductFilterValueResolver.cs               # CORE: validation of submitted values vs schema, type coercion
│       ├── ProductPriceCalculator.cs                   # CORE: base price + Σ(price_delta of selected options)
│       ├── CategoryService.cs                          # ADD: link/unlink/applied-type resolution
│       └── ProductService.cs                           # ADD: filter values write on insert/update; search-filtered
├── Lofn/
│   └── DTO/
│       ├── ProductType/
│       │   ├── ProductTypeInsertInfo.cs                # NEW
│       │   ├── ProductTypeUpdateInfo.cs                # NEW
│       │   ├── ProductTypeInfo.cs                      # NEW: id, name, filters[], customizationGroups[]
│       │   ├── ProductTypeFilterInsertInfo.cs          # NEW
│       │   ├── ProductTypeFilterUpdateInfo.cs          # NEW
│       │   ├── ProductTypeFilterInfo.cs                # NEW: id, label, dataType, required, allowedValues[]
│       │   ├── CustomizationGroupInsertInfo.cs         # NEW
│       │   ├── CustomizationGroupUpdateInfo.cs         # NEW
│       │   ├── CustomizationGroupInfo.cs               # NEW: id, label, selectionMode, required, options[]
│       │   ├── CustomizationOptionInsertInfo.cs        # NEW
│       │   ├── CustomizationOptionUpdateInfo.cs        # NEW
│       │   ├── CustomizationOptionInfo.cs              # NEW: id, label, priceDelta, isDefault
│       │   ├── ProductFilterValueInfo.cs               # NEW: filterId, label, dataType, value
│       │   └── ProductPriceCalculationRequest.cs       # NEW: optionIds[]
│       │   └── ProductPriceCalculationResult.cs        # NEW: basePrice, deltaTotal, total, breakdown[]
│       ├── Product/
│       │   ├── ProductInsertInfo.cs                    # ADD: FilterValues (list of {filterId, value}) — optional
│       │   ├── ProductUpdateInfo.cs                    # ADD: FilterValues — optional
│       │   ├── ProductInfo.cs                          # ADD: FilterValues[], AppliedProductType (snapshot)
│       │   └── ProductSearchFilteredParam.cs           # NEW: storeSlug?, categorySlug, filters[{filterId, value}], pageNum
│       └── Category/
│           ├── CategoryInsertInfo.cs                   # ADD: ProductTypeId (nullable, optional on create)
│           ├── CategoryUpdateInfo.cs                   # ADD: ProductTypeId
│           ├── CategoryGlobalInsertInfo.cs             # ADD: ProductTypeId
│           ├── CategoryGlobalUpdateInfo.cs             # ADD: ProductTypeId
│           ├── CategoryInfo.cs                         # ADD: ProductTypeId, AppliedProductTypeId (resolved), AppliedProductTypeOriginCategoryId
│           └── CategoryTreeNodeInfo.cs                 # ADD: ProductTypeId (only direct, not resolved — UI shows tree literally)
├── Lofn.GraphQL/
│   ├── Public/
│   │   └── PublicQuery.cs                              # ADD: GetProductsByCategoryFiltered (paginated), expose AppliedProductType in CategoryType
│   ├── Admin/
│   │   └── AdminQuery.cs                               # ADD: GetMyProductTypes, GetMyProductType(id)
│   └── Types/
│       ├── ProductTypeExtension.cs                     # NEW: applied product type computed field on Category
│       ├── CategoryTypeExtension.cs                    # ADD: AppliedProductType resolver (closest-ancestor)
│       └── ProductTypeExtension2.cs                    # NEW: filters/customizations as nested resolvers
├── Lofn.API/
│   ├── Controllers/
│   │   ├── ProductTypeController.cs                    # NEW: CRUD + filter/group/option mgmt (TenantAdmin)
│   │   ├── CategoryController.cs                       # ADD: PUT/DELETE producttype link (StoreOwner OR TenantAdmin per scope)
│   │   ├── CategoryGlobalController.cs                 # ADD: same for global categories (TenantAdmin)
│   │   └── ProductController.cs                        # ADD: search-filtered, calculate-price endpoints
│   └── Filters/
│       └── TenantAdminAttribute.cs                     # NEW: requires only IsAdmin = true (no Marketplace mutex)
├── Lofn.Application/
│   └── Startup.cs                                      # ADD: DI for ProductTypeRepository, ProductFilterValueRepository, ProductTypeService, ProductPriceCalculator, ProductFilterValueResolver
├── Lofn.Tests/
│   ├── Validators/
│   │   ├── ProductTypeInsertInfoValidatorTests.cs      # NEW
│   │   ├── ProductTypeFilterInsertInfoValidatorTests.cs # NEW (label uniqueness, dataType, allowed values)
│   │   ├── CustomizationGroupInsertInfoValidatorTests.cs # NEW
│   │   ├── CustomizationOptionInsertInfoValidatorTests.cs # NEW
│   │   └── CategoryProductTypeLinkValidatorTests.cs    # NEW
│   └── Services/
│       ├── ProductTypeServiceTest.cs                   # NEW: CRUD + filter mgmt + customization mgmt + admin gate
│       ├── ProductFilterValueResolverTest.cs           # NEW: required/optional/datatype/enum validation
│       ├── ProductPriceCalculatorTest.cs               # NEW: base + sum(deltas), edge cases
│       ├── CategoryServiceProductTypeTest.cs           # NEW: link/unlink/closest-ancestor resolution
│       └── ProductServiceFilteredSearchTest.cs         # NEW: search by filter values + category rollup
├── Lofn.ApiTests/
│   ├── Controllers/
│   │   ├── ProductTypeControllerTests.cs               # NEW: full REST CRUD round-trip + admin gate
│   │   ├── ProductTypeCustomizationTests.cs            # NEW: customization CRUD + price calc
│   │   ├── CategoryProductTypeLinkTests.cs             # NEW: link/unlink store + global, closest-ancestor
│   │   ├── ProductFilteredSearchTests.cs               # NEW: end-to-end filtered listing with pagination
│   │   └── ProductTypeGraphQLTests.cs                  # NEW: appliedProductType, productsByCategoryFiltered
│   ├── Fixtures/
│   │   └── ApiTestFixture.cs                           # ADD: helpers for seeding type + filter + customization + linking
│   └── Helpers/
│       └── TestDataHelper.cs                           # ADD: factories for ProductType seed + filter values
└── lofn.sql                                            # UPDATE: 6 new CREATE TABLE statements + product_type_id column on lofn_categories + indexes
```

**Structure Decision**: Single-solution Clean Architecture (existing layout). No new csproj. Novos arquivos seguem nomenclatura estabelecida. Schema change é uma única SQL migration (`20260430_AddProductTypes.sql`) espelhando o padrão de feature 002. O atributo `[TenantAdmin]` é criado em `Lofn.API/Filters/` paralelo ao `[MarketplaceAdmin]` existente; semantically distinto (sem mutex de marketplace).

## Complexity Tracking

> Sem violações constitucionais — tabela omitida intencionalmente.
