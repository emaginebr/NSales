# API Reference - Lofn Backend

> Documentação completa da API REST do backend Lofn para implementação do frontend. Inclui todos os endpoints, DTOs, enums e estruturas de dados.

**Created:** 2026-03-18
**Last Updated:** 2026-03-21

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

> **Nota:** Os endpoints de leitura (`getById`, `featured`, `listActive`, `getBySlug`) foram migrados para o GraphQL. Use as queries `products`, `featuredProducts` e `storeBySlug` no endpoint `/graphql`.

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

---

### 2. Category Controller

**Prefixo:** `/category`

> **Nota:** Os endpoints de leitura (`listActive`, `getBySlug`, `list`, `getById`) foram migrados para o GraphQL. Use as queries `categories` e `myCategories` nos endpoints `/graphql` e `/graphql/admin`.

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

### 4. Store Controller

**Prefixo:** `/store`

> **Nota:** Os endpoints de leitura (`list`, `listActive`, `getBySlug`, `getById`) foram migrados para o GraphQL. Use as queries `stores`, `storeBySlug` e `myStores` nos endpoints `/graphql` e `/graphql/admin`.

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

### 5. StoreUser Controller

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

### 6. ShopCar Controller

**Prefixo:** `/shopcar`

#### POST `/insert` — Criar carrinho de compras

- **Auth:** Requerida
- **Request Body:** `ShopCarInfo`
- **Response:** `ShopCarInfo`

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
| `discount` | `double` | Desconto do produto (default: 0) |
| `frequency` | `int` | Frequência (em dias) |
| `limit` | `int` | Limite de unidades |
| `status` | `ProductStatusEnum` | Status do produto |
| `productType` | `ProductTypeEnum` | Tipo do produto (default: Physical) |
| `featured` | `bool` | Produto em destaque |
| `createdAt` | `DateTime` | Data de criação |
| `updatedAt` | `DateTime` | Data da última atualização |
| `images` | `ProductImageInfo[]` | Lista de imagens do produto |

#### ProductInsertInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `categoryId` | `long?` | ID da categoria |
| `name` | `string` | Nome do produto |
| `description` | `string` | Descrição |
| `price` | `double` | Preço |
| `discount` | `double` | Desconto (default: 0) |
| `frequency` | `int` | Frequência (em dias) |
| `limit` | `int` | Limite de unidades |
| `status` | `ProductStatusEnum` | Status |
| `productType` | `ProductTypeEnum` | Tipo do produto (default: Physical) |
| `featured` | `bool` | Em destaque |

#### ProductUpdateInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `productId` | `long` | ID do produto (obrigatório) |
| `categoryId` | `long?` | ID da categoria |
| `name` | `string` | Nome |
| `description` | `string` | Descrição |
| `price` | `double` | Preço |
| `discount` | `double` | Desconto (default: 0) |
| `frequency` | `int` | Frequência |
| `limit` | `int` | Limite |
| `status` | `ProductStatusEnum` | Status |
| `productType` | `ProductTypeEnum` | Tipo do produto (default: Physical) |
| `featured` | `bool` | Em destaque |

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

### ShopCar

#### ShopCarInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `user` | `UserInfo` | Dados do usuário (NAuth) |
| `items` | `ShopCarItemInfo[]` | Itens do carrinho |
| `createdAt` | `DateTime` | Data de criação |

#### ShopCarItemInfo

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `product` | `ProductInfo` | Dados do produto |
| `quantity` | `int` | Quantidade |

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

### ProductTypeEnum

| Valor | Nome | Descrição |
|-------|------|-----------|
| `1` | `Physical` | Produto físico (padrão) |
| `2` | `InfoProduct` | Infoproduto / produto digital |

---

## GraphQL API

A API expõe dois endpoints GraphQL via HotChocolate, ambos com suporte a **offset-based pagination**, **projection**, **filtering** e **sorting**.

#### Paginação (Offset-Based)

Todas as queries que retornam listas suportam paginação offset-based com os seguintes argumentos:

| Argumento | Tipo | Default | Descrição |
|-----------|------|---------|-----------|
| `skip` | `Int` | `0` | Quantidade de registros a pular |
| `take` | `Int` | `10` | Quantidade de registros a retornar (máx: 50) |

O retorno é envelopado em um tipo `CollectionSegment` com a seguinte estrutura:

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `items` | `[T]` | Lista de itens da página atual |
| `pageInfo` | `CollectionSegmentInfo` | Informações de navegação (`hasNextPage`, `hasPreviousPage`) |
| `totalCount` | `Int` | Total de registros (sempre disponível) |

> **Nota:** A query `storeBySlug` não utiliza paginação pois retorna um único registro.

### Endpoint Público: `/graphql`

Playground interativo (Banana Cake Pop) disponível em `https://localhost:44374/graphql/`.

Não requer autenticação. Expõe apenas dados ativos/públicos.

#### Queries disponíveis

