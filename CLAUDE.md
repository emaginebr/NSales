# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Lofn is a full-stack sales/e-commerce platform with a .NET 8 backend API and a React 18 TypeScript frontend. It supports multi-tenant networks with sellers, products, orders, and invoices.

## Commands

### Frontend (React/TypeScript - root directory)
```bash
npm start          # Dev server at https://localhost:443
npm run build      # Production build to build/
npm test           # Jest tests in watch mode
```

### Backend (.NET 8 - from repo root)
```bash
dotnet build Lofn.sln                          # Build entire solution
dotnet run --project Lofn.API                  # Run API (https://localhost:44374)
dotnet run --project Lofn.BackgroundService    # Run background jobs
```

### Docker
```bash
docker-compose up        # Starts nginx-proxy (8081) + API services
```

## Architecture

### Backend - Clean Architecture (.NET 8)

```
Lofn.API              → Controllers, Startup/DI config, auth middleware
Lofn.GraphQL          → HotChocolate GraphQL schemas, queries, type extensions
Lofn.Application      → DI bootstrap (Startup.cs), wires up services via ConfigureLofn()
Lofn.Domain           → Business logic: Models/, Services/, Core/, Interfaces/
Lofn.DTO              → Data transfer objects shared across layers
Lofn.BackgroundService → Scheduled background jobs
Lofn.ACL              → Anti-corruption layer (external API adapters)
Lofn.Infra.Interfaces → Infrastructure abstractions (IUnitOfWork, Repository interfaces)
Lofn.Infra            → EF Core 9 DbContext (LofnContext), repositories, Unit of Work
Lib/                    → External DLLs: NAuth.ACL, NAuth.DTO, NTools.ACL, NTools.DTO
```

**Dependency flow:** API → GraphQL / Application → Domain → Lofn.Infra → PostgreSQL (Npgsql)

**Key patterns:**
- Repository + Unit of Work (Lofn.Infra)
- EF Core with lazy loading proxies
- Custom `RemoteAuthHandler` for Bearer token auth (delegates to NAuth)
- DI registration centralized in `Lofn.Application/Startup.cs` via `ConfigureLofn()` extension method

### GraphQL - HotChocolate (Lofn.GraphQL)

```
GraphQLServiceExtensions.cs → DI registration (AddLofnGraphQL), configures both schemas
GraphQLErrorLogger.cs       → Diagnostic event listener for logging GraphQL errors
Public/PublicQuery.cs       → Public queries (stores, products, categories, featuredProducts)
Public/PublicStoreType.cs   → ObjectType<Store> hiding internal fields (OwnerId, StoreUsers, Orders)
Admin/AdminQuery.cs         → Authenticated queries (myStores, myProducts, myCategories, myOrders)
Types/                      → ObjectType extensions adding computed fields via field resolvers
```

**Endpoints:**
- `POST /graphql` — public schema (anonymous)
- `POST /graphql/admin` — admin schema (requires Bearer token)
- Both endpoints expose interactive Banana Cake Pop playground

**Type extensions (computed fields):**
- `StoreTypeExtension` → `logoUrl` (resolves via IFileClient)
- `ProductTypeExtension` → `imageUrl` (resolves via IFileClient)
- `ProductImageTypeExtension` → `imageUrl` (resolves via IFileClient)
- `CategoryTypeExtension` → `productCount` (counts active products via navigation property), `isGlobal` (true when `StoreId` is null)

**Key patterns:**
- Queries return `IQueryable<Entity>` directly from EF Core DbContext (no DTOs)
- `[UseProjection]`, `[UseFiltering]`, `[UseSorting]` for HotChocolate optimizations
- `[ExtendObjectType]` for adding computed fields without modifying entities

### Frontend - Layered React (src/)

```
Pages/        → Route-level components (HomePage, DashboardPage, ProductPage, etc.)
Components/   → Reusable UI (Menu, MessageToast, EditMode, ImageModal, etc.)
Contexts/     → React Context providers per domain (Auth, Product, Order, Network, etc.)
Business/     → Business logic layer with interfaces + implementations
Services/     → API communication layer (Axios-based)
Infra/        → HttpClient wrapper (Axios) with interface
DTO/          → TypeScript types: Domain models, API responses, enums
lib/nauth-core/ → Auth library
```

