# Phase 0 — Research & Decisions

**Feature**: 003-product-type-filters
**Spec**: [spec.md](./spec.md)
**Plan**: [plan.md](./plan.md)

Resolve as questões abertas do Technical Context do `plan.md` e fixa padrões para a fase de implementação. Cada item segue o formato `Decision / Rationale / Alternatives considered`.

---

## R1. Storage de valores de filtro (polimórfico vs. multi-coluna)

**Decision**: Coluna única `value text` na tabela `lofn_product_filter_values` carregando o valor stringificado, com a interpretação determinada pelo `data_type` declarado em `lofn_product_type_filters`. Conversão acontece no Domain (`ProductFilterValueResolver`) ao ler/escrever, usando `data_type` como discriminador (`text → string`, `integer → long`, `decimal → decimal`, `boolean → "true"/"false"`, `enum → string` validado contra `lofn_product_type_filter_allowed_values`).

**Rationale**:
- O catálogo previsto (≤10 filtros por tipo, ≤10.000 produtos) coberto por um índice composto `(product_id, filter_id)` UNIQUE garante O(1) por linha em writes e leituras pontuais.
- O índice composto `(filter_id, value)` (não-único) cobre o caso de busca por igualdade `WHERE filter_id = X AND value = Y`, que é o fluxo dominante do storefront público (FR-014).
- Cinco tipos de dados não justificam cinco colunas físicas (`value_text`, `value_int`, `value_decimal`, `value_bool`, `value_enum`) — quatro delas ficariam NULL sempre. A normalização pesa mais que a economia.
- Range queries numéricos (`preço entre X e Y`) NÃO estão no escopo v1 (Out of Scope na spec), então comparação textual é suficiente.

**Alternatives considered**:
- *EAV completo com colunas tipadas separadas* — rejeitado: 4× mais NULLs, lógica de leitura mais ramificada, sem ganho enquanto não houver range query.
- *JSONB `attributes` no próprio `lofn_products`* — rejeitado: melhor para sparse e evolutivo mas piora drasticamente o filtro com índice em PostgreSQL (precisa GIN sobre expressão), foge do padrão das outras tabelas do Lofn (todas relacionais), e impede o pattern de FK do filtro.
- *Tabela única com discriminator + 5 colunas* — rejeitado: pior relação custo-benefício do que o EAV completo.

---

## R2. Identidade estável de filtros e opções (renomeação sem perda)

**Decision**: Cada filtro (`lofn_product_type_filters`) e cada opção de customização (`lofn_product_type_customization_options`) carrega uma chave primária `bigint` autoincrement (`filter_id`, `option_id`) que é a única referência usada por `lofn_product_filter_values` e (futuramente) pela linha de pedido. O `label` é só apresentação e pode ser editado livremente — valores históricos continuam apontando para o mesmo `filter_id`/`option_id` e absorvem o novo rótulo.

**Rationale**:
- Renomear "Cor" para "Coloração" não pode invalidar produtos existentes (Edge Case da spec). FK por id atende exatamente isso.
- Mantém o padrão do projeto (todas as entidades já usam `bigint` PK autoincrement gerada pelo PostgreSQL). Não precisamos de UUID nem composite key.

**Alternatives considered**:
- *Composite key `(type_id, label)`* — rejeitado: força cascade rename em N tabelas, quebra ao renomear o label.
- *UUID externo + bigint interno* — rejeitado: complica relacionamentos sem ganho — ninguém vai expor UUID para clientes externos que precisem dele estável (este é trabalho interno).

---

## R3. Resolução closest-ancestor de Tipo aplicável a uma Categoria

**Decision**: Implementar `GetAppliedProductTypeAsync(CategoryId)` no `CategoryRepository` que reusa o método `GetAncestorChainAsync` da feature 002 — ele já devolve a cadeia raiz→nó (ou nó→raiz) em ordem. Iterar a cadeia em C# do nó atual em direção à raiz e retornar o primeiro ancestral com `ProductTypeId IS NOT NULL`. Resultado é (`ProductType`, `OriginCategoryId`) onde `OriginCategoryId` indica de qual categoria veio a herança (útil para o storefront/admin debugar).

