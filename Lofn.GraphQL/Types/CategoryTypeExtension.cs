using System.Linq;
using HotChocolate;
using HotChocolate.Types;
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
}
