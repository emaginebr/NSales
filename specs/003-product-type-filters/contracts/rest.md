# REST Contracts

**Feature**: 003-product-type-filters
**Spec**: [../spec.md](../spec.md) | **Plan**: [../plan.md](../plan.md) | **Data Model**: [../data-model.md](../data-model.md)

Todos os endpoints abaixo são novos exceto onde marcado `EXTEND`. Convenção: REST sob path-style consistente com os controllers existentes (`/category`, `/product`, `/store`). Auth headers: `Authorization: Bearer <token>` quando exigido. Tenant header: o `TenantHeaderHandler` injeta automaticamente baseado no host.

---

## Tipo de Produto (admin only)

Todos os endpoints abaixo exigem `[TenantAdmin]` (`IsAdmin = true` no token), independente do modo `Marketplace`.

### `POST /producttype/insert`

Cria um novo Tipo de Produto.

**Request body** (`ProductTypeInsertInfo`):
```json
{
  "name": "Calçado",
  "description": "Sapatos, tênis, botas e similares"
}
```

**Response 200** (`ProductTypeInfo`):
```json
{
  "productTypeId": 12,
  "name": "Calçado",
  "description": "Sapatos, tênis, botas e similares",
  "filters": [],
  "customizationGroups": [],
  "createdAt": "2026-04-29T10:00:00Z",
  "updatedAt": "2026-04-29T10:00:00Z"
}
```

**Errors**:
- `403` se usuário não é admin.
- `422` se `name` vazio ou já existe no tenant.

---

### `POST /producttype/update`

Atualiza nome/descrição de um tipo. Não toca filtros nem grupos (esses têm endpoints próprios).

**Request body** (`ProductTypeUpdateInfo`):
```json
{
  "productTypeId": 12,
  "name": "Calçado e Acessórios",
  "description": null
}
```

**Response 200**: `ProductTypeInfo` (sem filters/groups expandidos por padrão; ver `GET` para shape completo).

**Errors**:
- `403` admin gate.
- `404` se `productTypeId` não existe.
- `422` se `name` colide com outro tipo do tenant.

---

### `DELETE /producttype/delete/{productTypeId}`

Hard delete. Cascade nas tabelas filhas (filtros, allowed_values, groups, options) e SET NULL em categorias vinculadas.

**Response 204** sem body.

**Errors**:
- `403` admin gate.
- `404` tipo não existe.

---

### `GET /producttype/list`

Lista todos os tipos do tenant. Sem paginação (volume baixo: ≤20 tipos por tenant).

**Response 200**: `IList<ProductTypeInfo>` — cada item carrega `filters[]` e `customizationGroups[]` expandidos.

**Errors**: `403` admin gate.

---

### `GET /producttype/{productTypeId}`

Recupera um tipo com schema completo.

**Response 200**: `ProductTypeInfo` com filters e customizationGroups completamente populados (cada filter com `allowedValues[]` quando enum; cada group com `options[]`).

**Errors**: `403`, `404`.

---

## Filtros do Tipo

### `POST /producttype/{productTypeId}/filter/insert`

Adiciona um filtro ao schema do tipo.

**Request body** (`ProductTypeFilterInsertInfo`):
```json
{
  "label": "Cor",
  "dataType": "enum",
  "isRequired": true,
  "displayOrder": 3,
  "allowedValues": ["Preto", "Branco", "Marrom"]
}
```

`allowedValues` é obrigatório quando `dataType == "enum"`, ignorado caso contrário.

**Response 200**: `ProductTypeFilterInfo` populado.

**Errors**:
- `403` admin gate.
- `404` tipo não existe.
- `422`: `label` duplicado no tipo, `dataType` inválido, `enum` sem `allowedValues`, `allowedValues` com duplicatas.

---

### `POST /producttype/filter/update`

Atualiza um filtro. NÃO permite mudar `dataType` (rejeita com 422). Permite mudar `label`, `isRequired`, `displayOrder`. Para enums, permite editar `allowedValues` (substituição completa — service reconcilia).

**Request body** (`ProductTypeFilterUpdateInfo`):
```json
{
  "filterId": 88,
  "label": "Coloração",
  "isRequired": true,
  "displayOrder": 3,
  "allowedValues": ["Preto", "Branco", "Marrom", "Cinza"]
}
```

**Response 200**: `ProductTypeFilterInfo`.