**Rationale**:
- A árvore de categoria é limitada a 5 níveis (FR da feature 002), então o caminho é O(5) — leitura única do conjunto via `GetAncestorChainAsync` que já foi otimizada.
- Cachear no nível de request é trivial: o GraphQL resolver pode usar DataLoader keyed por `CategoryId`, mas para volume previsto (≤500 categorias por tenant) o custo do walk repetido é insignificante.
- Não muda a regra de cycle/depth da feature 002 — herda transparentemente.

**Alternatives considered**:
- *Materializar o `applied_product_type_id` em cada categoria via trigger ao alterar pai/tipo* — rejeitado: aumenta complexidade de manutenção, piora consistência transacional em renomeações de árvore, e não traz ganho mensurável dado o O(5).
- *Resolver no service com SQL recursivo (CTE)* — rejeitado: duplica lógica que `GetAncestorChainAsync` já faz; pattern do projeto evita SQL bruto na repository.

---

## R4. Listagem pública filtrada (`POST /product/search-filtered`)

**Decision**: Construir uma query EF Core que parte do conjunto `lofn_products` joined a `lofn_categories` (rollup pai⇡filho da feature 002 via `IsDescendantOf`-equivalent, expandindo a categoria-alvo + descendentes) e aplica AND de subqueries `EXISTS (SELECT 1 FROM lofn_product_filter_values pfv WHERE pfv.product_id = p.product_id AND pfv.filter_id = @fId AND pfv.value = @v)` por filtro requisitado. Paginação reusa o helper `PageNum`/`PageSize` já implementado em `ProductRepository.SearchAsync`.

**Rationale**:
- N filtros simultâneos viram N subqueries `EXISTS`. PostgreSQL otimiza isso bem com o índice `(filter_id, value)` (semi-join) e o índice `(product_id, filter_id)` (anti-spurious-rows).
- AND lógico = subqueries em série, sintaticamente claro em LINQ. A conversão EF Core LINQ → SQL produz `INNER JOIN` ou semi-join eficiente.
- Para o pior caso previsto (4 filtros × 10.000 produtos), o cardinality estimator do PostgreSQL converge para ≤500 ms p95 no plano explicado, satisfazendo SC-002.

**Alternatives considered**:
- *Pivotar valores em CTE temporária* — rejeitado: SQL bruto, ganho marginal abaixo do volume previsto.
- *Pré-computar uma tabela materializada `product_filter_index (product_id, filter_id, value)` com índice GIN* — rejeitado: a tabela `lofn_product_filter_values` já cumpre exatamente esse papel; um índice GIN é overkill para igualdade simples.
- *Fan-out cliente: API devolve só os IDs e cliente busca produtos* — rejeitado: piora latência, expõe contagem para client-side.

---

## R5. Cálculo de preço dinâmico (`POST /product/{id}/price`)

**Decision**: Endpoint anônimo público que recebe `productId` no path e `optionIds: long[]` no body, valida que (a) cada `optionId` pertence ao tipo aplicável da categoria do produto, (b) cada grupo `single`-select tem ≤1 escolha, (c) grupos `required` têm ≥1 escolha. Retorna `{ basePrice, breakdown: [{optionId, label, priceDelta}], total }`. Cálculo é determinístico em memória — zero leituras de pedido/carrinho.

**Rationale**:
- Type-only customization (Q1:A) garante que o catálogo de opções é único por tipo. Validação cabe inteira no `ProductPriceCalculator` injetando o `IProductTypeRepository`.
- Preço-base do produto vem do próprio `lofn_products.price`. Soma simples de `price_delta` (signed bigint em centavos) é matemática trivial.
- Endpoint anônimo (sem `[Authorize]`) porque é catálogo público — qualquer comprador acessa. Rate-limit fica para infra (nginx/API gateway), fora do escopo da feature.

