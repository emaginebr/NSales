# Feature Specification: Tipo de Produto, filtros e customizações

**Feature Branch**: `003-product-type-filters`
**Created**: 2026-04-29
**Status**: Draft
**Input**: User description: "Crie um tipo de produto: Ex: Calçado, Roupa, Carro, Equipamento, Comida. Dependendo o tipo de produto, o administrador poderá cadastrar filtros (ex: Calçados deve ter um número, uma marca, uma cor, se é Masculino ou Feminino; Roupa podem ter um número e uma marca; Carros, podem ter uma marca, ano de fabricação, Modelo; Equipamento podem ter uma marca, um processador, uma quantidade de memória). Alguns tipo de produtos tb vao poder ser customizados (ex: Equipamentos de informatica, poderam ter um processador i3, mas pode ser trocado por um i5 por + R$500 ou i7 por + R$900; Comida, poderá retirar o queijo, o bacon, a alface; Pode incluir bacon extra). Toda categoria pode (ou não) estar relacionada a um tipo. Apenas o administrador (isAdmin=True) pode cadastrar esses filtros do tipo de produto. Deve criar os endpoints necessários para criar o filtro na categoria. Deve permitir obter uma lista de produtos paginada baseada no filtro e na categoria."

## Clarifications

### Session 2026-04-29

- Q: Onde vivem as customizações — Tipo, Produto, ou híbrido? → A: **Type-only**. Admin define grupos e opções (com price-deltas) exclusivamente sob o Tipo; todos os produtos sob esse tipo herdam o mesmo catálogo de customização sem possibilidade de override por produto. Vendedor não decide nem ajusta.
- Q: A integração com pedido/carrinho está nesta feature ou é deferida? → A: **Catálogo-only**. Esta feature entrega: (a) admin cadastra customizações, (b) storefront público exibe os grupos/opções e calcula preço dinâmico no detalhe do produto. NÃO entrega: gravar as opções na linha do pedido nem travar o preço calculado no checkout. O carrinho/pedido continuam usando o preço-base do produto. A integração com pedido fica para uma feature seguinte. Aceita-se a discrepância UX (comprador vê R$3.900 no detalhe mas adiciona R$3.000 ao carrinho) — vendedores ficam cientes desse limite explícito até a feature de pedido sair.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Admin define um Tipo de Produto e o esquema de filtros (Priority: P1)

Como administrador do tenant (`IsAdmin = true`), quero criar um Tipo de Produto (ex.: "Calçado") e declarar quais atributos os produtos desse tipo devem expor — número, marca, cor, gênero — para que vendedores cadastrem produtos consistentes e compradores filtrem o catálogo por esses atributos.

**Why this priority**: Sem o tipo e seu esquema de filtros nada da feature funciona. Toda a cadeia (vendedor preencher, comprador filtrar, customizar) depende dessa definição existir. É também o ato administrativo isolável que produz valor visível imediatamente: a partir do momento em que existe, novos produtos podem se beneficiar dele.

**Independent Test**: Pode ser totalmente validado criando um tipo "Calçado" com filtros {Número (numérico, obrigatório), Marca (texto, obrigatório), Cor (enum: Preto/Branco/Marrom, obrigatório), Gênero (enum: Masculino/Feminino/Unissex, obrigatório)}, recuperando o tipo via leitura e confirmando que todos os filtros voltam corretamente. Já entrega valor: outros admins podem auditar/clonar.

**Acceptance Scenarios**:

1. **Given** o admin está autenticado com `IsAdmin = true`, **When** ele cria um tipo "Calçado" com 4 filtros (incluindo um enum), **Then** o tipo é persistido com o esquema completo e a operação retorna sucesso.
2. **Given** um usuário sem `IsAdmin = true` está autenticado, **When** ele tenta criar um tipo, **Then** a operação é recusada com erro de autorização.
3. **Given** um tipo já existe, **When** o admin atualiza o esquema (renomeia um filtro, adiciona um novo), **Then** o esquema é atualizado e produtos pré-existentes daquele tipo continuam válidos (atributos novos ficam vazios, renomeados são preservados pelo identificador interno).
4. **Given** o admin tenta criar dois tipos com o mesmo nome no mesmo tenant, **When** ele salva, **Then** a segunda tentativa é recusada com mensagem clara.

---

### User Story 2 - Admin vincula Tipo a uma Categoria (Priority: P1)

Como administrador, quero vincular um Tipo de Produto a uma Categoria existente (ex.: "Calçado" → categoria "Calçados/Tênis") para que produtos cadastrados sob essa categoria automaticamente herdem o esquema de filtros do tipo.

