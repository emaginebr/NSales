using System.Linq;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
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
    public IQueryable<Category> GetCategories(LofnContext context)
        => context.Categories.Where(c => c.Products.Any(p => p.Status == 1));

    [UseProjection]
    public IQueryable<Store> GetStoreBySlug(LofnContext context, string slug)
        => context.Stores.Where(s => s.Status == 1 && s.Slug == slug);

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetFeaturedProducts(LofnContext context, string storeSlug)
        => context.Products.Where(p => p.Status == 1 && p.Featured && p.Store.Slug == storeSlug);
}