**Data flow:** Page → Context/Provider → Business → Service → HttpClient (Axios) → Backend API

**Key libraries:** MUI + Bootstrap (UI), React Router 6, Axios, Stripe, Web3, i18next, React DnD, Craft.js

### API Endpoints (Backend)

**GraphQL (read operations):**
- `POST /graphql` — public schema (anonymous): stores, products, categories, storeBySlug, featuredProducts
- `POST /graphql/admin` — admin schema (authenticated): myStores, myProducts, myCategories, myOrders

**REST — Store** (`/store`):
- `POST /store/insert` — [Authorize] create store
- `POST /store/update` — [Authorize] update store
- `POST /store/uploadLogo/{storeId}` — [Authorize] upload logo (100MB limit)
- `DELETE /store/delete/{storeId}` — [Authorize] delete store

**REST — Product** (`/product`):
- `POST /product/{storeSlug}/insert` — [Authorize] create product
- `POST /product/{storeSlug}/update` — [Authorize] update product
- `POST /product/search` — public product search with pagination

**REST — Category** (`/category`) — store-scoped, available only when tenant `Marketplace = false`:
- `POST /category/{storeSlug}/insert` — [Authorize] create category (returns 403 if `Marketplace = true`)
- `POST /category/{storeSlug}/update` — [Authorize] update category (returns 403 if `Marketplace = true`)
- `DELETE /category/{storeSlug}/delete/{categoryId}` — [Authorize] delete category (returns 403 if `Marketplace = true`)

**REST — CategoryGlobal** (`/category-global`) — tenant-global catalog, requires `IsAdmin = true` AND tenant `Marketplace = true`:
- `POST /category-global/insert` — [Authorize][MarketplaceAdmin] create global category
- `POST /category-global/update` — [Authorize][MarketplaceAdmin] update global category
- `DELETE /category-global/delete/{categoryId}` — [Authorize][MarketplaceAdmin] delete global category
- `GET /category-global/list` — [Authorize][MarketplaceAdmin] list global categories

**REST — Order** (`/order`):
- `POST /order/update` — [Authorize] update order
- `POST /order/search` — [Authorize] search orders with pagination
- `POST /order/list` — [Authorize] list orders by store/user/status
- `GET /order/getById/{orderId}` — [Authorize] get order by ID

**REST — Image** (`/image`):
- `POST /image/upload/{productId}` — [Authorize] upload product image (100MB limit)
- `GET /image/list/{productId}` — [Authorize] list images for product
- `DELETE /image/delete/{imageId}` — [Authorize] delete image

**REST — StoreUser** (`/storeuser`):
- `GET /storeuser/{storeSlug}/list` — [Authorize] list store members
- `POST /storeuser/{storeSlug}/insert` — [Authorize] add user to store
- `DELETE /storeuser/{storeSlug}/delete/{storeUserId}` — [Authorize] remove user from store

**Other:**
- `GET /` — health check
- `/swagger/ui` — Swagger UI (dev/docker only)

### Routing (App.tsx)
- Public: `/`, `/network`, `/account/login`, `/:networkSlug`, `/@/:sellerSlug`
- Admin (protected): `/admin/dashboard`, `/admin/products`, `/admin/teams`, `/admin/team-structure`

## Environment

- **Dev API URL:** `https://localhost:44374` (from `.env`)
- **Prod API URL:** configured in `.env.production`
- **Database:** PostgreSQL
- **TypeScript:** strict mode enabled, strictNullChecks disabled

## Conventions

- Frontend uses `.tsx` extension for all files including services and DTOs (not just components)
- Each domain concept (Product, Order, Network, User, Invoice, Profile, Template) follows the same layered pattern: Context → Business → Service
- Business/Service layers use interface + implementation pattern with factories (e.g., `ProductFactory.tsx` creates `ProductBusiness`)
- Context providers are composed via `ContextBuilder.tsx`
