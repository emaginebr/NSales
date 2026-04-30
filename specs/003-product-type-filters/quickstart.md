# Quickstart — Feature 003 Product Types & Filters

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

Roteiro end-to-end para validar a feature em ambiente local após o `/speckit.implement` ter rodado. Os passos cobrem o caminho feliz das US1–US6.

---

## Pré-requisitos

1. PostgreSQL up (Docker compose já levanta o stack: `docker-compose up`).
2. Migration `20260430_AddProductTypes.sql` aplicada no tenant ativo.
3. API em `https://localhost:44374` ou `http://localhost:8081/api` (via nginx).
4. Token `Bearer` de um usuário com `IsAdmin = true` no tenant — exporte como `$ADMIN`.
5. Token `Bearer` de um vendedor membro de uma loja `minha-loja` — exporte como `$SELLER`.
6. Dois ambientes a testar:
   - Marketplace (`Marketplace=true`): admin define tipos globais; categorias globais linkam tipos.
   - Loja-única (`Marketplace=false`): admin do tenant define tipos; categorias da loja linkam tipos.

---

## §1. Admin cria um Tipo (US1)

```bash
curl -X POST https://localhost:44374/producttype/insert \
  -H "Authorization: Bearer $ADMIN" \
  -H "Content-Type: application/json" \
  -d '{ "name": "Calçado", "description": "Sapatos, tênis e botas" }'
```

Esperado: `200 OK` com `productTypeId` (anote — chamemos de `$TYPE_ID`).

Verificação:
```bash
curl -X GET https://localhost:44374/producttype/$TYPE_ID \
  -H "Authorization: Bearer $ADMIN"
```
Resposta deve mostrar `filters: []` e `customizationGroups: []`.

---

## §2. Admin adiciona filtros (US1)

```bash
# Filtro enum: Cor
curl -X POST https://localhost:44374/producttype/$TYPE_ID/filter/insert \
  -H "Authorization: Bearer $ADMIN" \
  -H "Content-Type: application/json" \
  -d '{
    "label": "Cor",
    "dataType": "enum",
    "isRequired": true,
    "displayOrder": 1,
    "allowedValues": ["Preto", "Branco", "Marrom"]
  }'
```

```bash
# Filtro número: Tamanho
curl -X POST https://localhost:44374/producttype/$TYPE_ID/filter/insert \
  -H "Authorization: Bearer $ADMIN" \
  -H "Content-Type: application/json" \
  -d '{
    "label": "Tamanho",
    "dataType": "integer",
    "isRequired": true,
    "displayOrder": 2
  }'
```

```bash
# Filtro enum: Gênero
curl -X POST https://localhost:44374/producttype/$TYPE_ID/filter/insert \
  -H "Authorization: Bearer $ADMIN" \
  -H "Content-Type: application/json" \
  -d '{
    "label": "Gênero",
    "dataType": "enum",
    "isRequired": false,
    "displayOrder": 3,
    "allowedValues": ["Masculino", "Feminino", "Unissex"]
  }'
```

Anote os 3 `filterId` retornados — chamemos de `$F_COR`, `$F_TAM`, `$F_GEN`.

**Negative check** (admin gate):
```bash
# Tentar com SELLER (não admin) — deve dar 403
curl -X POST https://localhost:44374/producttype/$TYPE_ID/filter/insert \
  -H "Authorization: Bearer $SELLER" \
  -H "Content-Type: application/json" \
  -d '{ "label": "Marca", "dataType": "text", "isRequired": false, "displayOrder": 4 }'
```
Esperado: `403`.

---

## §3. Admin vincula Tipo à Categoria (US2)

Pré: já existe categoria "Calçados/Tênis" com `categoryId = $CAT_ID` (criada via feature 002 em release anterior).

```bash
curl -X PUT https://localhost:44374/category/$CAT_ID/producttype/$TYPE_ID \
  -H "Authorization: Bearer $ADMIN"
```

Esperado: `200 OK`. Resposta deve incluir `productTypeId == $TYPE_ID` e `appliedProductTypeOriginCategoryId == $CAT_ID`.

**Closest-ancestor check**: pegue uma categoria filha (`$CAT_FILHA`, ex.: "Calçados/Tênis/Casual") sem tipo direto e verifique:
```bash
curl -X GET https://localhost:44374/category/$CAT_FILHA/producttype/applied
```
Esperado: `appliedProductTypeId == $TYPE_ID`, `originCategoryId == $CAT_ID`, e o tipo completo.