**Errors**:
- `403`.
- `404` filtro não existe.
- `422` mudança de `dataType` solicitada, ou label colide.

---

### `DELETE /producttype/filter/delete/{filterId}`

Hard delete. Cascade em `allowed_values` e `product_filter_values`.

**Response 204**.

**Errors**: `403`, `404`.

---

## Customização do Tipo

### `POST /producttype/{productTypeId}/customization/group/insert`

Cria um grupo de customização.

**Request body** (`CustomizationGroupInsertInfo`):
```json
{
  "label": "Processador",
  "selectionMode": "single",
  "isRequired": true,
  "displayOrder": 1
}
```

**Response 200**: `CustomizationGroupInfo` (com `options: []` vazio).

**Errors**: `403`, `404`, `422` (label duplicado, `selectionMode` inválido).

---

### `POST /producttype/customization/group/update`

Atualiza grupo. Permite mudar `label`, `isRequired`, `selectionMode`, `displayOrder`. Mudar `selectionMode` de `single → multi` é OK; de `multi → single` valida que ≤1 option tem `isDefault = true`.

**Request body** (`CustomizationGroupUpdateInfo`):
```json
{
  "groupId": 33,
  "label": "Processador",
  "selectionMode": "single",
  "isRequired": true,
  "displayOrder": 1
}
```

**Response 200**: `CustomizationGroupInfo`.

**Errors**: `403`, `404`, `422` (mudança incompatível de `selectionMode`).

---

### `DELETE /producttype/customization/group/delete/{groupId}`

Hard delete cascateado nas options.

**Response 204**.

---

### `POST /producttype/customization/group/{groupId}/option/insert`

Adiciona uma opção a um grupo.

**Request body** (`CustomizationOptionInsertInfo`):
```json
{
  "label": "i7",
  "priceDeltaCents": 90000,
  "isDefault": false,
  "displayOrder": 3
}
```

**Response 200**: `CustomizationOptionInfo`.

**Errors**:
- `403`, `404`.
- `422`: `label` duplicado no grupo; `isDefault: true` em grupo single quando outra option já é default.

---

### `POST /producttype/customization/option/update`

**Request body** (`CustomizationOptionUpdateInfo`):
```json
{
  "optionId": 901,
  "label": "i7-12700",
  "priceDeltaCents": 90000,
  "isDefault": false,
  "displayOrder": 3
}
```

**Response 200**: `CustomizationOptionInfo`.

---

### `DELETE /producttype/customization/option/delete/{optionId}`

Hard delete. Sem cascade na release atual (pedido não persiste opção).

**Response 204**.

---

## Vínculo Categoria ↔ Tipo

Endpoints sob o controller existente `/category` e `/category-global`. Permissão:
- `/category/{slug}/producttype/...` — exige `[TenantAdmin]` (independente de `Marketplace`).
- `/category-global/{categoryId}/producttype/...` — exige `[TenantAdmin][MarketplaceAdmin]` (manter mutex existente).

> Decisão: para simplificar e seguir o spec ("Tipos vivem no tenant"), unificamos ambos os caminhos sob `[TenantAdmin]` apenas. O segundo `[MarketplaceAdmin]` no caminho global é dispensado porque o efeito é o mesmo (link em categoria global) e o gate `[TenantAdmin]` já cobre a permissão.

### `PUT /category/{categoryId}/producttype/{productTypeId}`

Vincula a categoria ao tipo. Substitui qualquer vínculo direto anterior (idempotente em si — chamar 2× com mesmo `productTypeId` mantém o estado).

**Response 200**: `CategoryInfo` atualizado, com `productTypeId` setado e `appliedProductTypeId` resolvido.

**Errors**:
- `403`.
- `404` categoria ou tipo não existe.
- `422` se categoria está em scope incompatível com regras existentes (não esperado, mas tratado).

---

### `DELETE /category/{categoryId}/producttype`

Remove o vínculo direto. Resolução closest-ancestor passa a valer normalmente.

**Response 200**: `CategoryInfo` com `productTypeId = null`.

**Errors**: `403`, `404`.

---

### `GET /category/{categoryId}/producttype/applied`

Conveniência: retorna o tipo aplicável (direto ou herdado) para a categoria.

**Response 200**:
```json
{
  "appliedProductTypeId": 12,
  "originCategoryId": 50,
  "productType": { ... ProductTypeInfo completo ... }
}
```
Ou `null` se a categoria nem ancestrais têm vínculo. **Anônimo** (público) — vendedores e compradores precisam para popular UI.