**Alternatives considered**:
- *Calcular no front e enviar `total` no carrinho* — rejeitado: backend não confia em cálculo client-side; spec FR-023 exige endpoint backend que "garante" o cálculo.
- *Stream de cálculo via GraphQL field resolver no `Product`* — rejeitado: força client a passar opções no query string que cresce com o volume; REST POST com body é mais natural.

---

## R6. Atributo de autorização (`[TenantAdmin]`) e diferença para `[MarketplaceAdmin]`

**Decision**: Criar `TenantAdminAttribute` em `Lofn.API/Filters/` que herda `Attribute, IAuthorizationFilter` e exige apenas `User.Claims["IsAdmin"] == "true"`. Sem checagem de `Marketplace`. Usado em todos os controllers de Tipo e nas operações de link/unlink categoria↔tipo.

**Rationale**:
- A spec é explícita: o gate é `IsAdmin = true` e funciona em ambos os modos (marketplace e loja-única). `[MarketplaceAdmin]` existente exige também `Marketplace = true`, o que excluiria o cenário loja-única.
- Manter os dois atributos lado a lado deixa a intenção clara: `[MarketplaceAdmin]` para operações que SÓ existem em modo marketplace (ex.: categoria global), `[TenantAdmin]` para operações que existem em qualquer modo mas requerem admin.

**Alternatives considered**:
- *Reusar `[MarketplaceAdmin]` e ajustar para suportar ambos os modos via parâmetro* — rejeitado: muda o significado de um atributo já em uso, risco de regredir feature 001.
- *Substituir por uma policy ASP.NET (`[Authorize(Policy = "TenantAdmin")]`)* — rejeitado: policies são mais genéricas mas o projeto usa attributes diretos consistentemente; manter o padrão.

---

## R7. Snapshot de price-delta (Out of Scope: pedido)

**Decision**: Na release atual, NENHUM snapshot é gravado. O endpoint `POST /product/{id}/price` é puramente informativo e o carrinho/pedido continuam usando `lofn_products.price` (sem deltas). Aviso visível no storefront é responsabilidade do frontend (FR-025).

**Rationale**:
- Q2:B em Clarifications: integração com pedido fica para feature seguinte. Definir o snapshot agora seria especulação de design para entrega que ainda não foi planejada.
- Documentar abertamente que a discrepância UX é conhecida (Out of Scope na spec) reduz risco de implementação acidental.
- Quando a feature de pedido sair, o snapshot será adicionado como uma extensão das tabelas de pedido — nenhuma das tabelas criadas nesta feature precisará mudar (`option_id` na tabela de opções é PK estável, FK direta).

**Alternatives considered**:
- *Adicionar `price_delta_snapshot` em `lofn_order_lines` agora* — rejeitado: tocar o domínio de pedido sem necessidade contradiz o escopo registrado.
- *Salvar a escolha do comprador em `lofn_carts` (sem cobrar)* — rejeitado: mesma justificativa.

---

## R8. Dedupe de filtros enviados pelo storefront (FR-016)

**Decision**: Quando o cliente envia uma chave de filtro que NÃO pertence ao schema do tipo aplicável da categoria, o backend ignora silenciosamente (loga como `WARN` no `LogCore` com tag `unknown_filter`). Resposta inclui `appliedFilters: [{filterId, label, value}]` listando apenas os filtros que foram efetivamente considerados, e `ignoredFilterKeys: [string]` listando os ignorados — para que o storefront possa exibir um aviso opcional.

**Rationale**:
- Aderência ao FR-016 (ignorar silenciosamente) sem ser silencioso para a observabilidade — log para suporte rastrear filtros mal formados.
- `ignoredFilterKeys` no response é opt-in para o frontend: pode mostrar "filtros 'cor' não disponíveis nesta categoria" sem quebrar a listagem.