**Why this priority**: É o que conecta o esquema (US1) ao catálogo real. Sem este vínculo, um vendedor cria um produto e não sabe quais filtros aplicar, e o storefront não sabe que facetas oferecer.

**Independent Test**: Vincule o tipo "Calçado" à categoria "Calçados/Tênis", consulte a categoria e confirme que o tipo associado aparece na resposta junto com seu esquema de filtros.

**Acceptance Scenarios**:

1. **Given** existe um tipo "Calçado" e uma categoria "Tênis", **When** o admin vincula os dois, **Then** a categoria passa a referenciar o tipo e o esquema de filtros fica acessível por meio da categoria.
2. **Given** uma categoria está vinculada ao tipo "Calçado", **When** o admin remove o vínculo, **Then** a categoria volta a não ter tipo e produtos da categoria perdem o esquema de filtros sugerido (valores existentes não são apagados, mas deixam de ser exibidos como facetas).
3. **Given** uma categoria filha não tem tipo próprio mas seu pai tem, **When** o sistema determina o tipo aplicável a um produto da categoria filha, **Then** ele usa o tipo do ancestral mais próximo (closest-ancestor wins).
4. **Given** o admin tenta vincular dois tipos diferentes à mesma categoria, **When** salva o segundo, **Then** o vínculo é único — o segundo substitui o primeiro com confirmação.

---

### User Story 3 - Vendedor cadastra produto preenchendo valores de filtros (Priority: P1)

Como vendedor, quero que ao cadastrar um produto sob uma categoria tipada (ex.: "Tênis Nike Air") o sistema me peça os valores dos filtros do tipo (Número 42, Marca Nike, Cor Branco, Gênero Masculino), validando os formatos e enums declarados.

**Why this priority**: Sem dados nos produtos, a US4 (filtragem pública) não retorna nada útil. É o ponto onde a infra-estrutura administrativa vira dado real. Independente das US1/US2, mas só faz sentido se elas estiverem prontas.

**Independent Test**: Crie um produto na categoria tipada, envie os valores dos 4 filtros, recupere o produto e confirme que os valores estão associados a ele.

**Acceptance Scenarios**:

1. **Given** existe categoria "Tênis" tipada como "Calçado" com 4 filtros, **When** o vendedor cadastra "Tênis Nike Air" preenchendo os 4 valores, **Then** o produto é criado e os valores ficam associados aos filtros corretos.
2. **Given** o tipo declara um filtro como obrigatório, **When** o vendedor tenta salvar sem preencher esse filtro, **Then** a operação é recusada com mensagem listando os filtros obrigatórios faltando.
3. **Given** o filtro "Cor" é enum com valores {Preto, Branco, Marrom}, **When** o vendedor envia "Vermelho", **Then** a operação é recusada listando os valores permitidos.
4. **Given** a categoria do produto não tem tipo (nem ela nem ancestrais), **When** o vendedor cadastra o produto, **Then** o sistema permite salvar normalmente sem exigir valores de filtro (compatibilidade com produtos não tipados).

---

### User Story 4 - Comprador filtra catálogo por categoria + atributos do tipo (Priority: P1)

Como comprador, quero abrir a categoria "Tênis" no storefront público e filtrar por Número 42 e Cor Branco para ver apenas produtos que satisfazem ambos os critérios, paginados.

**Why this priority**: É a entrega de valor final — o motivo pelo qual existe o esquema. Sem ela, US1-US3 são metadados ociosos. Depende de todas as anteriores.

**Independent Test**: Cadastre 5 produtos com combinações variadas de Número e Cor, filtre por (Número=42, Cor=Branco), confirme que apenas os matches aparecem e que a paginação funciona.

**Acceptance Scenarios**:

1. **Given** 10 produtos na categoria "Tênis", **When** o comprador filtra por "Número=42", **Then** apenas produtos com esse valor aparecem na página 1 e o total reflete os matches.
2. **Given** o comprador filtra por dois atributos (Número=42 AND Cor=Branco), **When** a listagem é solicitada, **Then** apenas produtos que satisfazem ambos os critérios aparecem (interseção AND).
3. **Given** a categoria pai "Calçados" tem tipo herdado por subcategorias filhas, **When** o comprador filtra na categoria pai, **Then** o rollup pai⇡filho da feature 002 segue valendo e os filtros aplicam-se aos produtos das subcategorias também.
4. **Given** uma página tem mais de 20 resultados, **When** o comprador navega para a página 2, **Then** os filtros e ordenação são preservados e novos produtos aparecem.
5. **Given** o comprador filtra por um valor que não existe (ex.: Cor=Roxo), **When** a listagem responde, **Then** o resultado é vazio com paginação coerente (zero itens, zero páginas).