---

## §4. Vendedor cadastra produto com filtros (US3)

```bash
curl -X POST https://localhost:44374/product/minha-loja/insert \
  -H "Authorization: Bearer $SELLER" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Tênis Nike Air",
    "price": 49990,
    "categoryId": '"$CAT_ID"',
    "filterValues": [
      { "filterId": '"$F_COR"', "value": "Branco" },
      { "filterId": '"$F_TAM"', "value": "42" },
      { "filterId": '"$F_GEN"', "value": "Masculino" }
    ]
  }'
```

Esperado: `200 OK` com `productInfo.filterValues` populado e `appliedProductTypeId == $TYPE_ID`.

**Negative check (filtro obrigatório faltando)**:
```bash
curl -X POST https://localhost:44374/product/minha-loja/insert \
  -H "Authorization: Bearer $SELLER" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Tênis Sem Cor",
    "price": 19990,
    "categoryId": '"$CAT_ID"',
    "filterValues": [{ "filterId": '"$F_TAM"', "value": "40" }]
  }'
```
Esperado: `422` listando "Cor" como obrigatório faltando.

**Negative check (valor enum inválido)**:
```bash
curl -X POST https://localhost:44374/product/minha-loja/insert \
  -H "Authorization: Bearer $SELLER" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Tênis Roxo",
    "price": 19990,
    "categoryId": '"$CAT_ID"',
    "filterValues": [
      { "filterId": '"$F_COR"', "value": "Roxo" },
      { "filterId": '"$F_TAM"', "value": "40" }
    ]
  }'
```
Esperado: `422` com `Roxo` apontado como valor inválido para o filtro `Cor`.

---

## §5. Comprador filtra catálogo (US4)

Crie 2 produtos adicionais (Cor=Preto/Tam=42 e Cor=Branco/Tam=43) para validar o filtro.

```bash
curl -X POST https://localhost:44374/product/search-filtered \
  -H "Content-Type: application/json" \
  -d '{
    "storeSlug": "minha-loja",
    "categorySlug": "calcados/tenis",
    "filters": [
      { "filterId": '"$F_COR"', "value": "Branco" },
      { "filterId": '"$F_TAM"', "value": "42" }
    ],
    "pageNum": 1
  }'
```

Esperado:
- `products[]` contém apenas o "Tênis Nike Air" (interseção AND).
- `appliedProductTypeId == $TYPE_ID`.
- `appliedFilters[]` ecoa as 2 entradas.
- `ignoredFilterIds == []`.

**Rollup pai⇡filho**: filtre na categoria pai "calcados" (sem o segmento /tenis) e confirme que produtos da subcategoria também aparecem.

**Filtro desconhecido (FR-016)**: envie um `filterId: 999999` que não existe no tipo:
```bash
curl -X POST https://localhost:44374/product/search-filtered \
  -H "Content-Type: application/json" \
  -d '{
    "storeSlug": "minha-loja",
    "categorySlug": "calcados/tenis",
    "filters": [
      { "filterId": '"$F_COR"', "value": "Branco" },
      { "filterId": 999999, "value": "x" }
    ],
    "pageNum": 1
  }'
```
Esperado: `200 OK`, `appliedFilters[]` só com o `Cor`, `ignoredFilterIds: [999999]`, listagem aplica apenas o Cor=Branco.

---

## §6. Admin define customizações para "Equipamento" (US5)

Crie um segundo tipo "Equipamento" e categoria "Informática":

```bash
curl -X POST https://localhost:44374/producttype/insert \
  -H "Authorization: Bearer $ADMIN" \
  -H "Content-Type: application/json" \
  -d '{ "name": "Equipamento", "description": "Eletrônicos e informática" }'
# anote $EQ_TYPE
```

```bash
# Grupo "Processador" single-select obrigatório
curl -X POST https://localhost:44374/producttype/$EQ_TYPE/customization/group/insert \
  -H "Authorization: Bearer $ADMIN" \
  -H "Content-Type: application/json" \
  -d '{ "label": "Processador", "selectionMode": "single", "isRequired": true, "displayOrder": 1 }'
# anote $G_PROC
```

