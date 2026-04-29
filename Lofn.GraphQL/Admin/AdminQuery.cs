using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Category;
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
        [Service] IUserClient userClient,
        [Service] ITenantResolver tenantResolver)
    {
        if (tenantResolver.Marketplace)
            return context.Categories.Where(c => c.StoreId == null);

        var storeIds = GetUserStoreIds(context, httpContextAccessor, userClient);
        return context.Categories.Where(c => c.StoreId.HasValue && storeIds.Contains(c.StoreId.Value));
    }

    public async Task<IList<CategoryTreeNodeInfo>> GetMyCategoryTree(
        LofnContext context,
        IHttpContextAccessor httpContextAccessor,
        [Service] IUserClient userClient,
        [Service] ITenantResolver tenantResolver,
        [Service] ICategoryService categoryService)
    {
        if (tenantResolver.Marketplace)
            return await categoryService.GetTreeAsync(null);

        var storeIds = GetUserStoreIds(context, httpContextAccessor, userClient).ToList();
        var aggregated = new List<CategoryTreeNodeInfo>();
        foreach (var storeId in storeIds)
        {
            var subtree = await categoryService.GetTreeAsync(storeId);
            aggregated.AddRange(subtree);
        }
        return aggregated;
    }
}
