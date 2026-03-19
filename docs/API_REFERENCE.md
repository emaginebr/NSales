# API Reference - Lofn Backend

> Documentação completa da API REST do backend Lofn para implementação do frontend. Inclui todos os endpoints, DTOs, enums e estruturas de dados.

**Created:** 2026-03-18
**Last Updated:** 2026-03-18

---

## Informações Gerais

| Item | Valor |
|------|-------|
| **Base URL (Dev)** | `https://localhost:44374` |
| **Autenticação** | Bearer Token via header `Authorization` |
| **Content-Type** | `application/json` (exceto upload de imagens) |
| **Respostas de Erro** | `400` Bad Request, `401` Unauthorized, `404` Not Found, `500` Internal Server Error |

---

## Autenticação

Todos os endpoints (exceto os marcados como **Público**) exigem o header:

```
Authorization: Bearer <token>
```

O token é validado via `NAuth`. Caso inválido ou ausente, retorna `401 Unauthorized`.

---

## Endpoints

### 1. Product Controller

**Prefixo:** `/product`

#### POST `/{storeSlug}/insert` — Criar produto

- **Auth:** Requerida
- **Request Body:** `ProductInsertInfo`
- **Response:** `ProductInfo`

#### POST `/{storeSlug}/update` — Atualizar produto

- **Auth:** Requerida
- **Request Body:** `ProductUpdateInfo`
- **Response:** `ProductInfo`

#### POST `/search` — Buscar produtos (paginado)

- **Auth:** Público
- **Request Body:** `ProductSearchParam`
- **Response:** `ProductListPagedResult`

#### GET `/{storeSlug}/getById/{productId}` — Obter produto por ID

- **Auth:** Requerida
- **Params:** `storeSlug` (string), `productId` (long)
- **Response:** `ProductInfo`

#### GET `/{storeSlug}/category/{categorySlug}/listActive` — Listar produtos ativos por categoria (público)

- **Auth:** Público
- **Params:** `storeSlug` (string), `categorySlug` (string)
- **Response:** `IList<ProductInfo>`
- **Descrição:** Retorna todos os produtos ativos de uma categoria (pelo slug), filtrado pelo slug da loja

#### GET `/getBySlug/{productSlug}` — Obter produto por slug

- **Auth:** Público
- **Params:** `productSlug` (string)
- **Response:** `ProductInfo`

---

### 2. Category Controller

**Prefixo:** `/category`

#### GET `/{storeSlug}/listActive` — Listar categorias com produtos ativos (público)

- **Auth:** Público
- **Params:** `storeSlug` (string)
- **Response:** `IList<CategoryInfo>` (com `productCount` de produtos ativos)
- **Descrição:** Retorna categorias da loja que possuem pelo menos 1 produto ativo. Categorias sem produtos ativos são excluídas

#### GET `/{storeSlug}/getBySlug/{categorySlug}` — Obter categoria por slug (público)

- **Auth:** Público
- **Params:** `storeSlug` (string), `categorySlug` (string)
- **Response:** `CategoryInfo`

#### GET `/{storeSlug}/list` — Listar categorias da loja

- **Auth:** Requerida
- **Params:** `storeSlug` (string)
- **Response:** `IList<CategoryInfo>`

#### GET `/{storeSlug}/getById/{categoryId}` — Obter categoria por ID

- **Auth:** Requerida
- **Params:** `storeSlug` (string), `categoryId` (long)
- **Response:** `CategoryInfo`

#### POST `/{storeSlug}/insert` — Criar categoria

- **Auth:** Requerida
- **Request Body:** `CategoryInsertInfo`
- **Response:** `CategoryInfo`

#### POST `/{storeSlug}/update` — Atualizar categoria

- **Auth:** Requerida
- **Request Body:** `CategoryUpdateInfo`
- **Response:** `CategoryInfo`

#### DELETE `/{storeSlug}/delete/{categoryId}` — Deletar categoria

