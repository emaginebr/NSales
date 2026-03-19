using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Lofn.Domain.Interfaces;
using Lofn.Infra.Context;
using zTools.ACL.Interfaces;

namespace Lofn.GraphQL.Types;

[ExtendObjectType(typeof(Product))]
public class ProductTypeExtension
{
    public async Task<string> GetImageUrl(
        [Parent] Product product,
        [Service] IFileClient fileClient,
        [Service] ITenantResolver tenantResolver)
    {
        if (string.IsNullOrEmpty(product.Image)) return null;
        return await fileClient.GetFileUrlAsync(tenantResolver.BucketName, product.Image);
    }
}