```bash
# Opções: i3 base, i5 +500, i7 +900
curl -X POST https://localhost:44374/producttype/customization/group/$G_PROC/option/insert \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{ "label": "i3", "priceDeltaCents": 0, "isDefault": true, "displayOrder": 1 }'
# anote $OPT_I3

curl -X POST https://localhost:44374/producttype/customization/group/$G_PROC/option/insert \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{ "label": "i5", "priceDeltaCents": 50000, "isDefault": false, "displayOrder": 2 }'
# anote $OPT_I5

curl -X POST https://localhost:44374/producttype/customization/group/$G_PROC/option/insert \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{ "label": "i7", "priceDeltaCents": 90000, "isDefault": false, "displayOrder": 3 }'
# anote $OPT_I7
```

Vincule "Equipamento" à categoria "Informática" (idêntico ao §3).

Cadastre um notebook (`$NOTE_ID`):
```bash
curl -X POST https://localhost:44374/product/minha-loja/insert \
  -H "Authorization: Bearer $SELLER" -H "Content-Type: application/json" \
  -d '{ "name": "Notebook X", "price": 300000, "categoryId": '"$CAT_INFO"' }'
```

---

## §7. Comprador calcula preço dinâmico (US6 — catálogo only)

```bash
curl -X POST https://localhost:44374/product/$NOTE_ID/price \
  -H "Content-Type: application/json" \
  -d '{ "optionIds": ['"$OPT_I7"'] }'
```

Esperado:
```json
{
  "productId": ...,
  "basePriceCents": 300000,
  "breakdown": [
    { "optionId": ..., "groupLabel": "Processador", "optionLabel": "i7", "priceDeltaCents": 90000 }
  ],
  "deltaTotalCents": 90000,
  "totalCents": 390000
}
```

Volte para i3 e o total cai para 300000:
```bash
curl -X POST https://localhost:44374/product/$NOTE_ID/price \
  -d '{ "optionIds": ['"$OPT_I3"'] }' -H "Content-Type: application/json"
```

**Negative check (single com >1 opção)**:
```bash
curl -X POST https://localhost:44374/product/$NOTE_ID/price \
  -d '{ "optionIds": ['"$OPT_I3"', '"$OPT_I7"'] }' -H "Content-Type: application/json"
```
Esperado: `422` "grupo Processador é single-select; envie no máximo 1 option".

**Lembre**: este endpoint NÃO afeta o carrinho. Adicionar `$NOTE_ID` ao cart e fechar o pedido cobra `R$3.000` (preço-base) — comportamento esperado nesta release. O frontend deve exibir o aviso de FR-025 nos produtos com customização.

---

## §8. GraphQL: query pública filtrada

```graphql
query {
  productsByCategoryFiltered(
    storeSlug: "minha-loja"
    categorySlug: "calcados/tenis"
    filters: [
      { filterId: 88, value: "Branco" }
      { filterId: 91, value: "42" }
    ]
    pageNum: 1
  ) {
    products { productId name slug price filterValues { filterLabel value } }
    pageNum
    pageCount
    totalItems
    appliedProductTypeId
    appliedFilters { filterId label value }
    ignoredFilterIds
  }
}
```

Em `POST /graphql` (anônimo). Espera o mesmo resultado que §5 mas na shape GraphQL.

---

## §9. Limpeza (opcional)

```bash
# Remove o vínculo da categoria → Tipo
curl -X DELETE https://localhost:44374/category/$CAT_ID/producttype \
  -H "Authorization: Bearer $ADMIN"

# Hard delete dos tipos (cascade nas tabelas filhas)
curl -X DELETE https://localhost:44374/producttype/delete/$TYPE_ID \
  -H "Authorization: Bearer $ADMIN"
curl -X DELETE https://localhost:44374/producttype/delete/$EQ_TYPE \
  -H "Authorization: Bearer $ADMIN"
```

Após delete: `lofn_categories.product_type_id` que apontavam viram NULL automaticamente (FK ON DELETE SET NULL). Produtos com `lofn_product_filter_values` perdem esses valores via cascade.

---

## Critérios de sucesso (mapeamento spec)

| Passo | SC validado |
|-------|-------------|
| §1 e §2 (admin gate negative) | SC-001 (autorização) |
| §5 (latência, ignorar desconhecidos, AND) | SC-002, FR-016 |
| §4 (obrigatórios) | SC-003 |
| §5 (3 filtros consecutivos no tempo de UX) | SC-004 (medição manual) |
| §7 (cálculo correto e negativo) | SC-005 |
| Categorias sem tipo continuam editáveis (não testado aqui — usar produtos pré-existentes) | SC-006 |