- **Auth:** Requerida
- **Params:** `storeSlug` (string), `categoryId` (long)
- **Response:** `204 No Content`

---

### 3. Image Controller

**Prefixo:** `/image`

#### POST `/upload/{productId}` — Upload de imagem

- **Auth:** Requerida
- **Content-Type:** `multipart/form-data`
- **Params:** `productId` (long), `sortOrder` (query, int, opcional, default: 0)
- **Body:** `file` (IFormFile, máx. 100MB)
- **Response:** `ProductImageInfo`

#### GET `/list/{productId}` — Listar imagens do produto

- **Auth:** Requerida
- **Params:** `productId` (long)
- **Response:** `IList<ProductImageInfo>`

#### DELETE `/delete/{imageId}` — Deletar imagem

- **Auth:** Requerida
- **Params:** `imageId` (long)
- **Response:** `204 No Content`

---

### 4. Order Controller

**Prefixo:** `/order`

#### POST `/update` — Atualizar pedido

- **Auth:** Requerida
- **Request Body:** `OrderInfo`
- **Response:** `OrderInfo`

#### POST `/search` — Buscar pedidos (paginado)

- **Auth:** Requerida
- **Request Body:** `OrderSearchParam`
- **Response:** `OrderListPagedResult`

#### POST `/list` — Listar pedidos com filtros

- **Auth:** Requerida
- **Request Body:** `OrderParam`
- **Response:** `IList<OrderInfo>`

#### GET `/getById/{orderId}` — Obter pedido por ID

- **Auth:** Requerida
- **Params:** `orderId` (long)
- **Response:** `OrderInfo`

---

### 5. Store Controller

**Prefixo:** `/store`

#### GET `/list` — Listar lojas do usuário (owner)

- **Auth:** Requerida
- **Response:** `IList<StoreInfo>`
- **Descrição:** Lista todas as lojas onde o usuário autenticado é owner, independente do status

#### GET `/listActive` — Listar lojas ativas (público)

- **Auth:** Público
- **Response:** `IList<StoreInfo>`
- **Descrição:** Lista apenas lojas com status Active, independente do owner

#### GET `/getBySlug/{storeSlug}` — Obter loja por slug (público)

- **Auth:** Público
- **Params:** `storeSlug` (string)
- **Response:** `StoreInfo`
- **Descrição:** Retorna a loja pelo slug. Retorna `404` se não encontrada e `400` se a loja não estiver ativa

#### GET `/getById/{storeId}` — Obter loja por ID

- **Auth:** Requerida
- **Params:** `storeId` (long)
- **Response:** `StoreInfo`

#### POST `/insert` — Criar loja

- **Auth:** Requerida
- **Request Body:** `StoreInsertInfo`
- **Response:** `StoreInfo`
- **Descrição:** Cria a loja com status Active por padrão

#### POST `/update` — Atualizar loja

- **Auth:** Requerida
- **Request Body:** `StoreUpdateInfo`
- **Response:** `StoreInfo`

#### POST `/uploadLogo/{storeId}` — Upload de logomarca

- **Auth:** Requerida
- **Content-Type:** `multipart/form-data`
- **Params:** `storeId` (long)
- **Body:** `file` (IFormFile, máx. 100MB)
- **Response:** `StoreInfo`

#### DELETE `/delete/{storeId}` — Deletar loja

- **Auth:** Requerida
- **Params:** `storeId` (long)
- **Response:** `204 No Content`

---

### 6. StoreUser Controller

**Prefixo:** `/storeuser`

#### GET `/{storeSlug}/list` — Listar usuários da loja

- **Auth:** Requerida
- **Params:** `storeSlug` (string)
- **Response:** `IList<StoreUserInfo>`

#### POST `/{storeSlug}/insert` — Adicionar usuário à loja

- **Auth:** Requerida
- **Request Body:** `StoreUserInsertInfo`
- **Response:** `StoreUserInfo`

