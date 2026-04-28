# REST contract — `CategoryGlobalController`

**Path prefix**: `/category-global`
**Target framework**: ASP.NET Core (`Microsoft.AspNetCore.Mvc`), `[ApiController]`
**Authentication**: NAuth Bearer (`[Authorize]`)
**Authorization**: must satisfy `MarketplaceAdminRequirement` (custom action filter requiring `userSession.IsAdmin == true` AND `tenantResolver.Marketplace == true`)
**Status if either rule fails**:
- Anonymous → `401 Unauthorized` (existing `[Authorize]` behaviour)
- Authenticated but not admin → `403 Forbidden`
- Authenticated admin but tenant `Marketplace = false` → `403 Forbidden`

---

## `POST /category-global/insert`

Creates a tenant-global category. Slug is generated from `name` and uniquified across the tenant DB.

Request body — `CategoryGlobalInsertInfo`:

```json
{ "name": "Periféricos" }
```

Validation (FluentValidation `CategoryGlobalInsertInfoValidator`):
- `name` required, length 1..120

Response `200 OK` — `CategoryInfo` (with `IsGlobal = true`):

```json
{
  "categoryId": 42,
  "slug": "perifericos",
  "name": "Periféricos",
  "storeId": null,
  "isGlobal": true,
  "productCount": 0
}
```

Errors:
- `400 Bad Request` — validation failure (returns `{ "success": false, "errors": ["..."] }`, same shape as `GlobalExceptionFilter` produces today)
- `401`, `403` — see top of file
- `500 Internal Server Error` — unexpected (logged via Serilog `GlobalExceptionFilter`)

---

## `POST /category-global/update`

Updates an existing global category's `name` (regenerates `slug`).

Request body — `CategoryGlobalUpdateInfo`:

```json
{ "categoryId": 42, "name": "Periféricos & Acessórios" }
```

Validation:
- `categoryId` > 0
- `name` required, length 1..120
- Targeted row MUST have `StoreId IS NULL` — otherwise `400 Bad Request` with message `Category {id} is not global`

Response `200 OK` — `CategoryInfo` reflecting the update.

Errors: `400` / `401` / `403` / `404` (when `categoryId` doesn't exist) / `500`.

---

## `DELETE /category-global/delete/{categoryId}`

Hard-deletes a global category. Products previously associated have their `CategoryId` set to `NULL` automatically (existing EF behaviour, `OnDelete = ClientSetNull`). FR-011 calls for a warning surface; the warning is **client-side responsibility** — the API performs the deletion when invoked.

Path parameter:
- `categoryId` (long, > 0)

Response `204 No Content` on success.

Errors:
- `401` / `403` (auth gates)
- `404 Not Found` if the category doesn't exist OR exists but is store-scoped (returning `404` for "not found within global namespace" keeps the surface clean)

---

## `GET /category-global/list`

Returns the tenant's global catalog. Authenticated and authorised callers only — same gate as the rest of this controller. (Public anonymous read of globals is served by the GraphQL public schema instead — see `graphql-schema.md`.)

Response `200 OK` — `IList<CategoryInfo>` ordered by `name` ascending.

```json
[
  { "categoryId": 42, "slug": "perifericos", "name": "Periféricos", "storeId": null, "isGlobal": true, "productCount": 12 },
  { "categoryId": 43, "slug": "monitores",  "name": "Monitores",  "storeId": null, "isGlobal": true, "productCount":  3 }
]
```

---

## DTO additions

`Lofn/DTO/Category/CategoryInfo.cs` — add:

```csharp
[JsonPropertyName("storeId")]
public long? StoreId { get; set; }

[JsonPropertyName("isGlobal")]
public bool IsGlobal { get; set; }   // computed: StoreId == null
```

`Lofn/DTO/Category/CategoryGlobalInsertInfo.cs` — new:

```csharp
public class CategoryGlobalInsertInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
```

`Lofn/DTO/Category/CategoryGlobalUpdateInfo.cs` — new:

```csharp
public class CategoryGlobalUpdateInfo
{
    [JsonPropertyName("categoryId")]
    public long CategoryId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
```
