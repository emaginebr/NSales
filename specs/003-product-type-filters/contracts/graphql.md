# GraphQL Contracts

**Feature**: 003-product-type-filters
**Spec**: [../spec.md](../spec.md) | **Plan**: [../plan.md](../plan.md)

Adições aos schemas Public e Admin do HotChocolate. Endpoints existentes (`POST /graphql` e `POST /graphql/admin`) ganham os campos descritos abaixo.

---

## Tipos novos (compartilhados entre Public e Admin)

```graphql
type ProductType {
  productTypeId: Long!
  name: String!
  description: String
  filters: [ProductTypeFilter!]!
  customizationGroups: [CustomizationGroup!]!
  createdAt: DateTime!
  updatedAt: DateTime!
}

type ProductTypeFilter {
  filterId: Long!
  label: String!
  dataType: FilterDataType!
  isRequired: Boolean!
  displayOrder: Int!
  allowedValues: [String!]!  # vazio quando dataType != enum
}

enum FilterDataType {
  TEXT
  INTEGER
  DECIMAL
  BOOLEAN
  ENUM
}

type CustomizationGroup {
  groupId: Long!
  label: String!
  selectionMode: SelectionMode!
  isRequired: Boolean!
  displayOrder: Int!
  options: [CustomizationOption!]!
}

enum SelectionMode {
  SINGLE
  MULTI
}

type CustomizationOption {
  optionId: Long!
  label: String!
  priceDeltaCents: Long!
  isDefault: Boolean!
  displayOrder: Int!
}

type ProductFilterValue {
  filterId: Long!
  filterLabel: String!
  dataType: FilterDataType!
  value: String!
}
```

---

## Public schema (`POST /graphql`) — anônimo

### Extensão de `Category`

```graphql
extend type Category {
  """
  Tipo aplicado diretamente a esta categoria. NULL se a categoria não tem
  vínculo direto (mesmo que herde via ancestral).
  """
  productTypeId: Long

  """
  Tipo efetivamente aplicável a esta categoria após resolução closest-ancestor.
  NULL se nem a categoria nem qualquer ancestral tem vínculo.
  """
  appliedProductType: ProductType

  """
  Id da categoria que de fato definiu o tipo aplicável (auto-referência se a
  própria categoria está vinculada; ancestral mais próximo caso contrário).
  NULL se nenhum tipo é aplicável.
  """
  appliedProductTypeOriginCategoryId: Long
}
```

### Extensão de `Product`

```graphql
extend type Product {
  """
  Tipo aplicável ao produto, herdado via categoria do produto + closest-ancestor.
  Mesmo cálculo do Category.appliedProductType.
  """
  appliedProductType: ProductType

  """
  Valores de filtro preenchidos para este produto. Sempre um subconjunto dos
  filtros do appliedProductType (filterIds extras foram descartados na escrita).
  """
  filterValues: [ProductFilterValue!]!
}
```

### Nova query

```graphql
extend type Query {
  """
  Listagem pública paginada por categoria + filtros. Equivalente ao endpoint
  REST `POST /product/search-filtered` mas via GraphQL para clientes que
  preferem o protocolo unificado.

  - storeSlug: opcional (omitir quando categoria global no marketplace)
  - categorySlug: obrigatório
  - filters: lista de pares { filterId, value } com semântica AND
  - pageNum: 1-based; pageSize fixo (mesmo padrão da feature 002)
  """
  productsByCategoryFiltered(
    storeSlug: String
    categorySlug: String!
    filters: [FilterValueInput!]
    pageNum: Int = 1
  ): ProductSearchFilteredPayload!
}

input FilterValueInput {
  filterId: Long!
  value: String!
}

type ProductSearchFilteredPayload {
  products: [Product!]!
  pageNum: Int!
  pageCount: Int!
  totalItems: Int!
  appliedProductTypeId: Long
  appliedFilters: [AppliedFilter!]!
  ignoredFilterIds: [Long!]!
}

type AppliedFilter {
  filterId: Long!
  label: String!
  value: String!
}
```

### Nova query (cálculo de preço)

```graphql
extend type Query {
  """
  Calcula preço dinâmico de um produto com customizações. Anônimo. Não
  persiste nada (Out of Scope: pedido).
  """
  productPrice(
    productId: Long!
    optionIds: [Long!]!
  ): ProductPriceResult!
}

type ProductPriceResult {
  productId: Long!
  basePriceCents: Long!
  breakdown: [PriceBreakdownItem!]!
  deltaTotalCents: Long!
  totalCents: Long!
}

type PriceBreakdownItem {
  optionId: Long!
  groupLabel: String!
  optionLabel: String!
  priceDeltaCents: Long!
}
```

---

## Admin schema (`POST /graphql/admin`) — exige Bearer token

### Novas queries

```graphql
extend type Query {
  """
  Lista todos os tipos do tenant. Exige IsAdmin = true.
  """
  myProductTypes: [ProductType!]!

  """
  Recupera um tipo específico com schema completo. Exige IsAdmin = true.
  """
  myProductType(productTypeId: Long!): ProductType
}
```

> Mutations não são adicionadas ao GraphQL Admin nesta release. Operações de
> escrita (CRUD de tipo, filtros, customizações, links) ficam exclusivamente
> sob REST (ver `rest.md`). Justificativa: o projeto já escolheu REST como
> superfície primária de escrita; o GraphQL é leitura-otimizada
> (`[UseProjection]`, `[UseFiltering]`).

---

## Comportamento de erros

GraphQL retorna HTTP 200 mesmo em erro lógico, com `errors[]` no body. Nossos códigos de erro sob `extensions.code`:

| Código | Significado |
|--------|-------------|
| `AUTH_NOT_AUTHENTICATED` | Token ausente em endpoint admin (já existente do projeto) |
| `AUTH_FORBIDDEN_NOT_ADMIN` | Token presente mas IsAdmin = false |
| `NOT_FOUND` | Produto/categoria/tipo/filtro não existe |
| `VALIDATION_FAILED` | Combinação de options inválida (single com >1, required ausente, total negativo) |

---

## Performance & projection

- Queries que retornam `ProductType` aproveitam `[UseProjection]` para evitar over-fetch (clients que pedem só `name` não materializam `filters`/`customizationGroups`).
- `productsByCategoryFiltered` usa o mesmo padrão de paginação da feature 002 (`pageNum` 1-based, page size fixo configurável globalmente).
- `productPrice` é cálculo puro em memória, sem `[UseProjection]` (resolver compõe o resultado).
