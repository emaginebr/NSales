# REST contract — `ProductController` — validation deltas

**Path prefix**: `/product`
**Authentication**: NAuth Bearer (unchanged for `[Authorize]` actions; `/product/search` continues to be anonymous)

The controller's surface is **unchanged**. The deltas are entirely in the service layer (`ProductService.InsertAsync`, `ProductService.UpdateAsync`); they manifest at the HTTP level as new `400 Bad Request` outcomes.

## New validation outcomes

### `POST /product/{storeSlug}/insert`

When `tenantResolver.Marketplace == true`:
- If the request `categoryId` is non-null and resolves to a category whose `StoreId IS NOT NULL` → **`400 Bad Request`** with body `{ "success": false, "errors": ["CategoryId must reference a tenant-global category in marketplace mode"] }`.
- If the request `categoryId` is non-null but does not resolve to any category → `400` with `Category {id} not found`.
- If the request `categoryId` is `null` → succeeds (today's behaviour preserved).

When `tenantResolver.Marketplace == false`:
- If the request `categoryId` is non-null and resolves to a category whose `StoreId != product.StoreId` → **`400 Bad Request`** with body `{ "success": false, "errors": ["CategoryId does not belong to this store"] }`. *(Today this case may silently succeed; the new validation makes it explicit and consistent with the marketplace check.)*
- If the request `categoryId` is `null` → succeeds.

### `POST /product/{storeSlug}/update`

Same rules as `insert`, applied to the `categoryId` of the update payload.

### `POST /product/search` (anonymous)

No change. Search ignores marketplace mode.

## Service-layer signature change

`Lofn.Domain.Services.ProductService` adds a private helper:

```csharp
private async Task AssertCategoryAllowedAsync(long? categoryId, long storeId)
{
    if (categoryId is null) return;
    var cat = await _categoryRepository.GetByIdAsync(categoryId.Value)
        ?? throw new ValidationException($"Category {categoryId} not found");

    if (_tenantResolver.Marketplace)
    {
        if (cat.StoreId != null)
            throw new ValidationException("CategoryId must reference a tenant-global category in marketplace mode");
    }
    else
    {
        if (cat.StoreId != storeId)
            throw new ValidationException("CategoryId does not belong to this store");
    }
}
```

The service constructor gains `ITenantResolver` (already in the DI graph) and `ICategoryRepository<CategoryModel>` (already used elsewhere). Both are existing DI registrations — no `Application/Startup.cs` change required for this helper.

The `ValidationException` thrown is the FluentValidation type already converted by `GlobalExceptionFilter` into a `400 Bad Request` with the standard error-list body shape.
