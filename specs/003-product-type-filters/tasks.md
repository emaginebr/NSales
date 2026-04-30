---
description: "Task list for feature 003-product-type-filters"
---

# Tasks: Tipo de Produto, filtros e customizações

**Input**: Design documents from `specs/003-product-type-filters/`
**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/rest.md](./contracts/rest.md), [contracts/graphql.md](./contracts/graphql.md), [quickstart.md](./quickstart.md)

**Tests**: Tests are explicitly part of this feature (tests-as-contract pattern já estabelecido pelas features 001 e 002, e cada User Story da spec carrega "Independent Test"). Unit tests em `Lofn.Tests`, integration tests em `Lofn.ApiTests` contra a API live.

**Organization**: Tasks são agrupadas por user story para permitir implementação e teste independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivo diferente, sem dependência em tasks incompletas).
- **[Story]**: Label de user story (US1 / US2 / US3 / US4 / US5 / US6) — apenas para Phase 3+.
- File paths são absolute project-relative (a partir de `C:\repos\Lofn\Lofn\`).

## Path Conventions

Single-solution Clean Architecture (mesmo layout das features 001 e 002 — sem novo csproj). Backend roots:

- `Lofn.Infra/` — entities, EF mapping, repositories, migrations
- `Lofn.Infra.Interfaces/` — repository interfaces
- `Lofn.Domain/` — domain models, services, mappers, validators, interfaces, core
- `Lofn/` — DTOs (`Lofn/DTO/...`)
- `Lofn.GraphQL/` — HotChocolate query types, type extensions
- `Lofn.API/` — REST controllers, filters
- `Lofn.Application/` — DI bootstrap (`ConfigureLofn()`)
- `Lofn.Tests/` — xUnit unit tests
- `Lofn.ApiTests/` — xUnit integration tests (Flurl.Http + FluentAssertions)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Sanity-check antes de introduzir mudanças de schema.

- [X] T001 Run `dotnet build Lofn.sln` from repo root and confirm 0 errors as a green baseline. Capture warning count for delta tracking.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Schema, entities, models, base DTOs, mappers, repositories e DI que TODAS as user stories dependem.

**⚠️ CRITICAL**: Nenhum trabalho de user story pode começar até esta phase terminar.

### DDL & schema

- [X] T002 Author `Lofn.Infra/Migrations/20260430_AddProductTypes.sql` com o bloco DDL idempotente derivado de `data-model.md`: 6 tabelas novas (`lofn_product_types`, `lofn_product_type_filters`, `lofn_product_type_filter_allowed_values`, `lofn_product_type_customization_groups`, `lofn_product_type_customization_options`, `lofn_product_filter_values`) com PKs, FKs com ON DELETE CASCADE/SET NULL conforme data-model, indexes (UKs em `(product_type_id, label)`, `(filter_id, value)` em allowed_values, `(group_id, label)`, `(product_id, filter_id)` UK no value-table; IDX em `(filter_id, value)`); ALTER TABLE `lofn_categories` ADD COLUMN `product_type_id BIGINT NULL` + FK `fk_lofn_categories_product_type ON DELETE SET NULL` + IDX `ix_lofn_categories_product_type_id`.
- [X] T003 Update `lofn.sql` (repo root) para que o bootstrap inicial inclua todas as 6 tabelas novas E a coluna `product_type_id` na tabela `lofn_categories`. Tenants novos devem nascer com o schema completo já presente.

### EF entities (Lofn.Infra/Context/)

- [X] T004 [P] Create `Lofn.Infra/Context/ProductType.cs` partial entity: `ProductTypeId (long)`, `Name (string)`, `Description (string?)`, `CreatedAt`, `UpdatedAt`, navigation collections `Filters` (ICollection<ProductTypeFilter>) e `CustomizationGroups` (ICollection<ProductTypeCustomizationGroup>).
- [X] T005 [P] Create `Lofn.Infra/Context/ProductTypeFilter.cs`: `FilterId`, `ProductTypeId` + nav `ProductType`, `Label`, `DataType`, `IsRequired`, `DisplayOrder`, `CreatedAt`, `UpdatedAt`, navigation collection `AllowedValues` (ICollection<ProductTypeFilterAllowedValue>).
- [X] T006 [P] Create `Lofn.Infra/Context/ProductTypeFilterAllowedValue.cs`: `AllowedValueId`, `FilterId` + nav `Filter`, `Value`, `DisplayOrder`.
- [X] T007 [P] Create `Lofn.Infra/Context/ProductTypeCustomizationGroup.cs`: `GroupId`, `ProductTypeId` + nav `ProductType`, `Label`, `SelectionMode`, `IsRequired`, `DisplayOrder`, `CreatedAt`, `UpdatedAt`, navigation collection `Options` (ICollection<ProductTypeCustomizationOption>).
- [X] T008 [P] Create `Lofn.Infra/Context/ProductTypeCustomizationOption.cs`: `OptionId`, `GroupId` + nav `Group`, `Label`, `PriceDeltaCents (long)`, `IsDefault`, `DisplayOrder`, `CreatedAt`, `UpdatedAt`.
- [X] T009 [P] Create `Lofn.Infra/Context/ProductFilterValue.cs`: `ProductFilterValueId`, `ProductId` + nav `Product`, `FilterId` + nav `Filter`, `Value (string)`, `CreatedAt`, `UpdatedAt`.

### Existing entity alterations

- [X] T010 Add `public long? ProductTypeId { get; set; }` and `public virtual ProductType ProductType { get; set; }` to `Lofn.Infra/Context/Category.cs`.
- [X] T011 Add `public virtual ICollection<ProductFilterValue> FilterValues { get; set; } = new List<ProductFilterValue>();` to `Lofn.Infra/Context/Product.cs`.

### LofnContext mapping

- [X] T012 Update `Lofn.Infra/Context/LofnContext.cs`: declare 6 new `DbSet<>` properties; in `OnModelCreating` map all 6 entities (table name, PK, columns, FKs with cascade rules per data-model), plus the `Category.ProductTypeId` FK to `ProductType` with `OnDelete(DeleteBehavior.SetNull)` and the `Product.FilterValues` navigation. Configure all UKs and IDXs explicitly. Depends on T004–T011.

### Domain models (Lofn.Domain/Models/)

- [X] T013 [P] Create `Lofn.Domain/Models/ProductTypeModel.cs`: `ProductTypeId`, `Name`, `Description`, `CreatedAt`, `UpdatedAt`, lists `Filters: List<ProductTypeFilterModel>`, `CustomizationGroups: List<ProductTypeCustomizationGroupModel>`.
- [X] T014 [P] Create `Lofn.Domain/Models/ProductTypeFilterModel.cs`: `FilterId`, `ProductTypeId`, `Label`, `DataType (string)`, `IsRequired`, `DisplayOrder`, list `AllowedValues: List<string>` (collapsed from entity for domain ergonomics).
- [X] T015 [P] Create `Lofn.Domain/Models/ProductTypeCustomizationGroupModel.cs`: `GroupId`, `ProductTypeId`, `Label`, `SelectionMode (string)`, `IsRequired`, `DisplayOrder`, list `Options: List<ProductTypeCustomizationOptionModel>`.
- [X] T016 [P] Create `Lofn.Domain/Models/ProductTypeCustomizationOptionModel.cs`: `OptionId`, `GroupId`, `Label`, `PriceDeltaCents (long)`, `IsDefault`, `DisplayOrder`.
- [X] T017 [P] Create `Lofn.Domain/Models/ProductFilterValueModel.cs`: `ProductFilterValueId`, `ProductId`, `FilterId`, `FilterLabel (string)`, `DataType (string)`, `Value (string)`.

### Existing model alterations

- [X] T018 Add `public long? ProductTypeId { get; set; }` to `Lofn.Domain/Models/CategoryModel.cs`.
- [X] T019 Add `public IList<ProductFilterValueModel> FilterValues { get; set; } = new List<ProductFilterValueModel>();` to `Lofn.Domain/Models/ProductModel.cs`.

### Infra mappers (Lofn.Infra/Mappers/)

- [X] T020 [P] Create `Lofn.Infra/Mappers/ProductTypeDbMapper.cs` with static methods: `ToModel(ProductType)`, `ToInsertEntity(ProductTypeModel)`, `ApplyToEntity(ProductTypeModel, ProductType)`. Internally maps Filters tree (filter row + allowed_values) and CustomizationGroups tree (group + options).
- [X] T021 [P] Create `Lofn.Infra/Mappers/ProductFilterValueDbMapper.cs` with `ToModel(ProductFilterValue, ProductTypeFilter)`, `ToInsertEntity(ProductFilterValueModel)`, `ApplyToEntity(ProductFilterValueModel, ProductFilterValue)`.
- [X] T022 Update `Lofn.Infra/Mappers/CategoryDbMapper.cs` para round-trip `ProductTypeId` em `ToModel` e `ToEntity`. Depends on T010 + T018.

### Domain mappers (Lofn.Domain/Mappers/)

- [X] T023 [P] Create `Lofn.Domain/Mappers/ProductTypeMapper.cs` com:
   - `ToInfo(ProductTypeModel)` → `ProductTypeInfo`
   - `ToInsertModel(ProductTypeInsertInfo)` → `ProductTypeModel`
   - `ToUpdateModel(ProductTypeUpdateInfo, existing)` → `ProductTypeModel`
   - `ToFilterInfo(ProductTypeFilterModel)` → `ProductTypeFilterInfo`
   - `ToCustomizationGroupInfo(ProductTypeCustomizationGroupModel)` → `CustomizationGroupInfo`
   - `ToCustomizationOptionInfo(ProductTypeCustomizationOptionModel)` → `CustomizationOptionInfo`
   Stub the methods returning `null` initially — DTOs criados nas phases seguintes; build deve passar.
- [X] T024 [P] Create `Lofn.Domain/Mappers/ProductFilterValueMapper.cs` com `ToInfo(ProductFilterValueModel)` → `ProductFilterValueInfo` (stub).
- [X] T025 Update `Lofn.Domain/Mappers/ProductMapper.cs`: `ToInfo` deve popular `FilterValues` (lista de `ProductFilterValueInfo`) e o campo `AppliedProductTypeId`. Adicionar overload `ToInfo(ProductModel md, ProductTypeModel? appliedType)` para receber o tipo aplicável resolvido pelo service. Depends on T019 + T024.

### Existing DTO alterations (Lofn/DTO/Category/)

- [X] T026 [P] Add `[JsonPropertyName("productTypeId")] public long? ProductTypeId { get; set; }` to `Lofn/DTO/Category/CategoryInsertInfo.cs`.
- [X] T027 [P] Add `[JsonPropertyName("productTypeId")] public long? ProductTypeId { get; set; }` to `Lofn/DTO/Category/CategoryUpdateInfo.cs`.
- [X] T028 [P] Add `[JsonPropertyName("productTypeId")] public long? ProductTypeId { get; set; }` to `Lofn/DTO/Category/CategoryGlobalInsertInfo.cs`.
- [X] T029 [P] Add `[JsonPropertyName("productTypeId")] public long? ProductTypeId { get; set; }` to `Lofn/DTO/Category/CategoryGlobalUpdateInfo.cs`.
- [X] T030 [P] Add three properties to `Lofn/DTO/Category/CategoryInfo.cs`: `ProductTypeId (long?)`, `AppliedProductTypeId (long?)`, `AppliedProductTypeOriginCategoryId (long?)`. All `[JsonPropertyName]` snake/camel matched.
- [X] T031 [P] Add `ProductTypeId (long?)` to `Lofn/DTO/Category/CategoryTreeNodeInfo.cs` (tree exibe apenas vínculo direto, não o resolvido).

### New DTO scaffolding (Lofn/DTO/ProductType/) — empty shells, populated by their Story phases

- [X] T032 [P] Create empty class shell `Lofn/DTO/ProductType/ProductTypeInsertInfo.cs` com namespace `Lofn.DTO.ProductType` — fields adicionados em T053 (US1).
- [X] T033 [P] Same for `ProductTypeUpdateInfo.cs`.
- [X] T034 [P] Same for `ProductTypeInfo.cs`.
- [X] T035 [P] Same for `ProductTypeFilterInsertInfo.cs`, `ProductTypeFilterUpdateInfo.cs`, `ProductTypeFilterInfo.cs`.
- [X] T036 [P] Same for `CustomizationGroupInsertInfo.cs`, `CustomizationGroupUpdateInfo.cs`, `CustomizationGroupInfo.cs`.
- [X] T037 [P] Same for `CustomizationOptionInsertInfo.cs`, `CustomizationOptionUpdateInfo.cs`, `CustomizationOptionInfo.cs`.
- [X] T038 [P] Same for `ProductFilterValueInfo.cs`, `ProductPriceCalculationRequest.cs`, `ProductPriceCalculationResult.cs`.

### Existing Product DTO alterations

- [X] T039 [P] Add `public IList<ProductFilterValueAssign>? FilterValues { get; set; }` to `Lofn/DTO/Product/ProductInsertInfo.cs`. Create `Lofn/DTO/Product/ProductFilterValueAssign.cs` with `FilterId (long)`, `Value (string)`.
- [X] T040 [P] Same: add `FilterValues` to `Lofn/DTO/Product/ProductUpdateInfo.cs`.
- [X] T041 [P] Add `IList<ProductFilterValueInfo> FilterValues` and `long? AppliedProductTypeId` to `Lofn/DTO/Product/ProductInfo.cs`.
- [X] T042 [P] Create `Lofn/DTO/Product/ProductSearchFilteredParam.cs`: `StoreSlug (string?)`, `CategorySlug (string)`, `Filters (IList<ProductFilterValueAssign>?)`, `PageNum (int)`.

### Repository interfaces (Lofn.Infra.Interfaces/Repository/)

- [X] T043 [P] Create `Lofn.Infra.Interfaces/Repository/IProductTypeRepository.cs` with method signatures used across all stories: `GetByIdAsync`, `GetByNameAsync`, `ListAllAsync`, `InsertAsync(ProductTypeModel)`, `UpdateAsync`, `DeleteAsync`, e métodos para filtros (`InsertFilterAsync`, `UpdateFilterAsync`, `DeleteFilterAsync`, `ReplaceAllowedValuesAsync(filterId, values)`) e customizações (`InsertGroupAsync`, `UpdateGroupAsync`, `DeleteGroupAsync`, `InsertOptionAsync`, `UpdateOptionAsync`, `DeleteOptionAsync`).
- [X] T044 [P] Create `Lofn.Infra.Interfaces/Repository/IProductFilterValueRepository.cs` with `GetByProductAsync(productId)`, `ReplaceForProductAsync(productId, IList<ProductFilterValueModel>)` (idempotent reconcile).
- [X] T045 Add to `Lofn.Infra.Interfaces/Repository/ICategoryRepository.cs`:
   - `Task<CategoryModel?> GetAppliedProductTypeAsync(long categoryId)` — retorna o ancestral mais próximo com `ProductTypeId != null`, ou null. Inclui `ProductTypeId` no model retornado e `OriginCategoryId` derivable from CategoryId.
   - `Task UpdateProductTypeIdAsync(long categoryId, long? productTypeId)` — para link/unlink.
- [X] T046 Add to `Lofn.Infra.Interfaces/Repository/IProductRepository.cs`: `Task<(IList<ProductModel> Items, int PageCount, int TotalItems)> SearchByFilterValuesAsync(long? storeId, long categoryId, IList<long> categoryIdsRollup, IList<(long FilterId, string Value)> filters, int pageNum)`.

### Repository implementations (Lofn.Infra/Repository/)

- [X] T047 [P] Create `Lofn.Infra/Repository/ProductTypeRepository.cs` implementando `IProductTypeRepository`. Usa EF Core com Include para carregar Filters (+ AllowedValues) e CustomizationGroups (+ Options) na leitura. Em insert/update/replace usa `_context.Database.BeginTransactionAsync()` para garantir atomicidade da árvore. Depends on T012 + T013–T016 + T020 + T043.
- [X] T048 [P] Create `Lofn.Infra/Repository/ProductFilterValueRepository.cs` implementando `IProductFilterValueRepository`. Em `ReplaceForProductAsync`: dentro de uma transação, lê valores atuais do produto, computa diff (insert/update/delete) e aplica em uma única SaveChanges. Depends on T012 + T017 + T021 + T044.
- [X] T049 Add `GetAppliedProductTypeAsync` a `Lofn.Infra/Repository/CategoryRepository.cs`. Reusa `GetAncestorChainAsync` da feature 002 (ordenado raiz→nó); itera começando do nó atual em direção à raiz e retorna o primeiro com `ProductTypeId != null`, populando `OriginCategoryId = ancestor.CategoryId`. Adiciona também `UpdateProductTypeIdAsync` que faz `_context.Categories.Where(c => c.CategoryId == id).ExecuteUpdateAsync(b => b.SetProperty(c => c.ProductTypeId, productTypeId))`. Depends on T010 + T018 + T022 + T045.
- [X] T050 Add `SearchByFilterValuesAsync` a `Lofn.Infra/Repository/ProductRepository.cs`. Query base: `_context.Products.Where(p => categoryIdsRollup.Contains(p.CategoryId.Value))`. Para cada `(FilterId, Value)` em `filters`, AND com `_context.ProductFilterValues.Any(pfv => pfv.ProductId == p.ProductId && pfv.FilterId == fId && pfv.Value == v)` (subquery EXISTS). Aplica `OrderBy(p => p.ProductId)` para paginação determinística. Depends on T012 + T011 + T046.

### Authorization filter (Lofn.API/Filters/)

- [X] T051 [P] Create `Lofn.API/Filters/TenantAdminAttribute.cs` que herda `Attribute, IAuthorizationFilter`. Em `OnAuthorization(AuthorizationFilterContext ctx)` verifica `ctx.HttpContext.User.Claims` por claim `IsAdmin = "true"` (caso-sensível matching da feature 001); caso falte, set `ctx.Result = new ForbidResult()`. NÃO checa `Marketplace` (diferença explícita do `[MarketplaceAdmin]` existente). (Implemented em `Lofn.Application/Authorization/TenantAdminAttribute.cs` — alinhado com `MarketplaceAdminAttribute` existente.)

### DI registration (Lofn.Application/Startup.cs)

- [X] T052 Update `Lofn.Application/Startup.cs`: register new services in DI. Adicionar (em região `#region Repository`): `injectDependency(typeof(IProductTypeRepository), typeof(ProductTypeRepository), services, scoped)`, `injectDependency(typeof(IProductFilterValueRepository), typeof(ProductFilterValueRepository), services, scoped)`. Outras services (ProductTypeService, ProductFilterValueResolver, ProductPriceCalculator) ficam para os tasks de seus respectivos US phases. Depends on T047 + T048.

### Foundation checkpoint

- [X] T053 Run `dotnet build Lofn.sln` — must return 0 errors. Compatibilidade aditiva: nenhum teste existente pode quebrar (existing 105 unit + 66 ApiTests passam sem modificação). ✅ 0 erros / 109 warnings (todos pré-existentes).

**Checkpoint**: Schema, entities, models, repositories base, DI, atributo de autorização e DTOs base/extensões prontos. User-story work pode começar.

---

## Phase 3: User Story 1 — Admin define um Tipo de Produto e o esquema de filtros (Priority: P1) 🎯 MVP foundation

**Goal**: Admin (`IsAdmin = true`) cria/atualiza/deleta Tipos de Produto e cadastra filtros (text/integer/decimal/boolean/enum com allowed_values) sob esses tipos. Operações idempotentes na lista de filtros (label único por tipo). Não-admin recusado com 403.

**Independent Test**: Login como admin, POST `/producttype/insert` para "Calçado", POST `/producttype/{typeId}/filter/insert` 4×, GET `/producttype/{typeId}` confirma a árvore. Repete o filter/insert com label duplicado → 422. Tenta com não-admin → 403.

### DTOs (US1)

- [X] T054 [US1] Populate `Lofn/DTO/ProductType/ProductTypeInsertInfo.cs`: `Name (string)`, `Description (string?)`. Dependencies on T032.
- [X] T055 [US1] Populate `Lofn/DTO/ProductType/ProductTypeUpdateInfo.cs`: `ProductTypeId (long)`, `Name (string)`, `Description (string?)`. Dependencies on T033.
- [X] T056 [US1] Populate `Lofn/DTO/ProductType/ProductTypeInfo.cs`: `ProductTypeId`, `Name`, `Description`, `Filters (IList<ProductTypeFilterInfo>)`, `CustomizationGroups (IList<CustomizationGroupInfo>)`, `CreatedAt`, `UpdatedAt`. Dependencies on T034, T035, T036.
- [X] T057 [US1] Populate `Lofn/DTO/ProductType/ProductTypeFilterInsertInfo.cs`: `Label`, `DataType (string)`, `IsRequired`, `DisplayOrder`, `AllowedValues (IList<string>?)`. Dependencies on T035.
- [X] T058 [US1] Populate `Lofn/DTO/ProductType/ProductTypeFilterUpdateInfo.cs`: `FilterId`, `Label`, `IsRequired`, `DisplayOrder`, `AllowedValues (IList<string>?)`. Sem `DataType` (regra: imutável). Dependencies on T035.
- [X] T059 [US1] Populate `Lofn/DTO/ProductType/ProductTypeFilterInfo.cs`: `FilterId`, `ProductTypeId`, `Label`, `DataType`, `IsRequired`, `DisplayOrder`, `AllowedValues (IList<string>)`. Dependencies on T035.

### Domain mapper completion (US1)

- [X] T060 [US1] Replace stubs in `Lofn.Domain/Mappers/ProductTypeMapper.cs`: implementar `ToInfo`, `ToInsertModel`, `ToUpdateModel` para Type + Filter (sem Customization ainda). Customization mappings continuam stub. Depends on T054–T059.

### Validators (US1)

- [X] T061 [P] [US1] Create `Lofn.Domain/Validators/ProductTypeInsertInfoValidator.cs`: `Name` NotEmpty + MaxLength(120). `Description` MaxLength(500).
- [X] T062 [P] [US1] Create `Lofn.Domain/Validators/ProductTypeUpdateInfoValidator.cs`: `ProductTypeId > 0`, mesmas regras de Name/Description.
- [X] T063 [P] [US1] Create `Lofn.Domain/Validators/ProductTypeFilterInsertInfoValidator.cs`: `Label` NotEmpty + MaxLength(120); `DataType` em {text, integer, decimal, boolean, enum}; `DisplayOrder >= 0`; quando `DataType = enum`, `AllowedValues` NotEmpty + MinElements(1) + AllUnique + cada item NotEmpty + MaxLength(120).
- [X] T064 [P] [US1] Create `Lofn.Domain/Validators/ProductTypeFilterUpdateInfoValidator.cs`: `FilterId > 0`, regras de Label/DisplayOrder; `AllowedValues` opcional mas quando presente segue mesmas regras de uniqueness.

### Service interface + implementation (US1)

- [X] T065 [US1] Create `Lofn.Domain/Interfaces/IProductTypeService.cs` com (por enquanto, customizações ficam nas phases seguintes):
   - `Task<ProductTypeModel> InsertAsync(ProductTypeInsertInfo)`
   - `Task<ProductTypeModel> UpdateAsync(ProductTypeUpdateInfo)`
   - `Task DeleteAsync(long productTypeId)`
   - `Task<ProductTypeModel?> GetByIdAsync(long productTypeId)`
   - `Task<IList<ProductTypeModel>> ListAllAsync()`
   - `Task<ProductTypeFilterModel> InsertFilterAsync(long productTypeId, ProductTypeFilterInsertInfo)`
   - `Task<ProductTypeFilterModel> UpdateFilterAsync(ProductTypeFilterUpdateInfo)`
   - `Task DeleteFilterAsync(long filterId)`
- [X] T066 [US1] Create `Lofn.Domain/Services/ProductTypeService.cs` implementando os métodos acima. Lógica:
   - `InsertAsync`: valida via `IValidator<ProductTypeInsertInfo>`, checa unicidade de `Name` no tenant via repo (`GetByNameAsync != null` → ValidationException com mensagem "Product type name already exists").
   - `UpdateAsync`: same + load existing + reject if name collides com outro id.
   - `InsertFilterAsync`: valida + checa unicidade de `Label` por tipo (UK do banco também enforce; service captura `DbUpdateException` e traduz). Quando `DataType = enum` chama `repo.ReplaceAllowedValuesAsync(filterId, values)` em sequência.
   - `UpdateFilterAsync`: valida + recusa mudança de DataType (load existing, compara) + atualiza label/required/displayOrder + se enum, replace allowed values.
   - `DeleteFilterAsync`: cascade no banco (ON DELETE CASCADE) — service só chama repo.
   Depends on T047 + T060 + T061–T064.

### REST controller (US1)

- [X] T067 [US1] Create `Lofn.API/Controllers/ProductTypeController.cs` com rota base `/producttype`, atributo `[Authorize]` no controller e `[TenantAdmin]` em cada action. Endpoints:
   - `POST insert` → `IProductTypeService.InsertAsync` → `200 OK` com `ProductTypeInfo`
   - `POST update` → `UpdateAsync`
   - `DELETE delete/{productTypeId}` → `DeleteAsync` → `204`
   - `GET list` → `ListAllAsync` → `IList<ProductTypeInfo>`
   - `GET {productTypeId}` → `GetByIdAsync` → `ProductTypeInfo` ou `404`
   - `POST {productTypeId}/filter/insert` → `InsertFilterAsync`
   - `POST filter/update` → `UpdateFilterAsync`
   - `DELETE filter/delete/{filterId}` → `DeleteFilterAsync` → `204`
   Cada action mapeia model → `ProductTypeMapper.ToInfo`. Trata `ValidationException` → `422`.

### DI registration (US1)

- [X] T068 [US1] Update `Lofn.Application/Startup.cs`: adicionar em `#region Service`: `injectDependency(typeof(IProductTypeService), typeof(ProductTypeService), services, scoped)`. Validators são auto-registrados via `AddValidatorsFromAssemblyContaining<ShopCartInfoValidator>` existente — confirmar que pega os 4 novos validators (estão na mesma assembly).

### Build green (US1)

- [X] T069 [US1] Run `dotnet build Lofn.sln` — 0 errors. ✅ 0 erros / 122 warnings (todos pré-existentes).

### Unit tests (US1)

- [X] T070 [P] [US1] Create `Lofn.Tests/Domain/Validators/ProductTypeInsertInfoValidatorTests.cs` com cases: empty name → fail; valid name → pass; description > 500 → fail.
- [X] T071 [P] [US1] Create `Lofn.Tests/Domain/Validators/ProductTypeUpdateInfoValidatorTests.cs`.
- [X] T072 [P] [US1] Create `Lofn.Tests/Domain/Validators/ProductTypeFilterInsertInfoValidatorTests.cs` cobrindo: dataType inválido, enum sem allowedValues, enum com duplicate, label vazio.
- [X] T073 [P] [US1] Create `Lofn.Tests/Domain/Validators/ProductTypeFilterUpdateInfoValidatorTests.cs`.
- [X] T074 [P] [US1] Create `Lofn.Tests/Domain/Services/ProductTypeServiceTest.cs` com mocks para `IProductTypeRepository` e validators. Cases: insert ok; insert com nome duplicado → ValidationException; update muda DataType → recusa; insertFilter enum sem allowedValues → recusa; deleteFilter chama repo. ✅ 138/138 unit tests aprovados (33 novos + 105 pré-existentes).

### Integration tests (US1)

- [X] T075 [P] [US1] Create `Lofn.ApiTests/Controllers/ProductTypeControllerTests.cs` com fixture compartilhada. Cases (mínimo 8):
   - admin insert → 200, GET por id retorna o tipo
   - non-admin insert → 403
   - sem token → 401
   - update muda nome → 200
   - delete remove → subsequente GET 404
   - duplicate name → 422
   - filter insert enum com allowedValues → ok; filter GET retorna allowedValues populados
   - filter update tentando mudar DataType → 422
- [X] T076 [P] [US1] Update `Lofn.ApiTests/Fixtures/ApiTestFixture.cs` com helper `Task<long> SeedProductTypeAsync(string name)` e `Task<long> SeedProductTypeFilterAsync(long typeId, string label, string dataType, string[]? allowedValues = null)`.

**Checkpoint**: Tipo de Produto e Filtros via REST funcionando. Testes verdes (unit + ApiTests específicos).

---

## Phase 4: User Story 2 — Admin vincula Tipo a Categoria + closest-ancestor (Priority: P1)

**Goal**: Admin vincula `ProductType` a uma `Category` (link 0..1 via `ProductTypeId` na tabela). Categoria filha sem vínculo direto resolve via closest-ancestor. Endpoint `GET /category/{categoryId}/producttype/applied` é anônimo e retorna o resolved type.

**Independent Test**: Setup um tipo (US1), PUT vínculo na categoria pai, GET applied na categoria filha → retorna o tipo do pai. DELETE vínculo, GET applied → null. Tenta vincular como não-admin → 403.

### CategoryService extension

- [X] T077 [US2] Add to `Lofn.Domain/Interfaces/ICategoryService.cs`:
   - `Task LinkProductTypeAsync(long categoryId, long productTypeId, long userId)` — valida que categoria existe, que tipo existe, que user é tenant admin (verificado no controller, mas service revalida).
   - `Task UnlinkProductTypeAsync(long categoryId, long userId)`.
   - `Task<(ProductTypeModel ProductType, long OriginCategoryId)?> GetAppliedProductTypeAsync(long categoryId)` — null quando nenhum ancestral tem tipo.
- [X] T078 [US2] Implement those methods em `Lofn.Domain/Services/CategoryService.cs`. `LinkProductTypeAsync` chama `_categoryRepository.UpdateProductTypeIdAsync(categoryId, productTypeId)` após validar existência via `GetByIdAsync`. `GetAppliedProductTypeAsync` chama `_categoryRepository.GetAppliedProductTypeAsync(categoryId)` e em seguida `_productTypeRepository.GetByIdAsync(...)` (com Include de filters/groups/options) para hidratar o tipo completo. Depends on T049 + T065.
- [X] T079 [US2] Inject `IProductTypeRepository` em `CategoryService` constructor (atualizar DI registration se preciso). T052 já registrou o repo.

### REST controller extension

- [X] T080 [US2] Add to `Lofn.API/Controllers/CategoryController.cs`:
   - `[HttpPut("{categoryId:long}/producttype/{productTypeId:long}")] [TenantAdmin]` → `LinkProductTypeAsync`.
   - `[HttpDelete("{categoryId:long}/producttype")] [TenantAdmin]` → `UnlinkProductTypeAsync`.
   - `[HttpGet("{categoryId:long}/producttype/applied")] [AllowAnonymous]` → `GetAppliedProductTypeAsync`. Retorna `{ appliedProductTypeId, originCategoryId, productType: ProductTypeInfo }` ou `null`.
- [X] T081 [US2] Add same triple endpoints to `Lofn.API/Controllers/CategoryGlobalController.cs` para categorias globais (rota: `/category-global/{categoryId}/producttype/{productTypeId}` etc.). Reuso do mesmo `ICategoryService.LinkProductTypeAsync`.

### Existing DTO/mapper updates

- [X] T082 [US2] Update `Lofn.Domain/Mappers/CategoryMapper.cs`: `ToInfo(CategoryModel)` mapeia `ProductTypeId` para o DTO. `AppliedProductTypeId` e `AppliedProductTypeOriginCategoryId` ficam null aqui — o service os populates separadamente quando relevante (ver T083).
- [X] T083 [US2] Add overload `Lofn.Domain.Mappers.CategoryMapper.ToInfo(CategoryModel md, (long? appliedTypeId, long? originCategoryId)? applied)` que popula esses dois campos quando passado. ICategoryService.GetByIdAsync (existente) ganha sobrecarga ou flag `bool resolveAppliedType = false` para chamar o resolver. Public/admin GraphQL usam essa via.

### GraphQL extension

- [X] T084 [US2] Update `Lofn.GraphQL/Types/CategoryTypeExtension.cs` adicionando field resolver `[BindMember]` `GetAppliedProductType([Parent] Category parent, [Service] ICategoryService svc)` que chama `svc.GetAppliedProductTypeAsync(parent.CategoryId)`. Expõe também `productTypeId` direto e `appliedProductTypeOriginCategoryId`. Map ProductTypeModel → ProductType via HotChocolate auto-mapping (entidade já existe via T004).
- [X] T085 [US2] Confirm `Category` GraphQL type já está em ambos os schemas (Public + Admin) — não precisa duplicar a extension.

### Unit tests (US2)

- [X] T086 [P] [US2] Create `Lofn.Tests/Domain/Services/CategoryServiceProductTypeTest.cs` com mocks. Cases: LinkProductTypeAsync com categoria inexistente → throws; LinkProductTypeAsync com tipo inexistente → throws; UnlinkProductTypeAsync chama UpdateProductTypeIdAsync(id, null); GetAppliedProductTypeAsync com categoria sem ancestral tipado → null; com ancestral tipado → retorna o tipo + originCategoryId. ✅ 144/144 unit tests aprovados (6 novos US2).

### Integration tests (US2)

- [X] T087 [P] [US2] Create `Lofn.ApiTests/Controllers/CategoryProductTypeLinkTests.cs` com cases:
   - PUT link como admin → 200 + body com productTypeId atualizado.
   - PUT link como non-admin → 403.
   - DELETE unlink → 200 + productTypeId null.
   - GET applied (anonymous) em categoria filha sem tipo direto mas com pai tipado → retorna tipo do pai + originCategoryId = pai.
   - GET applied em categoria sem tipo direto e sem ancestral tipado → null.
   - PUT link em categoria global via `/category-global/{id}/producttype/{typeId}` ok no marketplace.
- [X] T088 [P] [US2] Update `Lofn.ApiTests/Fixtures/ApiTestFixture.cs` com helper `Task LinkCategoryToProductTypeAsync(long categoryId, long productTypeId)` para reuso em testes seguintes.

**Checkpoint**: Vínculo + closest-ancestor funcionando via REST e GraphQL Public.

---

## Phase 5: User Story 3 — Vendedor cadastra produto com filterValues (Priority: P1)

**Goal**: Quando categoria do produto tem tipo aplicável, vendedor envia `filterValues: [{filterId, value}]` no insert/update. Service valida (obrigatórios, datatype, enum allowedValues), descarta extras silenciosamente. Produto sem tipo aplicável segue fluxo legado.

**Independent Test**: Setup tipo + filtros (US1) + link na categoria (US2). POST `/product/.../insert` com 3 filterValues válidos → 200 com `filterValues[]` populado e `appliedProductTypeId`. Faltando obrigatório → 422 listando faltantes. Enum value fora → 422. FilterId fora do tipo → ignorado, log warn, response sem ele.

### Domain service: ProductFilterValueResolver

- [X] T089 [US3] Create `Lofn.Domain/Services/ProductFilterValueResolver.cs` com:
   - `Task<(IList<ProductFilterValueModel> Resolved, IList<long> IgnoredFilterIds, IList<string> MissingRequiredLabels)> ResolveAsync(long categoryId, IList<(long FilterId, string Value)> input)`.
   - Carrega tipo aplicável via `ICategoryService.GetAppliedProductTypeAsync`. Se null → returns todos como `IgnoredFilterIds`, `MissingRequiredLabels = []`.
   - Para cada par input: confere se filterId existe no schema. Se não → adiciona a `IgnoredFilterIds`.
   - Para cada filtro do schema: se `IsRequired && não está em input` → adiciona o label a `MissingRequiredLabels`.
   - Para cada par válido: valida `value` por `DataType` (`text` aceita anything; `integer` testa `long.TryParse`; `decimal` testa `decimal.TryParse(InvariantCulture)`; `boolean` aceita "true"/"false"; `enum` confere em `AllowedValues`).
   - Retorna `Resolved` populated. Caller (`ProductService`) lança `ValidationException` se `MissingRequiredLabels` not empty ou se algum value falhou parse.
- [X] T090 [US3] Register `ProductFilterValueResolver` em `Lofn.Application/Startup.cs` (`#region Service`: `services.AddScoped<ProductFilterValueResolver>()` — sem interface, é util concreto). Inject `ICategoryService` e o resolver depende dele.

### ProductService extension

- [X] T091 [US3] Update `Lofn.Domain/Services/ProductService.cs`:
   - Inject `ProductFilterValueResolver` e `IProductFilterValueRepository`.
   - `InsertAsync(ProductInsertInfo)`: depois de criar o ProductModel, se `info.FilterValues != null && info.FilterValues.Any()`, chama `_resolver.ResolveAsync(categoryId, info.FilterValues.Select(...))`. Trata MissingRequired/parse errors com ValidationException listando todos. Após sucesso, chama `_filterValueRepo.ReplaceForProductAsync(productId, resolved)`. Persistência atômica via UoW.
   - `UpdateAsync(ProductUpdateInfo)`: similar; reconcile values.
   - `GetByIdAsync(long productId)`: hidrata `FilterValues` na ProductModel via repo.
- [X] T092 [US3] Update `Lofn.Domain/Mappers/ProductMapper.cs.ToInfo`: lê `model.FilterValues` e mapeia para `IList<ProductFilterValueInfo>`. Popula `AppliedProductTypeId` se overload aceitar (vide T025). Depends on T024 + T091.

### Integration tests (US3)

- [X] T093 [P] [US3] Create `Lofn.Tests/Domain/Services/ProductFilterValueResolverTest.cs` com cases:
   - categoria sem tipo aplicável → tudo ignorado, missing vazio.
   - tipo com filtro `enum required = "Cor"`, input vazio → MissingRequired = ["Cor"].
   - tipo com filtro `integer`, input "abc" → throws com erro de parse.
   - tipo com filtro `enum` com allowed [A, B], input "C" → throws.
   - input contém filterId que não existe no schema → ignorado, present em IgnoredFilterIds.
- [ ] T094 [P] [US3] Create `Lofn.Tests/Domain/Services/ProductServiceFilterValuesTest.cs`: mocks de `ProductFilterValueResolver` + repos. Cases:
   - InsertAsync com filterValues válidos → produto criado, repo.ReplaceForProductAsync chamado com a lista resolved.
   - InsertAsync com missing required → throws ValidationException antes de criar produto.
   - UpdateAsync substitui valores existentes.
   - InsertAsync sem filterValues e categoria sem tipo aplicável → produto criado normalmente sem chamar repo.

- [ ] T095 [P] [US3] Create `Lofn.ApiTests/Controllers/ProductCreateWithFilterValuesTests.cs` (integration). Setup: tipo + filtros + categoria linkada. Cases:
   - POST insert com 3 valores válidos → 200, body inclui filterValues e appliedProductTypeId.
   - POST insert sem filtro obrigatório → 422 mencionando o label faltante.
   - POST insert com enum value inválido → 422.
   - POST insert com filterId desconhecido → 200, response não inclui esse id.
   - POST update muda valores → 200, GET subsequente reflete.
   - POST insert em categoria SEM tipo aplicável → 200 mesmo sem filterValues.

**Checkpoint**: Produto persiste filter values, validação cobre obrigatório/datatype/enum/extras. Pronto para listagem filtrada.

---

## Phase 6: User Story 4 — Comprador filtra catálogo por categoria + atributos (Priority: P1) 🎯 P1 catálogo MVP

**Goal**: `POST /product/search-filtered` (anônimo) recebe `categorySlug + filters[]` e retorna produtos paginados que satisfaem AND de filtros. Aplica rollup pai⇡filho da feature 002. Filtros desconhecidos são ignorados silenciosamente; response inclui `appliedFilters[]` e `ignoredFilterIds[]`. GraphQL public expõe a mesma operação como `productsByCategoryFiltered`.

**Independent Test**: Catálogo com produtos variados sob categoria pai e subcategoria, todos com valores de filtro distintos. Buscar por (cor=branco AND tam=42) na categoria pai → retorna apenas matches da árvore. Filtro desconhecido vem como ignored.

### Service layer

- [X] T096 [US4] Add to `Lofn.Domain/Interfaces/IProductService.cs`: `Task<ProductSearchFilteredResult> SearchFilteredAsync(ProductSearchFilteredParam param)`.
- [X] T097 [US4] Add `Lofn/DTO/Product/ProductSearchFilteredResult.cs` com `IList<ProductInfo> Products`, `int PageNum`, `int PageCount`, `int TotalItems`, `long? AppliedProductTypeId`, `IList<AppliedFilterInfo> AppliedFilters`, `IList<long> IgnoredFilterIds`. Cria também `Lofn/DTO/Product/AppliedFilterInfo.cs` com `FilterId`, `Label`, `Value`.
- [X] T098 [US4] Implement `SearchFilteredAsync` em `Lofn.Domain/Services/ProductService.cs`:
   - Resolve categoria via slug (storeSlug + categorySlug → categoryId; reuso do existing pattern).
   - Carrega árvore de descendentes via `ICategoryRepository` (rollup pai⇡filho — reusa `GetDescendantsAsync` da feature 002 + a própria categoria).
   - Resolve tipo aplicável via `_categoryService.GetAppliedProductTypeAsync(categoryId)`.
   - Filtra `param.Filters` em (i) válidos (filterId pertence ao tipo) e (ii) desconhecidos (`ignoredFilterIds`).
   - Chama `_productRepo.SearchByFilterValuesAsync(storeId, categoryId, categoryIdsRollup, validFilterPairs, pageNum)`.
   - Mapeia items → `ProductInfo` (com FilterValues hidratados); compõe `AppliedFilters[]` traduzindo filterId → label via tipo aplicável.

### REST endpoint

- [X] T099 [US4] Add to `Lofn.API/Controllers/ProductController.cs`: `[HttpPost("search-filtered")] [AllowAnonymous]` → chama `_productService.SearchFilteredAsync(param)` e retorna `ProductSearchFilteredResult`. Trata `Exception "Store not found" / "Category not found"` → `404`.

### GraphQL public

- [ ] T100 [US4] Add to `Lofn.GraphQL/Public/PublicQuery.cs` query `productsByCategoryFiltered(storeSlug: String, categorySlug: String!, filters: [FilterValueInput!], pageNum: Int = 1) -> ProductSearchFilteredPayload`. Reusa `ProductSearchFilteredParam` internamente. Define `FilterValueInput` (HotChocolate input type) e `ProductSearchFilteredPayload`/`AppliedFilter` como ObjectType extensions.
- [ ] T101 [US4] Define os HotChocolate types em `Lofn.GraphQL/Types/`: `FilterValueInputType.cs` (InputType), `ProductSearchFilteredPayloadType.cs` (ObjectType).

### Unit tests (US4)

- [ ] T102 [P] [US4] Create `Lofn.Tests/Domain/Services/ProductServiceFilteredSearchTest.cs` com mocks. Cases:
   - Filtros válidos → repo chamado com pares corretos, items retornados via mapper.
   - Filtros desconhecidos → IgnoredFilterIds populado, repo chamado só com válidos.
   - Categoria sem tipo aplicável → todos os filtros são ignorados, busca aplica só categoria.
   - Loja inexistente / categoria inexistente → throws com mensagem "Store not found" / "Category not found".

### Integration tests (US4)

- [ ] T103 [P] [US4] Create `Lofn.ApiTests/Controllers/ProductFilteredSearchTests.cs` (REST). Setup completo: 5 produtos com filtros distintos sob 2 subcategorias da mesma árvore. Cases:
   - Search no slug pai com filtro AND → retorna union dos descendentes que satisfazem.
   - Search com 0 filtros → retorna todos da árvore (paginado).
   - Search com filtro com valor não-existente → 0 produtos, paginação coerente.
   - Search com filterId desconhecido → ignorado em response.
   - Search sem `storeSlug` em categoria global (modo marketplace) → 200.
- [ ] T104 [P] [US4] Create `Lofn.ApiTests/Controllers/ProductFilteredGraphQLTests.cs`. Cases mínimas:
   - Query `productsByCategoryFiltered` retorna mesma listagem que REST.
   - Resposta inclui `appliedFilters[]` com `label` populado (não só id).
   - Anônimo (sem token) é OK.

**Checkpoint**: Listagem filtrada via REST + GraphQL pública funcionando. Catálogo MVP (US1+US2+US3+US4) completo. Decisão: stop e validate antes de US5/US6 se preferir entrega incremental.

---

## Phase 7: User Story 5 — Admin define customizações por Tipo de Produto (Priority: P2)

**Goal**: Admin cadastra grupos de customização (single/multi-select, required) e opções (label, price_delta_cents, is_default) sob um Tipo. Restrições: 1 default por grupo single; rótulos únicos por escopo. Type-only — sem override por produto.

**Independent Test**: Tipo "Equipamento" + grupo "Processador" single required + 3 opções (i3 default, i5 +500, i7 +900). GET tipo retorna tudo. Tentar inserir 2 defaults no mesmo grupo single → 422.

### DTOs (US5)

- [X] T105 [US5] Populate `Lofn/DTO/ProductType/CustomizationGroupInsertInfo.cs`: `Label`, `SelectionMode (string)`, `IsRequired (bool)`, `DisplayOrder (int)`. (T036 created shell.)
- [X] T106 [US5] Populate `Lofn/DTO/ProductType/CustomizationGroupUpdateInfo.cs`: `GroupId`, mesmos campos.
- [X] T107 [US5] Populate `Lofn/DTO/ProductType/CustomizationGroupInfo.cs`: `GroupId`, `ProductTypeId`, `Label`, `SelectionMode`, `IsRequired`, `DisplayOrder`, `Options (IList<CustomizationOptionInfo>)`.
- [X] T108 [US5] Populate `Lofn/DTO/ProductType/CustomizationOptionInsertInfo.cs`: `Label`, `PriceDeltaCents (long)`, `IsDefault (bool)`, `DisplayOrder`.
- [X] T109 [US5] Populate `Lofn/DTO/ProductType/CustomizationOptionUpdateInfo.cs`: `OptionId`, mesmos campos.
- [X] T110 [US5] Populate `Lofn/DTO/ProductType/CustomizationOptionInfo.cs`: `OptionId`, `GroupId`, `Label`, `PriceDeltaCents`, `IsDefault`, `DisplayOrder`.

### Domain mapper completion (US5)

- [X] T111 [US5] Replace stubs in `Lofn.Domain/Mappers/ProductTypeMapper.cs` for customization mappings (`ToCustomizationGroupInfo`, `ToCustomizationOptionInfo`, `ToInsertGroupModel`, `ToInsertOptionModel`, `ToUpdateGroupModel`, `ToUpdateOptionModel`). Confirma que `ToInfo` para `ProductTypeInfo` agora popula `CustomizationGroups` (T056). Depends on T105–T110.

### Validators (US5)

- [X] T112 [P] [US5] Create `Lofn.Domain/Validators/CustomizationGroupInsertInfoValidator.cs`: `Label` NotEmpty + MaxLength(120); `SelectionMode` em {single, multi}; `DisplayOrder >= 0`.
- [X] T113 [P] [US5] Create `Lofn.Domain/Validators/CustomizationGroupUpdateInfoValidator.cs`. (consolidado em arquivo único)
- [X] T114 [P] [US5] Create `Lofn.Domain/Validators/CustomizationOptionInsertInfoValidator.cs`: `Label` NotEmpty + MaxLength(120); `PriceDeltaCents` (qualquer long); `DisplayOrder >= 0`.
- [X] T115 [P] [US5] Create `Lofn.Domain/Validators/CustomizationOptionUpdateInfoValidator.cs`. (consolidado em arquivo único)

### Service interface + implementation (US5)

- [X] T116 [US5] Add to `Lofn.Domain/Interfaces/IProductTypeService.cs`:
   - `Task<ProductTypeCustomizationGroupModel> InsertGroupAsync(long productTypeId, CustomizationGroupInsertInfo)`
   - `Task<ProductTypeCustomizationGroupModel> UpdateGroupAsync(CustomizationGroupUpdateInfo)`
   - `Task DeleteGroupAsync(long groupId)`
   - `Task<ProductTypeCustomizationOptionModel> InsertOptionAsync(long groupId, CustomizationOptionInsertInfo)`
   - `Task<ProductTypeCustomizationOptionModel> UpdateOptionAsync(CustomizationOptionUpdateInfo)`
   - `Task DeleteOptionAsync(long optionId)`
- [X] T117 [US5] Implement these em `Lofn.Domain/Services/ProductTypeService.cs`. Regras adicionais:
   - `InsertOptionAsync` em grupo single: valida que se `IsDefault = true`, nenhuma outra option daquele grupo já é default; senão → ValidationException "single-select group already has a default option".
   - `UpdateOptionAsync` com `IsDefault = true` em grupo single: similar (ignora a própria option na checagem).
   - `UpdateGroupAsync` mudando `SelectionMode multi → single`: valida que o grupo tem ≤1 option com `IsDefault = true`; senão → ValidationException.
   - Cascade de delete cobrado pelo banco.

### REST controller extension (US5)

- [X] T118 [US5] Add to `Lofn.API/Controllers/ProductTypeController.cs` endpoints customizing (todos `[TenantAdmin]`):
   - `POST {productTypeId}/customization/group/insert`
   - `POST customization/group/update`
   - `DELETE customization/group/delete/{groupId}`
   - `POST customization/group/{groupId}/option/insert`
   - `POST customization/option/update`
   - `DELETE customization/option/delete/{optionId}`

### Build green (US5)

- [X] T119 [US5] Run `dotnet build Lofn.sln` — 0 errors. ✅

### Unit tests (US5)

- [ ] T120 [P] [US5] Create `Lofn.Tests/Domain/Validators/CustomizationGroupInsertInfoValidatorTests.cs` cobrindo selectionMode inválido, label vazio.
- [ ] T121 [P] [US5] Create `Lofn.Tests/Domain/Validators/CustomizationOptionInsertInfoValidatorTests.cs`.
- [ ] T122 [P] [US5] Create `Lofn.Tests/Domain/Services/ProductTypeServiceCustomizationTest.cs` com cases: insertGroup OK; insertOption single com 2 defaults → 2nd recusada; updateGroup single→multi OK; updateGroup multi→single com 2 defaults → recusa; deleteGroup cascateia option (verificar via mock que delete chamado).

### Integration tests (US5)

- [X] T123 [P] [US5] Create `Lofn.ApiTests/Controllers/ProductTypeCustomizationTests.cs`. Cases:
   - admin insert group + 3 options + GET tipo → árvore completa retorna. ✅
   - sem auth → 401 (insertGroup, insertOption). ✅
   - 2 defaults em single → 400/422. ✅
   - update multi→single com 2 defaults → 400/422. ✅
   - update single→multi → 200 e refletido no GET. ✅
   - update option label/preço refletido no GET. ✅
   - delete group cascateia options (não aparecem mais no GET tipo). ✅
   - delete option some do GET tipo. ✅

**Checkpoint**: Customizações configuráveis por admin. Pronto para cálculo de preço (US6).

---

## Phase 8: User Story 6 — Comprador visualiza customizações e preço dinâmico (Priority: P2)

**Goal**: `POST /product/{id}/price` (anônimo) calcula `basePrice + Σ(price_delta das options escolhidas)`. Valida options pertencem ao tipo aplicável da categoria do produto, single ≤1, required ≥1, total ≥ 0. Catálogo-only — não toca pedido (Out of Scope).

**Independent Test**: Produto sob categoria com tipo customizável (i3 base, i5 +500, i7 +900). POST price com optionId(i7) → total = base + 900. POST com 2 optionIds em grupo single → 422.

### Domain calculator

- [X] T124 [US6] Create `Lofn.Domain/Services/ProductPriceCalculator.cs` com:
   - `Task<ProductPriceCalculationResult> CalculateAsync(long productId, IList<long> optionIds)`.
   - Carrega produto via `IProductService.GetByIdAsync(productId)` (sem auth) → categoria → applied type via `ICategoryService.GetAppliedProductTypeAsync`.
   - Se sem tipo aplicável: se optionIds não-vazio → ValidationException "Product has no customizations"; se vazio → retorna basePrice + breakdown vazio.
   - Para cada optionId: localiza no schema (group → options). Se não pertence → ValidationException "Option {id} does not belong to product type".
   - Verifica grupos single: agrupa optionIds por groupId; em grupo single, count > 1 → ValidationException.
   - Verifica grupos required: para cada group required, conta presença em optionIds; ausência → ValidationException.
   - Soma `PriceDeltaCents`. Total = basePrice + sum. Se < 0 → ValidationException "Total price cannot be negative".
   - Compõe `breakdown[]` com label do group + label da option + delta.
- [X] T125 [US6] Populate `Lofn/DTO/ProductType/ProductPriceCalculationRequest.cs`: `OptionIds (IList<long>)`. (T038 created shell.)
- [X] T126 [US6] Populate `Lofn/DTO/ProductType/ProductPriceCalculationResult.cs`: `ProductId`, `BasePriceCents`, `Breakdown (IList<PriceBreakdownItem>)`, `DeltaTotalCents`, `TotalCents`. Cria também `Lofn/DTO/ProductType/PriceBreakdownItem.cs` com `OptionId`, `GroupLabel`, `OptionLabel`, `PriceDeltaCents`.

### REST endpoint

- [X] T127 [US6] Add to `Lofn.API/Controllers/ProductController.cs`: `[HttpPost("{productId:long}/price")] [AllowAnonymous]` → `_priceCalculator.CalculateAsync(productId, request.OptionIds)`. Retorna 200 + Result. Trata ValidationException → 422; ProductNotFoundException → 404.

### GraphQL public

- [ ] T128 [US6] Add to `Lofn.GraphQL/Public/PublicQuery.cs` query `productPrice(productId: Long!, optionIds: [Long!]!) -> ProductPriceResult`. Reusa o calculator.

### DI registration

- [X] T129 [US6] Register `ProductPriceCalculator` em `Lofn.Application/Startup.cs` `#region Service`: `services.AddScoped<ProductPriceCalculator>()` (concrete, sem interface).

### Build green

- [X] T130 [US6] Run `dotnet build Lofn.sln` — 0 errors. ✅

### Unit tests (US6)

- [X] T131 [P] [US6] Create `Lofn.Tests/Domain/Services/ProductPriceCalculatorTest.cs`. Cases: ✅ 7 cases — 156/156 unit tests aprovados.
   - basePrice 30000 + optionIds [i7Id] (delta 90000) → total 120000.
   - basePrice + 2 ids em grupo single → throws.
   - basePrice + grupo required ausente → throws.
   - basePrice + sem optionIds em produto sem tipo aplicável → returns basePrice.
   - basePrice + optionIds não-vazios em produto sem tipo aplicável → throws.
   - Total negativo (delta muito negativo) → throws.

### Integration tests (US6)

- [ ] T132 [P] [US6] Create `Lofn.ApiTests/Controllers/ProductPriceCalculationTests.cs`. Cases:
   - POST com optionIds=[i7] → 200 com totalCents correto e breakdown[].
   - POST com optionIds=[] em produto com customização e grupo required → 422.
   - POST com optionId que não pertence ao tipo → 422.
   - Anonymous (sem token) → 200.
- [ ] T133 [P] [US6] Create `Lofn.ApiTests/Controllers/ProductPriceGraphQLTests.cs` para a query `productPrice`. Sanity: mesmo total e breakdown que REST.

**Checkpoint**: Cálculo de preço dinâmico funcional. Storefront pode exibir preço total ajustado. Carrinho/pedido continuam usando basePrice (Out of Scope).

---

## Phase 9: Polish & Cross-Cutting

**Purpose**: Validação end-to-end, smoke tests, ajustes finais.

- [X] T134 [P] Run full unit suite: `dotnet test Lofn.Tests --no-build -nologo --logger "console;verbosity=minimal"` — 100% pass. ✅ **156/156 unit tests** (105 pré-existentes + 51 novos para feature 003).
- [ ] T135 [P] Run full ApiTests suite: `dotnet test Lofn.ApiTests --no-build -nologo --logger "console;verbosity=minimal"` contra a API live (Docker compose up). 100% pass, incluindo as 66 ApiTests pré-existentes da feature 002 sem alteração.
- [ ] T136 Execute `quickstart.md` §1–§9 manualmente contra a API live; confirmar todos os asserts esperados (basePrice + delta, ignored filterIds, applied product type origin, etc.). Capturar evidência em comentário no PR.
- [X] T137 [P] Update `CLAUDE.md` raiz: adicionar na seção "API Endpoints" o bloco "REST — ProductType (`/producttype`)" listando os 13 endpoints do controller; atualizar a seção "GraphQL" com `productsByCategoryFiltered`, `productPrice`, e os tipos novos (`ProductType`, `CustomizationGroup`, etc.). ✅
- [X] T138 Run `dotnet build Lofn.sln -c Release` — 0 errors. ✅ 0 erros / 150 warnings (todos pré-existentes ou nullable annotations em test mocks).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: sem dependências — pode começar imediatamente.
- **Phase 2 (Foundational)**: depende de Phase 1. **BLOQUEIA todas as user stories**.
- **Phase 3 (US1)**: depende de Phase 2 completa.
- **Phase 4 (US2)**: depende de Phase 2 e Phase 3 (precisa de tipos para vincular). Pode rodar em paralelo com Phase 5 e 6 mas só após US1.
- **Phase 5 (US3)**: depende de Phase 2 + Phase 3 (tipo+filtros) + Phase 4 (link categoria↔tipo).
- **Phase 6 (US4)**: depende de Phase 2 + Phase 3 + Phase 4 + Phase 5 (precisa de produtos com filterValues persistidos).
- **Phase 7 (US5)**: depende de Phase 2 + Phase 3 (extensão do controller existente).
- **Phase 8 (US6)**: depende de Phase 2 + Phase 3 + Phase 4 + Phase 7.
- **Phase 9 (Polish)**: depende de todas as user stories desejadas.

### User Story Dependencies (interno P1 cluster)

- **US1**: livre após Phase 2.
- **US2**: precisa de US1 (tipo precisa existir antes do link).
- **US3**: precisa de US1 + US2 (categoria precisa estar linkada para validação de filtros funcionar).
- **US4**: precisa de US1 + US2 + US3 (precisa de valores persistidos para filtrar).
- **US5**: precisa de US1 (extensão direta).
- **US6**: precisa de US1 + US2 + US5 (cálculo carrega tipo aplicável + opções).

### Within Each User Story

- DTOs antes de validators (validators usam os DTOs).
- Validators antes de service (service usa validators via FluentValidation).
- Service antes de controller (controller chama service).
- Controller antes dos integration tests (ApiTests batem nos endpoints).
- Unit tests podem ser escritos em paralelo com a implementação (TDD opcional).
- Integration tests vão por último (precisam de tudo built + fixtures atualizadas).

### Parallel Opportunities

- **Phase 2**: T004–T009 (entities) [P], T013–T017 (models) [P], T020–T024 (mappers) [P], T026–T042 (DTOs) [P], T043–T044 (repo interfaces) [P], T047–T048 (repo impls) [P].
- **Phase 3**: T054–T059 (DTOs populate) [P], T061–T064 (validators) [P], T070–T073 (validator tests) [P], T074 + T075 (service tests + ApiTests) [P].
- **Phase 5**: T093–T095 (testes em arquivos diferentes) [P].
- **Phase 6**: T103 + T104 [P].
- **Phase 7**: T112–T115 (validators) [P], T120–T123 (tests) [P].
- **Phase 8**: T131–T133 (tests) [P].
- **Phase 9**: T134, T135, T137 [P].

### MVP cuts possíveis

- **MVP-Catálogo (P1 only)**: T001–T104 (Phases 1–6). Entrega catálogo filtrado completo. ~104 tarefas. US5+US6 ficam para release seguinte.
- **MVP-Mínimo (US1 isolada)**: T001–T076 (Phases 1–3). Apenas Tipos e Filtros. ~76 tarefas. Não tem efeito visível para comprador, mas valida o fluxo admin.
- **Full (P1 + P2)**: T001–T138. ~138 tarefas. Recomendado se há tempo para release única.

---

## Parallel Example: Phase 2 entities

```bash
# Os 6 arquivos de entidade são totalmente independentes — podem ser
# escritos simultaneamente:
T004 Create Lofn.Infra/Context/ProductType.cs
T005 Create Lofn.Infra/Context/ProductTypeFilter.cs
T006 Create Lofn.Infra/Context/ProductTypeFilterAllowedValue.cs
T007 Create Lofn.Infra/Context/ProductTypeCustomizationGroup.cs
T008 Create Lofn.Infra/Context/ProductTypeCustomizationOption.cs
T009 Create Lofn.Infra/Context/ProductFilterValue.cs

# Em seguida (sequencial, depende dos 6 acima):
T012 Update Lofn.Infra/Context/LofnContext.cs com DbSets + OnModelCreating

# Em paralelo com T012, podem rodar:
T013-T017 Domain models (5 arquivos novos, sem dependência cruzada)
```

## Parallel Example: User Story 1 validators

```bash
T061 Create ProductTypeInsertInfoValidator.cs
T062 Create ProductTypeUpdateInfoValidator.cs
T063 Create ProductTypeFilterInsertInfoValidator.cs
T064 Create ProductTypeFilterUpdateInfoValidator.cs

# E depois em paralelo, os respectivos tests:
T070 ProductTypeInsertInfoValidatorTests.cs
T071 ProductTypeUpdateInfoValidatorTests.cs
T072 ProductTypeFilterInsertInfoValidatorTests.cs
T073 ProductTypeFilterUpdateInfoValidatorTests.cs
```

---

## Implementation Strategy

### MVP-Catálogo (recomendado)

1. Phase 1 (T001) — baseline.
2. Phase 2 (T002–T053) — schema + foundation.
3. Phase 3 (T054–T076) — Tipos + Filtros admin (US1).
4. Phase 4 (T077–T088) — Vincular Tipo a Categoria (US2).
5. Phase 5 (T089–T095) — Vendedor cadastra com filterValues (US3).
6. Phase 6 (T096–T104) — Listagem pública filtrada (US4).
7. **STOP & VALIDATE**: roda quickstart §1–§5 + ApiTests parciais. Demo / deploy MVP catálogo.
8. Phase 7 (T105–T123) — Customizações admin (US5).
9. Phase 8 (T124–T133) — Cálculo de preço (US6).
10. Phase 9 (T134–T138) — Polish + smoke completo.

### Incremental Delivery

- Após Phase 6 já temos catálogo filtrado em produção (P1 inteira).
- Phase 7+8 são aditivas; storefront ganha customização sem afetar pedido (Out of Scope explícito).
- Phase 9 é a "release gate" — corre o full quickstart e bate todos os ApiTests.

### Parallel Team Strategy

Com 2-3 devs:
- Foundational é um esforço sequencial (Phase 2). Pode ser dividido por sub-blocos: dev A faz T002-T012 (schema + entities + LofnContext), dev B faz T013-T030 (models + DTOs + mappers), dev C faz T043-T053 (repos + DI + filter + build).
- Após Phase 2: dev A toca US1+US5 (admin surface), dev B toca US2+US3 (categoria + produto vendedor), dev C toca US4+US6 (storefront público).
- Phase 9 polish é trabalho de revisor de release.

---

## Notes

- [P] = arquivos diferentes, sem dependência incompleta.
- [Story] label rastreia a US de cada task — qualquer task em `Lofn.Tests` ou `Lofn.ApiTests` carrega o label da US que ela valida.
- Cada US deve ser independentemente testável após sua phase fechar — o `Independent Test` na descrição da phase é o critério de aceitação.
- Verifique tests fail antes de implementar (quando aplicável).
- Commit após cada task ou grupo lógico (ex.: 4 entities juntas).
- Pare em qualquer checkpoint para validar story isoladamente.
- Evite: tasks vagas, conflito de mesmo arquivo entre [P], dependências cross-story que quebrem independência.