---

### User Story 5 - Admin define customizações por Tipo de Produto (Priority: P2)

Como administrador, quero declarar opções de customização para um tipo (ex.: para "Equipamento": grupo "Processador" com opções {i3 base, i5 +R$500, i7 +R$900}) para que compradores possam personalizar produtos desse tipo na hora da compra.

**Why this priority**: É a segunda metade da feature pedida pelo usuário. Não é P1 porque a base catálogo+filtros já entrega valor sem customização, mas é entrega obrigatória para o roadmap completo.

**Independent Test**: Defina um grupo de customização para o tipo "Equipamento", recupere o tipo e confirme que o grupo e suas opções (com price-deltas) aparecem no esquema.

**Acceptance Scenarios**:

1. **Given** existe o tipo "Equipamento", **When** o admin adiciona um grupo "Processador" com opções {i3 base R$0, i5 +R$500, i7 +R$900} e marca i3 como default, **Then** o esquema do tipo passa a expor o grupo com 3 opções e a default destacada.
2. **Given** existe o tipo "Comida", **When** o admin adiciona um grupo "Ingredientes a remover" com opções {Queijo, Bacon, Alface} (todas price-delta R$0, multi-seleção, opcionais) e outro grupo "Adicionais" com {Bacon extra +R$3}, **Then** ambos os grupos coexistem com semânticas distintas (multi vs single seleção).
3. **Given** um usuário sem `IsAdmin = true` está autenticado, **When** tenta editar customizações, **Then** a operação é recusada.
4. **Given** o admin tenta criar duas opções com o mesmo rótulo no mesmo grupo, **When** salva, **Then** a segunda é recusada (rótulos únicos por grupo).

---

### User Story 6 - Comprador visualiza customizações e preço dinâmico no catálogo (Priority: P2)

Como comprador, quero abrir um produto cujo tipo tem customizações, escolher minhas opções (ex.: trocar processador para i7) e ver o preço total ajustado no detalhe do produto, mesmo sabendo que o carrinho ainda cobrará apenas o preço-base nesta release.

**Why this priority**: Entrega a metade catálogo da customização — admin configura, comprador visualiza, preço é calculado dinamicamente. A integração com carrinho/pedido fica explícita e separadamente deferida para uma feature seguinte (ver Out of Scope abaixo). Mesmo parcial, valida com usuários reais a curva de aprendizado e a hierarquia visual antes do investimento na integração de pedido.

**Independent Test**: Abra um produto tipado com customização, simule a escolha de opções não-default e confirme que o preço final exibido reflete o preço-base + sum(price-deltas das opções escolhidas) — sem expectativa de propagar para o carrinho/pedido.

**Acceptance Scenarios**:

1. **Given** um produto tipo "Equipamento" com preço-base R$3.000 e customização "Processador {i3 base, i5 +500, i7 +900}", **When** o comprador escolhe i7 no detalhe do produto, **Then** o detalhe exibe preço total calculado de R$3.900 com a quebra (base + delta) visível.
2. **Given** o comprador volta para i3 (default), **When** o detalhe atualiza, **Then** o preço volta para R$3.000.
3. **Given** o comprador escolhe múltiplas opções em grupos compatíveis (multi-seleção), **When** o detalhe atualiza, **Then** o preço exibido soma todos os deltas das opções selecionadas.
4. **Given** o produto não tem customizações declaradas, **When** o comprador abre o detalhe, **Then** nenhum painel de customização aparece e o preço é o preço-base.
5. **Given** o comprador escolheu opções e clica "adicionar ao carrinho", **When** o item é adicionado, **Then** o carrinho usa o preço-base do produto (não o calculado) e exibe um aviso visível de que customizações ainda não são cobradas — esperando a feature de pedido.

---

### Edge Cases

