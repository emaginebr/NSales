# GraphQL contract — schema deltas

**Schemas**: public (`POST /graphql`), admin (`POST /graphql/admin`)
**Stack**: HotChocolate 14.3

The schema **shape** does not change. Only the resolvers' behaviour branches on `ITenantResolver.Marketplace`.

## Public schema (`/graphql`)

### Query — `categories(skip, take)`

Today: `IQueryable<Category>` `where Categories.Any(p => p.Status == 1)` (filtered to those that have at least one active product), with `[UseProjection][UseFiltering][UseSorting]`.

After the feature:

```csharp
public IQueryable<Category> GetCategories(LofnContext context, [Service] ITenantResolver tenantResolver)
    => tenantResolver.Marketplace
        ? context.Categories.Where(c => c.StoreId == null && c.Products.Any(p => p.Status == 1))
        : context.Categories.Where(c => c.Products.Any(p => p.Status == 1));
```

- Marketplace tenant: only globals (`StoreId IS NULL`) reach the response.
- Non-marketplace tenant: unchanged behaviour.

### Type extension — `CategoryTypeExtension.GetProductCount`

Already isolated per-call DbContext in PR #001-marketplace-categories scope (already shipped as part of the `dotnet-graphql` concurrency fix). No further change required for this feature.

A **new** computed field is added to the `Category` type via `CategoryTypeExtension`:

```csharp
[ExtendObjectType(typeof(Category))]
public class CategoryTypeExtension
{
    public bool GetIsGlobal([Parent] Category category) => category.StoreId is null;
    // ... existing GetProductCount stays
}
```

This exposes `category.isGlobal: Boolean!` on the GraphQL surface, matching the new `CategoryInfo.IsGlobal` REST field.

## Admin schema (`/graphql/admin`)

### Query — `myCategories`

Today: returns categories owned by stores the current user owns.

After the feature, the resolver branches:

- Marketplace tenant: returns globals only (`StoreId IS NULL`). The admin can navigate `category.products { ... }` and the projection will correctly walk into store-scoped products.
- Non-marketplace tenant: unchanged.

```csharp
public IQueryable<Category> GetMyCategories(
    LofnContext context,
    [Service] ITenantResolver tenantResolver,
    [Service] IHttpContextAccessor httpContextAccessor,
    [Service] IUserClient userClient)
{
    if (tenantResolver.Marketplace)
        return context.Categories.Where(c => c.StoreId == null);

    var userStoreIds = GetUserStoreIds(context, httpContextAccessor, userClient);
    return context.Categories.Where(c => c.StoreId.HasValue && userStoreIds.Contains(c.StoreId.Value));
}
```

`GetUserStoreIds` already exists in `AdminQuery`.

## Public storefront example query (after feature)

```graphql
{
  stores(skip: 0, take: 10) {
    items {
      storeId
      name
      products {
        productId
        name
        category { categoryId name isGlobal slug }
      }
    }
    totalCount
  }
}
```

In a marketplace tenant, `category` (when not null) always has `isGlobal: true`. In a non-marketplace tenant, `isGlobal` is always `false`.

## Schema versioning

This is an additive change (new field `isGlobal`, no removed/renamed fields). No deprecation cycle is required.
