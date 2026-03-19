using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using Lofn.Infra.Context;

namespace Lofn.GraphQL.Types;

[ExtendObjectType(typeof(Category))]
public class CategoryTypeExtension
{
    public int GetProductCount(
        [Parent] Category category,
        [Service] LofnContext context)
    {
        return context.Products.Count(p => p.CategoryId == category.CategoryId && p.Status == 1);
    }
}