- O que acontece quando um filtro do tipo é excluído pelo admin e há produtos com valores nesse filtro? → Os valores históricos são preservados como dados orfãos consultáveis em modo administrativo, mas deixam de aparecer como facetas no storefront. Admins podem optar por purgar ou migrar os valores.
- O que acontece se um filtro é renomeado? → A identidade interna do filtro é preservada; valores antigos seguem associados; apenas o rótulo muda. Storefront mostra o novo rótulo.
- O que acontece se um filtro enum tem um valor permitido removido? → Produtos com aquele valor histórico mantêm o valor (legível), mas a faceta pública deixa de exibir o valor como opção. Vendedores não conseguem mais selecioná-lo em novas edições.
- O que acontece quando uma categoria muda de tipo (ex.: deixa de ser "Calçado" e vira "Roupa")? → Valores de filtro de produtos da categoria são preservados, mas só os filtros do novo tipo viram facetas/validação. O admin é avisado da mudança de esquema antes de confirmar.
- O que acontece quando um vendedor envia mais valores de filtro do que o esquema declara? → Valores extras são silenciosamente descartados com aviso (não-bloqueante).
- O que acontece quando dois ou mais filtros do tipo têm o mesmo nome? → Não é permitido — admin recebe erro na criação/atualização do esquema.
- O que acontece quando um comprador filtra por um atributo que não existe no tipo da categoria? → O filtro é ignorado silenciosamente e a listagem responde aplicando apenas os filtros válidos.
- O que acontece quando o admin altera uma opção de customização (ex.: troca o price-delta de i7 de +900 para +1.200)? → O preço-de-tabela atualizado vale para novas adições ao carrinho dali em diante. Pedidos já fechados mantêm o preço original (snapshot no pedido).

## Requirements *(mandatory)*

### Functional Requirements

#### Tipo de Produto e Esquema (US1)

- **FR-001**: O sistema MUST permitir que um administrador (`IsAdmin = true`) crie, atualize e exclua Tipos de Produto identificados unicamente por nome dentro do tenant.
- **FR-002**: O sistema MUST recusar qualquer operação de gerenciamento de tipos vinda de usuários sem `IsAdmin = true`, retornando erro de autorização.
- **FR-003**: O sistema MUST permitir ao admin definir, por tipo, um esquema de filtros — cada filtro contendo: rótulo (texto exibido ao usuário), tipo de dado (texto livre, número inteiro, número decimal, booleano, ou enum com lista de valores permitidos), flag de obrigatoriedade.
- **FR-004**: O sistema MUST manter, para cada filtro, um identificador interno estável independente do rótulo, de modo que renomear o rótulo não invalide valores históricos.
- **FR-005**: O sistema MUST recusar a criação de dois filtros com o mesmo rótulo dentro do mesmo tipo.

#### Vínculo Categoria ↔ Tipo (US2)

- **FR-006**: O sistema MUST permitir ao admin vincular ou desvincular um Tipo a uma Categoria existente; o vínculo é 0..1 (cada categoria tem no máximo um tipo direto).
- **FR-007**: O sistema MUST aplicar o esquema do tipo do ancestral mais próximo da árvore de categorias (closest-ancestor wins) a categorias que não têm tipo direto vinculado, herdando recursivamente conforme a hierarquia da feature 002.
- **FR-008**: O sistema MUST expor, na leitura de uma categoria, o tipo aplicável (direto ou herdado) e a origem (id da categoria que define o vínculo) — sem exigir do consumidor uma resolução manual.

#### Cadastro de produto com valores (US3)

- **FR-009**: O sistema MUST aceitar, no cadastro/edição de produto, um conjunto de pares (filtro-id, valor) e validá-los contra o esquema do tipo aplicável à categoria do produto.
- **FR-010**: O sistema MUST recusar o salvamento se algum filtro obrigatório do esquema estiver ausente, listando todos os obrigatórios faltando.
- **FR-011**: O sistema MUST recusar valores que não respeitam o tipo de dado declarado (ex.: texto em campo numérico) ou que não estão na lista de valores permitidos de um filtro enum.
- **FR-012**: O sistema MUST aceitar produtos cuja categoria não tem tipo aplicável (sem esquema ativo) sem exigir valores de filtro — a feature é aditiva e compatível com produtos não tipados.
- **FR-013**: O sistema MUST descartar silenciosamente valores enviados que não correspondem a nenhum filtro do esquema (ignorar atributos extras), retornando um aviso não-bloqueante na resposta.

#### Listagem pública filtrada (US4)