#### DELETE `/{storeSlug}/delete/{storeUserId}` — Remover usuário da loja

- **Auth:** Requerida
- **Params:** `storeSlug` (string), `storeUserId` (long)
- **Response:** `204 No Content`

---

## DTOs (Data Transfer Objects)

### Product

#### ProductInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `productId` | `long` | ID do produto |
| `storeId` | `long?` | ID da loja |
| `categoryId` | `long?` | ID da categoria |
| `slug` | `string` | Slug único do produto |
| `image` | `string` | Nome do arquivo da imagem principal |
| `imageUrl` | `string` | URL completa da imagem principal |
| `name` | `string` | Nome do produto |
| `description` | `string` | Descrição do produto |
| `price` | `double` | Preço do produto |
| `frequency` | `int` | Frequência (em dias) |
| `limit` | `int` | Limite de unidades |
| `status` | `ProductStatusEnum` | Status do produto |
| `images` | `ProductImageInfo[]` | Lista de imagens do produto |

#### ProductInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `categoryId` | `long?` | ID da categoria |
| `name` | `string` | Nome do produto |
| `description` | `string` | Descrição |
| `price` | `double` | Preço |
| `frequency` | `int` | Frequência (em dias) |
| `limit` | `int` | Limite de unidades |
| `status` | `ProductStatusEnum` | Status |

#### ProductUpdateInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `productId` | `long` | ID do produto (obrigatório) |
| `categoryId` | `long?` | ID da categoria |
| `name` | `string` | Nome |
| `description` | `string` | Descrição |
| `price` | `double` | Preço |
| `frequency` | `int` | Frequência |
| `limit` | `int` | Limite |
| `status` | `ProductStatusEnum` | Status |

#### ProductImageInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `imageId` | `long` | ID da imagem |
| `productId` | `long` | ID do produto |
| `image` | `string` | Nome do arquivo |
| `imageUrl` | `string` | URL completa da imagem |
| `sortOrder` | `int` | Ordem de exibição |

#### ProductSearchParam

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `userSlug` | `string` | Slug do usuário (filtro) |
| `networkSlug` | `string` | Slug da rede (filtro) |
| `storeId` | `long?` | ID da loja (filtro) |
| `userId` | `long?` | ID do usuário (filtro) |
| `keyword` | `string` | Termo de busca |
| `onlyActive` | `bool` | Filtrar apenas produtos ativos |
| `pageNum` | `int` | Número da página |

> Herda de `ProductSearchInternalParam` adicionando `userSlug` e `networkSlug`.

#### ProductListPagedResult

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `products` | `ProductInfo[]` | Lista de produtos |
| `pageNum` | `int` | Página atual |
| `pageCount` | `int` | Total de páginas |

---

### Order

#### OrderInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `orderId` | `long` | ID do pedido |
| `storeId` | `long?` | ID da loja |
| `userId` | `long` | ID do comprador |
| `sellerId` | `long?` | ID do vendedor |
| `status` | `OrderStatusEnum` | Status do pedido |
| `createdAt` | `DateTime` | Data de criação |
| `updatedAt` | `DateTime` | Data da última atualização |
| `user` | `UserInfo` | Dados do comprador (NAuth) |
| `seller` | `UserInfo` | Dados do vendedor (NAuth) |
| `items` | `OrderItemInfo[]` | Itens do pedido |

#### OrderItemInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `itemId` | `long` | ID do item |
| `orderId` | `long` | ID do pedido |
| `productId` | `long` | ID do produto |
| `quantity` | `int` | Quantidade |
| `product` | `ProductInfo` | Dados do produto |

#### OrderSearchParam

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `storeId` | `long` | ID da loja |
| `userId` | `long?` | ID do usuário (filtro) |
| `sellerId` | `long?` | ID do vendedor (filtro) |
| `pageNum` | `int` | Número da página |

