using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Category;
using Lofn.Infra.Context;

namespace Lofn.GraphQL.Public;

public class PublicQuery
{
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Store> GetStores(LofnContext context)
        => context.Stores.Where(s => s.Status == 1);

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts(LofnContext context)
        => context.Products.Where(p => p.Status == 1);

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Category> GetCategories(LofnContext context, [Service] ITenantResolver tenantResolver)
        => tenantResolver.Marketplace
            ? context.Categories.Where(c => c.StoreId == null && c.Products.Any(p => p.Status == 1))
            : context.Categories.Where(c => c.Products.Any(p => p.Status == 1));

    [UseProjection]
    public IQueryable<Store> GetStoreBySlug(LofnContext context, string slug)
        => context.Stores.Where(s => s.Status == 1 && s.Slug == slug);

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetFeaturedProducts(LofnContext context, string storeSlug)
        => context.Products.Where(p => p.Status == 1 && p.Featured && p.Store.Slug == storeSlug);

    public async Task<IList<CategoryTreeNodeInfo>> GetCategoryTree(
        LofnContext context,
        [Service] ITenantResolver tenantResolver,
        [Service] ICategoryService categoryService,
        string storeSlug = null)
    {
        if (tenantResolver.Marketplace)
            return await categoryService.GetTreeAsync(null);

        if (string.IsNullOrWhiteSpace(storeSlug))
            return new List<CategoryTreeNodeInfo>();

        var store = context.Stores.FirstOrDefault(s => s.Slug == storeSlug && s.Status == 1);
        if (store == null)
            return new List<CategoryTreeNodeInfo>();

        return await categoryService.GetTreeAsync(store.StoreId);
    }
}