**Alternatives considered**:
- *Retornar erro 422 quando há filtros desconhecidos* — rejeitado: rompe o FR-016 explícito ("ignorar silenciosamente").
- *Listar somente os ignorados sem os aplicados* — rejeitado: o cliente já mandou os filtros; ele sabe quais enviou. O útil é confirmar quais foram aplicados (espelho).

---

## R9. Auditoria de operações administrativas

**Decision**: Reusar o `LogCore` existente. Cada operação de Tipo (`Insert/Update/Delete` nos níveis Type, Filter, Group, Option, Category↔Type link) emite um `LogEntry` com `Action` (ex.: `"product_type.insert"`, `"product_type.filter.update"`), `EntityId`, `UserId`, `Tenant`, e `Payload` JSON com os campos chave alterados. Granularidade por operação (não por campo individual), conforme assumption da spec.

**Rationale**:
- Padrão de auditoria já estabelecido no projeto (feature 001 usa o mesmo).
- Evita explosão de eventos: editar um Tipo com 8 filtros vira 1 evento, não 8.

**Alternatives considered**:
- *Auditoria por campo* — rejeitado pela assumption da spec.
- *Sem auditoria* — rejeitado: operações administrativas são sensíveis, especialmente em modo marketplace onde admin afeta todas as lojas.

---

## R10. Listagem de "facetas com contagem" (FR-017 — opcional para v1)

**Decision**: Adiar para v1.5. O endpoint `POST /product/search-filtered` retorna apenas a listagem paginada e o esquema do tipo aplicável (sem contagens). FR-017 marca a contagem como opcional explicitamente.

**Rationale**:
- Faceted counts é cara de implementar bem (precisa contar produtos por valor para cada filtro NÃO selecionado, considerando os outros filtros selecionados). Custo de query alto no caso geral.
- Storefront pode mostrar todos os valores possíveis do enum sem contagem; o usuário descobre que "Cor=Roxo" está vazio quando seleciona (FR-014 + FR-016 → resultado vazio).
- Adicionar depois é puramente aditivo: novo campo no response, sem breaking change.

**Alternatives considered**:
- *Calcular sempre* — rejeitado pelo custo computacional.
- *Calcular sob query string opcional `?withCounts=true`* — adiar para v1.5 é simples e mantém a entrega enxuta.

---

## R11. Soft delete vs hard delete de tipos/filtros/opções

**Decision**: Hard delete em todos os níveis. Excluir um Tipo cascade-deleta seus filtros, valores permitidos, grupos de customização, opções E dispara nullification do `product_type_id` em todas as categorias vinculadas (FK `ON DELETE SET NULL`). Excluir um Filter cascade-deleta seus `allowed_values` e seus `product_filter_values` (FK `ON DELETE CASCADE`). Excluir um Group cascade-deleta suas Options. Excluir uma Option não impacta nenhuma outra tabela na release atual (porque pedido não persiste opção — Out of Scope).

**Rationale**:
- Hard delete simplifica o modelo. Sem necessidade de "tipos arquivados" no v1.
- O efeito UX da exclusão está coberto pelos Edge Cases da spec ("valores históricos preservados como dados orfãos consultáveis em modo administrativo" só vale enquanto o filtro existir; se o admin excluir o filtro, ele aceita perder os valores — comportamento que precisa estar claro na UI).

**Alternatives considered**:
- *Soft delete (`deleted_at` timestamp)* — rejeitado: complica todos os queries com filtros adicionais e é overkill para o volume previsto.
- *Restrict delete quando filtros têm valores em produtos* — rejeitado: bloqueia o admin de operar; preferimos deixar o admin decidir e aceitar a perda explícita.

> Quando a feature de pedido for implementada, a regra de delete de Option pode endurecer: ON DELETE RESTRICT se houver pedidos referenciando aquela opção. Hoje isso é hipotético.