#### OrderParam

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `storeId` | `long` | ID da loja |
| `userId` | `long` | ID do usuário |
| `status` | `OrderStatusEnum?` | Status (filtro opcional) |

#### OrderListPagedResult

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `orders` | `OrderInfo[]` | Lista de pedidos |
| `pageNum` | `int` | Página atual |
| `pageCount` | `int` | Total de páginas |

---

### Category

#### CategoryInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `categoryId` | `long` | ID da categoria |
| `slug` | `string` | Slug da categoria |
| `name` | `string` | Nome da categoria |
| `storeId` | `long` | ID da loja |
| `productCount` | `int` | Quantidade de produtos na categoria |

#### CategoryInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `name` | `string` | Nome da categoria |

#### CategoryUpdateInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `categoryId` | `long` | ID da categoria |
| `name` | `string` | Novo nome |

---

### Store

#### StoreInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `storeId` | `long` | ID da loja |
| `slug` | `string` | Slug da loja |
| `name` | `string` | Nome da loja |
| `ownerId` | `long` | ID do proprietário |
| `logo` | `string` | Nome do arquivo da logomarca |
| `logoUrl` | `string` | URL completa da logomarca |
| `status` | `StoreStatusEnum` | Status da loja |

#### StoreInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `name` | `string` | Nome da loja |

#### StoreUpdateInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `storeId` | `long` | ID da loja |
| `name` | `string` | Novo nome |
| `status` | `StoreStatusEnum` | Novo status |

#### StoreUserInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `storeUserId` | `long` | ID do vínculo |
| `storeId` | `long` | ID da loja |
| `userId` | `long` | ID do usuário |
| `user` | `UserInfo` | Dados do usuário (NAuth) |

#### StoreUserInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `userId` | `long` | ID do usuário a adicionar |

---

### Settings

#### LofnSetting

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `apiUrl` | `string` | URL base da API |
| `bucketName` | `string` | Nome do bucket de armazenamento |

---

## Enums

### StoreStatusEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `0` | `Inactive` | Loja inativa |
| `1` | `Active` | Loja ativa (padrão) |
| `2` | `Suspended` | Loja suspensa |

### ProductStatusEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Active` | Produto ativo e visível |
| `2` | `Inactive` | Produto inativo/oculto |
| `3` | `Expired` | Produto expirado |

### OrderStatusEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Incoming` | Pedido recebido |
| `2` | `Active` | Pedido ativo |
| `3` | `Suspended` | Pedido suspenso |
| `4` | `Finished` | Pedido finalizado |
| `5` | `Expired` | Pedido expirado |

### OrderFrequencyEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `7` | `Weekly` | Semanal |
| `30` | `Monthly` | Mensal |
| `365` | `Annual` | Anual |

---

## Referências Externas

### UserInfo (NAuth.DTO)

DTO externo do pacote NAuth, referenciado em `OrderInfo.User`, `OrderInfo.Seller` e `StoreUserInfo.User`. Contém dados do usuário autenticado (ID, nome, email, etc.).

---

## Resumo

| Recurso | Endpoints | DTOs |
|---------|-----------|------|
| **Product** | 5 | 9 (inclui enums e params) |
| **Category** | 5 | 3 |
| **Image** | 3 | 1 |
| **Order** | 4 | 7 (inclui enums e params) |
| **Store** | 7 | 4 (inclui enum) |
| **StoreUser** | 3 | 2 |
| **Total** | **25** | **25** |

- **Endpoints públicos:** `POST /product/search`, `GET /product/getBySlug/{slug}`, `GET /store/listActive`, `GET /store/getBySlug/{storeSlug}`, `GET /category/{storeSlug}/listActive`, `GET /category/{storeSlug}/getBySlug/{categorySlug}`, `GET /product/{storeSlug}/category/{categorySlug}/listActive`
- **Todos os demais requerem Bearer Token**
- **Serialização JSON:** propriedades em `camelCase` via `[JsonPropertyName]`
