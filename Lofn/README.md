# Lofn - Client SDK for the Lofn Sales Platform API

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![NuGet](https://img.shields.io/badge/NuGet-Package-purple)
![License](https://img.shields.io/badge/License-Proprietary-green)

## Overview

**Lofn** is a .NET 8 client SDK (Anti-Corruption Layer) that provides typed HTTP clients and DTOs for consuming the Lofn Sales Platform REST API. It allows external applications to integrate with the Lofn backend for managing **stores**, **products**, **categories**, **images**, **store users**, and **shopping carts** without coupling to internal API details.

Part of the **Lofn ecosystem** — this package is consumed by frontend applications and other services that need to interact with the Lofn API.

---

## 🚀 Features

- 🛒 **Product Management** — Search, list, create, and update products with pagination support
- 🏪 **Store Operations** — Full CRUD for stores with slug-based lookups
- 📂 **Category Management** — Create, update, delete, and list categories per store
- 🖼️ **Image Upload** — Stream-based image upload with sort order support
- 👥 **Store User Management** — Add, list, and remove store team members
- 🛍️ **Shopping Cart** — Create and manage shopping carts
- 🔐 **Multi-Tenant Support** — Automatic `X-Tenant-Id` header propagation via `TenantHeaderHandler`
- 📦 **Typed DTOs** — Strongly-typed request/response objects with enums for status and product types

---

## 🛠️ Technologies Used

### Core Framework
- **.NET 8.0** — Target framework

### Dependencies
- **NAuth 0.5.5** — Authentication integration
- **Newtonsoft.Json 13.0.3** — JSON serialization/deserialization
- **Microsoft.AspNetCore.Authentication 2.3.0** — Authentication abstractions
- **Microsoft.Extensions.Configuration.Abstractions 9.0.8** — Configuration support

---

## 📁 Project Structure

```
Lofn/
├── ACL/                        # Anti-Corruption Layer (HTTP clients)
│   ├── Core/
│   │   └── BaseClient.cs       # Abstract base with HttpClient setup
│   ├── Handlers/
│   │   └── TenantHeaderHandler.cs  # DelegatingHandler for X-Tenant-Id
│   ├── Interfaces/
│   │   ├── ICategoryClient.cs  # Category operations contract
│   │   ├── IImageClient.cs     # Image operations contract
│   │   ├── IProductClient.cs   # Product operations contract
│   │   ├── IStoreClient.cs     # Store operations contract
│   │   └── IStoreUserClient.cs # Store user operations contract
│   ├── CategoryClient.cs       # Category API client
│   ├── ImageClient.cs          # Image API client
│   ├── ProductClient.cs        # Product API client
│   ├── StoreClient.cs          # Store API client
│   └── StoreUserClient.cs      # Store user API client
├── DTO/                        # Data Transfer Objects
│   ├── Category/
│   │   ├── CategoryInfo.cs
│   │   ├── CategoryInsertInfo.cs
│   │   └── CategoryUpdateInfo.cs
│   ├── Product/
│   │   ├── ProductInfo.cs
│   │   ├── ProductInsertInfo.cs
│   │   ├── ProductUpdateInfo.cs
│   │   ├── ProductImageInfo.cs
│   │   ├── ProductListPagedInfo.cs
│   │   ├── ProductListPagedResult.cs
│   │   ├── ProductSearchParam.cs
│   │   ├── ProductSearchInternalParam.cs
│   │   ├── ProductStatusEnum.cs
│   │   └── ProductTypeEnum.cs
│   ├── Settings/
│   │   └── LofnSetting.cs     # API URL + Bucket configuration
│   ├── ShopCart/
│   │   └── ShopCartInfo.cs
│   └── Store/
│       ├── StoreInfo.cs
│       ├── StoreInsertInfo.cs
│       ├── StoreUpdateInfo.cs
│       ├── StoreStatusEnum.cs
│       ├── StoreUserInfo.cs
│       └── StoreUserInsertInfo.cs
├── Lofn.csproj
└── README.md
```

### Ecosystem

| Project | Type | Description |
|---------|------|-------------|
| **Lofn.API** | Backend | REST API + GraphQL endpoints |
| **Lofn.Domain** | Backend | Business logic and domain services |
| **Lofn.Infra** | Backend | EF Core repositories and database context |
| **Lofn** | NuGet Package | Client SDK (this package) |

---

## 📦 Installation

### NuGet Package

```bash
dotnet add package Lofn
```

### Build from Source

```bash
dotnet build Lofn.csproj
```

---

## ⚙️ Configuration

Add the Lofn settings to your `appsettings.json`:

```json
{
  "LofnSetting": {
    "ApiUrl": "https://your-lofn-api-url",
    "BucketName": "your-bucket-name"
  },
  "Tenant": {
    "DefaultTenantId": "your-tenant-id"
  }
}
```

Register in your DI container:

```csharp
services.Configure<LofnSetting>(configuration.GetSection("LofnSetting"));

// Register clients
services.AddScoped<IProductClient, ProductClient>();
services.AddScoped<IStoreClient, StoreClient>();
services.AddScoped<ICategoryClient, CategoryClient>();
services.AddScoped<IImageClient, ImageClient>();
services.AddScoped<IStoreUserClient, StoreUserClient>();

// Optional: register TenantHeaderHandler for automatic tenant propagation
services.AddTransient<TenantHeaderHandler>();
```

---

## 📚 Usage

### Product Client

```csharp
// Search products with pagination
var results = await productClient.SearchAsync(new ProductSearchParam
{
    Keyword = "premium",
    StoreId = 1,
    OnlyActive = true,
    PageNum = 1
});

// Get product by slug
var product = await productClient.GetBySlugAsync("my-product");

// Create a product
var newProduct = await productClient.InsertAsync("my-store", new ProductInsertInfo
{
    Name = "New Product",
    Price = 29.90,
    Status = ProductStatusEnum.Active,
    ProductType = ProductTypeEnum.Physical
});

// List featured products
var featured = await productClient.ListFeaturedAsync("my-store", limit: 5);
```

### Store Client

```csharp
// List active stores
var stores = await storeClient.ListActiveAsync();

// Get store by slug
var store = await storeClient.GetBySlugAsync("my-store");

// Create a store
var newStore = await storeClient.InsertAsync(new StoreInsertInfo
{
    Name = "My Store"
});
```

### Category Client

```csharp
// List categories for a store
var categories = await categoryClient.ListActiveAsync("my-store");

// Create a category
var category = await categoryClient.InsertAsync("my-store", new CategoryInsertInfo
{
    Name = "Electronics"
});
```

### Image Client

```csharp
// Upload product image
using var stream = File.OpenRead("photo.jpg");
var image = await imageClient.UploadAsync(productId: 1, stream, "photo.jpg", sortOrder: 0);

// List images for a product
var images = await imageClient.ListAsync(productId: 1);

// Delete an image
await imageClient.DeleteAsync(imageId: 5);
```

---

## 📋 Available Clients

| Client | Interface | Operations |
|--------|-----------|------------|
| `ProductClient` | `IProductClient` | Search, GetById, GetBySlug, ListActive, ListFeatured, Insert, Update |
| `StoreClient` | `IStoreClient` | List, ListActive, GetBySlug, GetById, Insert, Update, Delete |
| `CategoryClient` | `ICategoryClient` | List, ListActive, GetBySlug, GetById, Insert, Update, Delete |
| `ImageClient` | `IImageClient` | List, Upload (stream), Delete |
| `StoreUserClient` | `IStoreUserClient` | List, Insert, Delete |

---

## 📊 DTOs Reference

### Enums

| Enum | Values |
|------|--------|
| `ProductStatusEnum` | `Active (1)`, `Inactive (2)`, `Expired (3)` |
| `ProductTypeEnum` | `Physical (1)`, `InfoProduct (2)` |
| `StoreStatusEnum` | `Inactive (0)`, `Active (1)`, `Suspended (2)` |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
5. Push to the branch (`git push origin feature/AmazingFeature`)
6. Open a Pull Request

---

## 👨‍💻 Author

Developed by **[emagine](https://github.com/emaginebr)**

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/emaginebr/Lofn/issues)

---

**⭐ If you find this project useful, please consider giving it a star!**