- **FR-014**: O sistema MUST expor uma listagem pública paginada de produtos por categoria que aceita um conjunto de filtros (chave-valor) e retorna apenas produtos cuja categoria está no escopo (incluindo descendentes via rollup pai⇡filho da feature 002) E que satisfazem todos os filtros (interseção AND).
- **FR-015**: O sistema MUST aplicar paginação consistente (página, tamanho de página, total de itens, total de páginas) preservando os filtros entre páginas.
- **FR-016**: O sistema MUST ignorar silenciosamente filtros enviados que não pertencem ao esquema do tipo da categoria; a listagem responde com base nos filtros válidos.
- **FR-017**: O sistema MUST expor, junto com a listagem, a relação de filtros disponíveis (esquema aplicável) e — opcional para v1 — a contagem de cada valor possível dentro do conjunto resultante (faceta com contagens).

#### Customizações (US5)

- **FR-018**: O sistema MUST permitir ao admin declarar, por tipo, grupos de customização. Cada grupo contém: rótulo, semântica de seleção (single/multi), flag obrigatoriedade. As customizações são **definidas exclusivamente no Tipo** (Type-only): todos os produtos cujo tipo aplicável tem customizações herdam o mesmo catálogo de grupos e opções, sem possibilidade de override por produto. Vendedor não declara nem ajusta customizações.
- **FR-019**: O sistema MUST permitir ao admin declarar, em cada grupo, um conjunto de opções; cada opção contém rótulo, price-delta (signed, em centavos, padrão R$0,00), e flag de "default" (no máximo uma default por grupo single-select; nenhuma exigida por padrão em grupos multi-select).
- **FR-020**: O sistema MUST recusar a criação de duas opções com o mesmo rótulo dentro do mesmo grupo.
- **FR-021**: O sistema MUST recusar qualquer operação de gerenciamento de customizações vinda de usuários sem `IsAdmin = true`.

#### Ajuste de preço por customização (US6)

- **FR-022**: O sistema MUST expor, na leitura pública de um produto cujo tipo aplicável tem customizações, o esquema de grupos e opções com seus price-deltas e defaults — para que o consumidor (storefront) calcule o preço final no detalhe do produto.
- **FR-023**: O sistema MUST oferecer um endpoint de cálculo de preço que recebe um produto e um conjunto de opções escolhidas e retorna o preço total = `preço-base do produto + soma(price-delta) das opções escolhidas`. Esse endpoint serve apenas exibição de catálogo; o resultado NÃO é persistido em nenhum estado de pedido nesta feature.
- **FR-024**: A integração com pedido/carrinho está **fora do escopo desta feature** (ver seção "Out of Scope" abaixo). Operações de adicionar-ao-carrinho e checkout MUST continuar usando o preço-base do produto sem aplicar customizações; uma feature seguinte cuidará de gravar as opções escolhidas na linha do pedido e travar o preço-snapshot.
- **FR-025**: A interface pública (storefront) MUST exibir um aviso visível em produtos com customizações declaradas, indicando ao comprador que as customizações ainda não são aplicadas no carrinho — para evitar surpresa de cobrança. O aviso é controlado pela camada de apresentação; o backend apenas expõe o esquema e o endpoint de cálculo.

### Key Entities *(include if feature involves data)*

- **Tipo de Produto**: Classificador declarado por admin (ex.: Calçado, Carro, Comida). Tenant-scoped. Atributos: nome único no tenant, esquema de filtros, esquema de customizações.
- **Filtro do Tipo**: Atributo declarado pelo admin sob um tipo. Atributos: identificador estável, rótulo, tipo de dado, lista de valores permitidos (para enum), obrigatoriedade.
- **Valor de Filtro do Produto**: Par (produto, filtro, valor) — o preenchimento concreto que um vendedor faz ao cadastrar um produto da categoria tipada.
- **Grupo de Customização**: Conjunto de opções relacionadas declaradas sob um tipo. Atributos: rótulo, semântica de seleção (single/multi), obrigatoriedade.
- **Opção de Customização**: Item dentro de um grupo. Atributos: rótulo, price-delta (signed), flag de default.
- **Categoria** (entidade existente da feature 002): ganha vínculo opcional 0..1 com um Tipo de Produto.
- **Produto** (entidade existente): ganha relacionamento com seus Valores de Filtro (1:N) e — caso a opção (b) da clarificação não seja escolhida — um modo de carregar/aplicar customizações no fluxo de pedido.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das tentativas de gerenciar tipos, filtros ou customizações por usuários sem `IsAdmin = true` são recusadas com erro claro de autorização.
- **SC-002**: Em um catálogo com pelo menos 100 produtos tipados, a listagem pública filtrada retorna a primeira página em ≤500 ms p95 (mesmo budget da feature 002).
- **SC-003**: 100% dos produtos cadastrados sob categorias tipadas após a feature ser ativada têm valores de filtro completos para todos os filtros marcados como obrigatórios — zero produtos publicados em estado incompleto.
- **SC-004**: Compradores conseguem aplicar 3 filtros consecutivos em uma categoria com pelo menos 50 itens em menos de 10 segundos no fluxo padrão (carga de página + ajuste de filtros).
- **SC-005**: Em um produto cujo tipo tem customizações, 100% das combinações válidas de opções produzem preço total exibido no detalhe do produto = preço-base + soma(price-deltas) corretamente, verificável por amostragem manual ou automatizada. A persistência do preço no carrinho/pedido NÃO faz parte desta validação.
- **SC-006**: Produtos pré-existentes (sem tipo aplicável) continuam acessíveis e editáveis sem regressão — zero perdas de dado e zero quebras de fluxo de vendedor após a ativação.
- **SC-007**: Tickets de suporte com a queixa "não consigo cadastrar atributo X" (atributo não suportado por nenhum tipo) caem em 80% no primeiro mês após a feature, indicando que o esquema cobre o universo real de produtos do tenant.

