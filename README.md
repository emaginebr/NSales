# Lofn - Sales & E-Commerce Platform Backend

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)
![HotChocolate](https://img.shields.io/badge/HotChocolate-14.3-E535AB)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791)
![License](https://img.shields.io/badge/License-MIT-green)

## Overview

**Lofn** is a multi-tenant sales and e-commerce platform backend built with **.NET 8** following **Clean Architecture** principles. It provides a **GraphQL API** (via HotChocolate) for read operations and **REST API** for write operations, managing products, categories, stores, and multi-tenant networks with sellers. Supports payment processing via Stripe, file storage with AWS S3, and delegated authentication through NAuth.

The solution is organized into 8 layered projects with clear dependency boundaries, including a dedicated **Lofn.GraphQL** project for all GraphQL schemas, queries, and type extensions.

---

## 🚀 Features

- 🏪 **Multi-Tenant Networks** - Support for multiple seller networks with isolated data via TenantResolver
- 📦 **Product Management** - Full CRUD with slug generation, image handling, discount, product types, and paged search
- 🛒 **Shopping Cart** - ShopCart entity for cart management
- 🏷️ **Category Management** - Categories with automatic product count
- 🔍 **GraphQL API** - HotChocolate-powered queries with projection, filtering, and sorting
- 📡 **REST API** - Write operations with Swagger documentation
- 💳 **Stripe Integration** - Payment processing with Stripe product/price IDs
- 🔐 **NAuth Authentication** - Delegated Bearer token authentication via custom handler
- ☁️ **AWS S3 Storage** - File upload and signed URL resolution
- 🔧 **zTools Integration** - ChatGPT, email sending, file management utilities
- 📄 **Swagger/OpenAPI** - Auto-generated API documentation
- 🐳 **Docker Support** - Docker Compose for development and production
- 🔄 **CI/CD** - GitHub Actions for versioning, releases, NuGet publishing, and deployment

---

## 🛠️ Technologies Used

### Core Framework
- **.NET 8** - Backend runtime and SDK
- **ASP.NET Core** - Web API framework

### GraphQL
- **HotChocolate 14.3** - GraphQL server with projection, filtering, and sorting
- **HotChocolate.Data.EntityFramework** - EF Core integration for IQueryable resolvers

### Database
- **PostgreSQL 17** - Primary relational database
- **Entity Framework Core 9** - ORM with lazy loading proxies
- **Npgsql** - PostgreSQL provider for EF Core

### Security
- **NAuth** - External authentication service integration
- **Custom RemoteAuthHandler** - Bearer token validation middleware

### Payments & Cloud
- **Stripe.net** - Payment gateway integration
- **AWSSDK.S3** - Amazon S3 file storage
- **SixLabors.ImageSharp** - Image processing

### Additional Libraries
- **Newtonsoft.Json** - JSON serialization
- **Swashbuckle** - Swagger/OpenAPI documentation

### Testing
- **xUnit 2.9** - Test framework
- **Moq 4.20** - Mocking library
- **coverlet** - Code coverage

### DevOps
- **Docker** - Containerization with multi-stage builds
- **Docker Compose** - Service orchestration (API + PostgreSQL)
- **GitHub Actions** - CI/CD pipelines (versioning, NuGet publishing, releases, deployment)
- **GitVersion** - Semantic versioning from conventional commits

---

## 📁 Project Structure

```
Lofn/
├── Lofn.API/                    # Web API entry point
│   ├── Controllers/             # REST controllers (Product, Category, Store, Image, StoreUser, ShopCart)
│   ├── Middlewares/             # TenantMiddleware
│   ├── Startup.cs               # DI, auth, CORS, Swagger, GraphQL endpoints
│   └── Dockerfile               # Multi-stage Docker build
├── Lofn.GraphQL/                # GraphQL schemas and resolvers
│   ├── Public/                  # Public queries (stores, products, categories)
│   ├── Admin/                   # Authenticated queries (myStores, myProducts, myCategories)
│   ├── Types/                   # Type extensions (logoUrl, imageUrl, productCount)
│   ├── GraphQLServiceExtensions.cs  # Schema registration
│   └── GraphQLErrorLogger.cs    # Error diagnostics
├── Lofn.Application/            # DI bootstrap (ConfigureLofn)
├── Lofn.Domain/                 # Business logic layer
│   ├── Interfaces/              # Service contracts
│   ├── Models/                  # Domain models
│   ├── Services/                # Service implementations
│   └── Mappers/                 # Model ↔ DTO mappers
├── Lofn/                        # Shared package (DTOs + ACL)
│   ├── ACL/                     # Anti-Corruption Layer (external API clients)
│   └── DTO/                     # Data Transfer Objects (Product, Category, Store, ShopCart)
├── Lofn.Infra.Interfaces/      # Repository interfaces (IUnitOfWork, IRepository)
├── Lofn.Infra/                  # Infrastructure implementation
│   ├── Context/                 # EF Core DbContext + entities
│   └── Repository/              # Repository implementations
├── Lofn.Tests/                  # Unit tests (xUnit + Moq)
├── bruno-collection/            # Bruno API testing collection
├── scripts/                     # Seed scripts (info-store.py)
├── docs/                        # Documentation
├── docker-compose.yml           # Development (API + PostgreSQL)
├── docker-compose-prod.yml      # Production
├── lofn.sql                     # Database creation script
├── .github/workflows/           # CI/CD (versioning, NuGet, releases, deploy)
├── GitVersion.yml               # Semantic versioning config
├── Lofn.sln                     # Solution file
└── README.md                    # This file
```

---

## 🏗️ System Design

The following diagram illustrates the high-level architecture of **Lofn**:

![System Design](docs/system-design.png)

**Dependency flow:** `API / GraphQL → Application → Domain → Infra → PostgreSQL`

- **Lofn.API** receives REST requests and delegates to Domain services
- **Lofn.GraphQL** handles GraphQL queries directly against EF Core DbContext with type extensions for computed fields
- **Lofn.Application** bootstraps all DI registrations via `ConfigureLofn()` and manages multi-tenant context
- **Lofn.Domain** contains business rules, service implementations, and domain models
- **Lofn (Shared)** provides DTOs and ACL clients for external consumers
- **Lofn.Infra** implements repositories using EF Core 9 with PostgreSQL

> 📄 **Source:** The editable Mermaid source is available at [`docs/system-design.mmd`](docs/system-design.mmd).

---

## 📖 Additional Documentation

| Document | Description |
|----------|-------------|
| [API_REFERENCE.md](docs/API_REFERENCE.md) | Complete REST and GraphQL API reference with all endpoints, DTOs, enums, and examples |

---

## ⚙️ Environment Configuration

Before running the application, configure the environment variables:

### 1. Copy the environment template

```bash
cp .env.example .env
```

### 2. Edit the `.env` file

```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password_here
POSTGRES_DB=lofn

# Tenant: monexup
MONEXUP_CONNECTION_STRING=Host=db;Port=5432;Database=lofn;Username=postgres;Password=your_password_here
MONEXUP_JWT_SECRET=dev-jwt-secret-min-32-chars-long-here

# Lofn
LOFN_BUCKET_NAME=lofn

# NAuth
NAUTH_API_URL=https://your-nauth-url/auth-api
NAUTH_BUCKET_NAME=nauth

# zTools
ZTOOLS_API_URL=https://your-ztools-url/tools-api

# App
APP_PORT=5000
```

⚠️ **IMPORTANT**:
- Never commit the `.env` file with real credentials
- Only `.env.example` and `.env.prod.example` should be version controlled
- Change all default passwords and secrets before deployment

---

## 🐳 Docker Setup

### Quick Start with Docker Compose

#### 1. Prerequisites

```bash
# Create the external Docker network (shared with other services)
docker network create emagine-network
```

#### 2. Build and Start Services

```bash
docker-compose up -d --build
```

This starts:
- **lofn-api** - .NET 8 API on port 5000 (configurable via `APP_PORT`)
- **lofn-db** - PostgreSQL 17 on port 5432

#### 3. Verify Deployment

```bash
docker-compose ps
docker-compose logs -f api
```

### Accessing the Application

| Service | URL |
|---------|-----|
| **API** | `http://localhost:5000` |
| **Swagger UI** | `http://localhost:5000/swagger/ui` |
| **GraphQL Playground** | `http://localhost:5000/graphql` |
| **GraphQL Admin** | `http://localhost:5000/graphql/admin` |
| **Health Check** | `http://localhost:5000/` |

### Docker Compose Commands

| Action | Command |
|--------|---------|
| Start services | `docker-compose up -d` |
| Start with rebuild | `docker-compose up -d --build` |
| Stop services | `docker-compose stop` |
| View status | `docker-compose ps` |
| View logs | `docker-compose logs -f` |
| Remove containers | `docker-compose down` |
| Remove containers and volumes | `docker-compose down -v` |

### Production Deployment

```bash
docker-compose -f docker-compose-prod.yml up -d --build
```

---

## 🔧 Manual Setup (Without Docker)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 17+](https://www.postgresql.org/download/)

### Setup Steps

#### 1. Clone the repository

```bash
git clone https://github.com/emaginebr/Lofn.git
cd Lofn
```

#### 2. Create the database

```bash
psql -U postgres -c "CREATE DATABASE lofn;"
psql -U postgres -d lofn -f lofn.sql
```

#### 3. Build the solution

```bash
dotnet build Lofn.sln
```

#### 4. Run the API

```bash
dotnet run --project Lofn.API
```

The API will be available at `https://localhost:44374`.

---

## 🧪 Testing

### Running Tests

**All Tests:**
```bash
dotnet test Lofn.sln
```

**With Coverage:**
```bash
dotnet test Lofn.sln --collect:"XPlat Code Coverage"
```

### Test Structure

```
Lofn.Tests/
├── Domain/
│   ├── Mappers/         # DTO ↔ Model mapping tests
│   │   ├── CategoryMapperTest.cs
│   │   ├── ProductMapperTest.cs
│   │   └── StoreMapperTest.cs
│   └── Services/        # Business logic tests
│       ├── CategoryServiceTest.cs
│       ├── ProductServiceTest.cs
│       ├── StoreServiceTest.cs
│       └── StoreUserServiceTest.cs
```

---

## 📚 API Documentation

### Authentication Flow

```
1. Client sends Bearer token → 2. RemoteAuthHandler validates via NAuth → 3. TenantMiddleware resolves tenant → 4. Request processed
```

### GraphQL Endpoints (Read Operations)

| Endpoint | Auth | Queries |
|----------|------|---------|
| `POST /graphql` | No | `stores`, `products`, `categories`, `storeBySlug`, `featuredProducts` |
| `POST /graphql/admin` | Yes | `myStores`, `myProducts`, `myCategories` |

**Example query:**
```graphql
{
  stores {
    storeId
    name
    logoUrl
    products {
      name
      price
      discount
      imageUrl
      productImages { imageUrl sortOrder }
    }
    categories {
      name
      productCount
    }
  }
}
```

### REST Endpoints (Write Operations)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/product/{storeSlug}/insert` | Create product | Yes |
| POST | `/product/{storeSlug}/update` | Update product | Yes |
| POST | `/product/search` | Search products (paged) | No |
| POST | `/category/{storeSlug}/insert` | Create category | Yes |
| POST | `/category/{storeSlug}/update` | Update category | Yes |
| DELETE | `/category/{storeSlug}/delete/{id}` | Delete category | Yes |
| POST | `/store/insert` | Create store | Yes |
| POST | `/store/update` | Update store | Yes |
| POST | `/store/uploadLogo/{storeId}` | Upload logo | Yes |
| DELETE | `/store/delete/{storeId}` | Delete store | Yes |
| POST | `/image/upload/{productId}` | Upload image | Yes |
| GET | `/image/list/{productId}` | List images | Yes |
| DELETE | `/image/delete/{imageId}` | Delete image | Yes |
| POST | `/shopcar/insert` | Create shopping cart | Yes |
| GET | `/storeuser/{storeSlug}/list` | List store members | Yes |
| POST | `/storeuser/{storeSlug}/insert` | Add store member | Yes |
| DELETE | `/storeuser/{storeSlug}/delete/{id}` | Remove store member | Yes |

> Full interactive documentation available at `/swagger/ui` when running the API.

> Complete API reference with DTOs, enums, and examples at [`docs/API_REFERENCE.md`](docs/API_REFERENCE.md).

---

## 🔒 Security Features

### Authentication
- **Bearer Token** - All protected endpoints require a valid Bearer token
- **Remote Validation** - Tokens are validated against the NAuth external service
- **Session Management** - User context (userId, session) maintained per request

### Infrastructure
- **Multi-Tenant Isolation** - Tenant-specific database connections and JWT secrets
- **CORS** - Configurable cross-origin resource sharing
- **Request Size Limits** - 100MB limit for file upload endpoints

---

## 💾 Database

### Schema Creation

```bash
psql -U postgres -d lofn -f lofn.sql
```

### Backup

```bash
pg_dump -U postgres lofn > backup_lofn_$(date +%Y%m%d).sql
```

### Restore

```bash
psql -U postgres -d lofn < backup_lofn_20260319.sql
```

---

## 🔄 CI/CD

### GitHub Actions

| Workflow | Trigger | Description |
|----------|---------|-------------|
| **Version & Tag** | Push to `main` | Creates semantic version tags using GitVersion |
| **Create Release** | After version tag | Creates GitHub releases for minor/major versions |
| **Publish NuGet** | After version tag | Builds and publishes the Lofn NuGet package |
| **Deploy Prod** | Manual dispatch | Deploys to production via SSH |

**Versioning strategy** (GitVersion - ContinuousDelivery):
- `feat:` / `feature:` → Minor version bump
- `fix:` / `patch:` → Patch version bump
- `breaking:` / `major:` → Major version bump

---

## 🧩 Seed Data

A Python script is available to populate a demo store with products and AI-generated images:

```bash
cd scripts
pip install requests python-dotenv openai
python info-store.py
```

The script creates a "Loja de Informatica" with 30 products across 6 categories (Notebooks, Processors, RAM, Storage, Cases, Keyboards, Mice), including discount and featured flags.

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Setup

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Run tests (`dotnet test Lofn.sln`)
5. Commit your changes using conventional commits (`git commit -m 'feat: add some AmazingFeature'`)
6. Push to the branch (`git push origin feature/AmazingFeature`)
7. Open a Pull Request

### Coding Standards

- Follow Clean Architecture dependency rules
- Use conventional commits for semantic versioning
- All new endpoints must include authorization where appropriate
- Repository pattern for all data access
- Read operations via GraphQL, write operations via REST

---

## 👨‍💻 Author

Developed by **[Rodrigo Landim Carneiro](https://github.com/landim32)**

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- GraphQL powered by [HotChocolate](https://chillicream.com/docs/hotchocolate)
- Database powered by [PostgreSQL](https://www.postgresql.org/)
- ORM by [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- Payments by [Stripe](https://stripe.com/)
- Image processing by [SixLabors ImageSharp](https://sixlabors.com/products/imagesharp/)

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/emaginebr/Lofn/issues)
- **Discussions**: [GitHub Discussions](https://github.com/emaginebr/Lofn/discussions)

---

**⭐ If you find this project useful, please consider giving it a star!**
