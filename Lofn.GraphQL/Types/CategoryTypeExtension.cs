using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Lofn.Domain.Interfaces;
using Lofn.Infra.Context;

namespace Lofn.GraphQL.Types;

[ExtendObjectType(typeof(Category))]
public class CategoryTypeExtension
{
    public bool GetIsGlobal([Parent] Category category) => category.StoreId is null;

    public int GetProductCount(
        [Parent] Category category,
        [Service] TenantDbContextFactory dbContextFactory)
    {
        using var context = dbContextFactory.CreateDbContext();
        return context.Products.Count(p => p.CategoryId == category.CategoryId && p.Status == 1);
    }

    public async Task<long?> GetAppliedProductTypeId(
        [Parent] Category category,
        [Service] ICategoryService categoryService)
    {
        var resolution = await categoryService.GetAppliedProductTypeAsync(category.CategoryId);
        return resolution?.ProductType?.ProductTypeId;
    }

    public async Task<long?> GetAppliedProductTypeOriginCategoryId(
        [Parent] Category category,
        [Service] ICategoryService categoryService)
    {
        var resolution = await categoryService.GetAppliedProductTypeAsync(category.CategoryId);
        return resolution?.OriginCategoryId;
    }
}
