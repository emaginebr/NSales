using System.Linq;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Lofn.Infra.Context;
using Microsoft.AspNetCore.Http;
using NAuth.ACL.Interfaces;

namespace Lofn.GraphQL.Admin;

[Authorize]
public class AdminQuery
{
    private IQueryable<long> GetUserStoreIds(LofnContext context, IHttpContextAccessor httpContextAccessor, IUserClient userClient)
    {
        var userSession = userClient.GetUserInSession(httpContextAccessor.HttpContext!);
        var userId = userSession!.UserId;
        return context.StoreUsers
            .Where(su => su.UserId == userId)
            .Select(su => su.StoreId);
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Store> GetMyStores(
        LofnContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var storeIds = GetUserStoreIds(context, httpContextAccessor, userClient);
        return context.Stores.Where(s => storeIds.Contains(s.StoreId));
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetMyProducts(
        LofnContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var storeIds = GetUserStoreIds(context, httpContextAccessor, userClient);
        return context.Products.Where(p => p.StoreId.HasValue && storeIds.Contains(p.StoreId.Value));
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Category> GetMyCategories(
        LofnContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient)
    {
        var storeIds = GetUserStoreIds(context, httpContextAccessor, userClient);
        return context.Categories.Where(c => c.StoreId.HasValue && storeIds.Contains(c.StoreId.Value));
    }

}