| Query | Retorno | Descrição |
|-------|---------|-----------|
| `stores(skip, take)` | `StoreCollectionSegment` | Lojas ativas (`status = 1`) |
| `products(skip, take)` | `ProductCollectionSegment` | Produtos ativos (`status = 1`) |
| `categories(skip, take)` | `CategoryCollectionSegment` | Categorias que possuem pelo menos 1 produto ativo |
| `storeBySlug(slug: String!)` | `[Store]` | Loja ativa pelo slug (sem paginação) |
| `featuredProducts(storeSlug: String!, skip, take)` | `ProductCollectionSegment` | Produtos ativos e em destaque da loja |

#### Campos ocultos no schema público

O tipo `Store` no endpoint público **não expõe**: `storeUsers`, `ownerId`.

#### Exemplo

```graphql
{
  stores(skip: 0, take: 10) {
    items {
      storeId
      name
      slug
      logoUrl
      products {
        productId
        name
        price
        imageUrl
        productImages { imageUrl sortOrder }
      }
      categories {
        name
        productCount
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}
```

---

### Endpoint Autenticado: `/graphql/admin`

Requer Bearer Token via header `Authorization`. Retorna `401` se ausente ou inválido.

Todas as queries são filtradas automaticamente pelas lojas vinculadas ao usuário autenticado via `store_users`.

#### Queries disponíveis

| Query | Retorno | Descrição |
|-------|---------|-----------|
| `myStores(skip, take)` | `StoreCollectionSegment` | Todas as lojas do usuário (qualquer status) |
| `myProducts(skip, take)` | `ProductCollectionSegment` | Todos os produtos das lojas do usuário (qualquer status) |
| `myCategories(skip, take)` | `CategoryCollectionSegment` | Todas as categorias das lojas do usuário |
#### Exemplo

```graphql
{
  myStores(skip: 0, take: 10) {
    items {
      storeId
      name
      logoUrl
      products {
        productId
        name
        price
        imageUrl
        status
      }
      categories {
        name
        productCount
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}
```

---

### Tipos GraphQL

Os tipos GraphQL mapeiam diretamente as entidades do banco de dados, com as navigation properties disponíveis para consulta em profundidade:

| Tipo | Campos principais | Campos computados | Relações navegáveis |
|------|-------------------|-------------------|---------------------|
| `Store` | `storeId`, `slug`, `name`, `logo`, `status` | `logoUrl` | `products`, `categories`, `storeUsers`* |
| `Product` | `productId`, `slug`, `name`, `price`, `discount`, `status`, `productType`, `featured`, `description` | `imageUrl` | `store`, `category`, `productImages` |
| `Category` | `categoryId`, `slug`, `name` | `productCount` | `store`, `products` |
| `ProductImage` | `imageId`, `image`, `sortOrder` | `imageUrl` | `product` |
| `StoreUser` | `storeUserId`, `storeId`, `userId` | — | `store` |

> \* `storeUsers` e `ownerId` são **ocultos** no schema público (`/graphql`), visíveis apenas no admin (`/graphql/admin`).

### Filtering e Sorting

Todos os campos escalares suportam filtering e sorting via argumentos gerados automaticamente pelo HotChocolate.

**Exemplo de paginação com filtering:**
```graphql
{
  products(skip: 0, take: 10, where: { price: { gte: 10 }, name: { contains: "premium" } }) {
    items {
      productId
      name
      price
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}
```

**Exemplo de paginação com sorting:**
```graphql
{
  products(skip: 0, take: 10, order: { price: DESC }) {
    items {
      productId
      name
      price
    }
    totalCount
  }
}
```

---

## Referências Externas

### UserInfo (NAuth.DTO)

DTO externo do pacote NAuth, referenciado em `ShopCarInfo.User` e `StoreUserInfo.User`. Contém dados do usuário autenticado (ID, nome, email, etc.).

---

## Resumo

| Recurso | Endpoints REST | DTOs |
|---------|----------------|------|
| **Product** | 3 (insert, update, search) | 9 (inclui enums e params) |
| **Category** | 3 (insert, update, delete) | 3 |
| **Image** | 3 (upload, list, delete) | 1 |
| **Store** | 4 (insert, update, uploadLogo, delete) | 4 (inclui enum) |
| **StoreUser** | 3 (list, insert, delete) | 2 |
| **ShopCar** | 1 (insert) | 2 |
| **GraphQL** | 2 endpoints, 8 queries | 5 tipos + 4 campos computados |
| **Total** | **17 REST + 2 GraphQL** | **25** |

- **Endpoint público REST:** `POST /product/search`
- **Endpoints GraphQL públicos:** `/graphql` (stores, products, categories, storeBySlug, featuredProducts)
- **Endpoints GraphQL autenticados:** `/graphql/admin` (myStores, myProducts, myCategories)
- **Todos os demais endpoints REST requerem Bearer Token**
- **Serialização JSON:** propriedades em `camelCase` via `[JsonPropertyName]`
- **Leituras migradas para GraphQL:** listagem e busca de stores, products e categories agora são feitas exclusivamente via GraphQL, com suporte a paginação offset-based, projection, filtering e sorting