---

## Cadastro de Produto com filtros (EXTEND)

### `POST /product/{storeSlug}/insert` (EXTEND)

Já existe. Body atual ganha campo opcional `filterValues`:

```json
{
  "name": "Tênis Nike Air Max",
  "price": 49990,
  "categoryId": 50,
  "filterValues": [
    { "filterId": 88, "value": "Branco" },
    { "filterId": 91, "value": "42" },
    { "filterId": 92, "value": "Masculino" }
  ]
}
```

**Validation extension**:
- Se a categoria do produto tem tipo aplicável: para cada `filter.is_required = true`, exige uma entrada em `filterValues`. Lista todos os faltando em caso de erro.
- Cada `(filterId, value)` é validado contra `filter.data_type` e `allowed_values` (se enum).
- `filterId` que não pertence ao tipo aplicável → ignorado silenciosamente (loga warning).
- Se a categoria não tem tipo aplicável: `filterValues` ignorado.

**Response 200**: `ProductInfo` com `filterValues[]` populado e `appliedProductTypeId` (snapshot resolvido).

**Errors**:
- `400`/`422`: filtro obrigatório faltando, valor inválido, valor enum fora do allowed.

---

### `POST /product/{storeSlug}/update` (EXTEND)

Mesmo shape de `insert`. Reconcilia: opções enviadas substituem o conjunto atual (insert/update/delete conforme).

---

## Listagem pública filtrada

### `POST /product/search-filtered`

Anônimo (público). Substitui o uso atual de `POST /product/search` quando o cliente quer filtrar por categoria + valores de filtro. Mantemos `/product/search` como está (busca por keyword genérica).

**Request body** (`ProductSearchFilteredParam`):
```json
{
  "storeSlug": "minha-loja",
  "categorySlug": "calcados/tenis",
  "filters": [
    { "filterId": 88, "value": "Branco" },
    { "filterId": 91, "value": "42" }
  ],
  "pageNum": 1
}
```

`storeSlug` é opcional em modo marketplace (categoria global). `categorySlug` é obrigatório.

**Response 200**:
```json
{
  "products": [ ... ProductInfo[] ... ],
  "pageNum": 1,
  "pageCount": 3,
  "totalItems": 42,
  "appliedProductTypeId": 12,
  "appliedFilters": [
    { "filterId": 88, "label": "Cor", "value": "Branco" },
    { "filterId": 91, "label": "Número", "value": "42" }
  ],
  "ignoredFilterIds": [555]
}
```

**Errors**: `404` se categoria/loja não existe; `422` se body malformado.

---

## Cálculo de preço dinâmico

### `POST /product/{productId}/price`

Anônimo (público). Calcula preço total = preço-base + Σ(price-delta das options escolhidas).

**Request body** (`ProductPriceCalculationRequest`):
```json
{
  "optionIds": [901, 905]
}
```

`optionIds` pode ser vazio (retorna preço-base). Cada `optionId` precisa pertencer ao tipo aplicável da categoria do produto.

**Response 200** (`ProductPriceCalculationResult`):
```json
{
  "productId": 1234,
  "basePriceCents": 300000,
  "breakdown": [
    { "optionId": 901, "groupLabel": "Processador", "optionLabel": "i7", "priceDeltaCents": 90000 },
    { "optionId": 905, "groupLabel": "Memória", "optionLabel": "32GB", "priceDeltaCents": 60000 }
  ],
  "deltaTotalCents": 150000,
  "totalCents": 450000
}
```

**Validation**:
- Cada `optionId` deve pertencer ao tipo aplicável da categoria.
- Em grupos `single`, no máximo 1 `optionId` daquele grupo.
- Em grupos `multi`, qualquer combinação.
- Em grupos `is_required = true`, exige ≥1 `optionId`.
- Total final ≥ 0; senão retorna 422.

**Errors**:
- `404` produto não existe.
- `422`: option não pertence, violação de single, required faltando, total negativo.

---

## Headers / convenções
- `Content-Type: application/json` em todos POST/PUT/DELETE com body.
- `Accept: application/json` (default).
- Erros seguem o formato existente do projeto: `{ "error": "...", "details": [...] }` para 422.
- Status: 200 (sucesso com body), 204 (sucesso sem body para DELETE), 401 (não autenticado), 403 (autenticado sem permissão), 404 (não encontrado), 422 (validação), 500 (server).