## Out of Scope

Os itens abaixo foram explicitamente excluídos do escopo desta feature por decisão registrada em Clarifications:

- **Override de customização por produto**: Vendedor NÃO pode oferecer um catálogo de opções diferente do declarado pelo admin no Tipo. Customizações são Type-only. Caso esse uso surja como necessidade real, será uma feature separada que adicionará uma camada de override por produto.
- **Integração de customização com carrinho/pedido**: Esta feature NÃO grava a opção escolhida pelo comprador na linha do pedido nem trava o preço calculado no checkout. O carrinho e o pedido continuam usando o preço-base do produto. Uma feature seguinte (a planejar) fará: (a) extensão do modelo de linha de pedido para registrar opções, (b) snapshot de price-delta no momento do checkout, (c) exibição da customização escolhida no histórico de pedido.
- **Customizações afetando estoque/SKU**: Esta feature NÃO trata variações de estoque por combinação de opções (ex.: "5 unidades em i5 + 3 em i7"). O estoque continua sendo do produto inteiro como hoje.
- **Filtros multivalorados em valores de produto**: Um produto tem um único valor por filtro do tipo. Filtros multi-select por produto (ex.: "este tênis serve nos números 41 e 42") não estão suportados em v1.

## Assumptions

- **Escopo administrativo**: Tipos, esquemas de filtro e esquemas de customização são tenant-scoped. Em modo marketplace o admin do tenant define para todas as lojas; em modo loja-única o admin define para a loja. Não existe definição cross-tenant.
- **Permissão**: O gate é `IsAdmin = true` no contexto do tenant atual — herda o mesmo mecanismo de autorização usado pelas operações de categoria global da feature 001.
- **Cardinalidade do vínculo categoria↔tipo**: 0..1 por categoria. Uma categoria não pode ter dois tipos diretos; pode ter zero (e herdar do ancestral mais próximo, se houver) ou um.
- **Herança no tree**: Closest-ancestor type wins. Categorias filhas sem tipo direto usam o tipo do primeiro ancestral que tiver um. Se nenhum ancestral tem tipo, o produto é não-tipado e o cadastro segue o fluxo legado.
- **Compatibilidade**: A feature é aditiva. Produtos pré-existentes não são afetados; categorias sem tipo continuam funcionando como hoje. Nenhuma migração de dados existentes é exigida na ativação.
- **Filtro vs. Customização — distinção semântica**: Filtro é atributo descobrível e pesquisável (afeta listagem pública); Customização é escolha do comprador no momento da compra (afeta o preço final). Os dois esquemas vivem no mesmo Tipo mas têm fluxos diferentes.
- **Tipos de dado de filtro suportados em v1**: texto, número inteiro, número decimal, booleano, enum (lista fechada de valores). Tipos compostos (intervalo, data, multi-select num filtro) ficam fora do MVP.
- **Performance**: A listagem filtrada é otimizada para o caso comum (1 a 4 filtros simultâneos) sobre catálogos de até 10.000 produtos por tenant. Cargas maiores entram em escopo de evolução posterior.
- **Snapshot de preço em pedido**: Quando uma customização é aplicada e o pedido é fechado, o price-delta vigente NO MOMENTO do checkout é gravado na linha do pedido. Mudanças posteriores no esquema de customização não afetam pedidos passados.
- **Auditoria**: Operações administrativas (criar/editar/excluir tipo, filtro, customização, vínculo) são auditadas com granularidade por operação, sem detalhar campos individuais.
