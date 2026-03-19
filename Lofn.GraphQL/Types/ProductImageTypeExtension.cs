using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Lofn.Domain.Interfaces;
using Lofn.Infra.Context;
using zTools.ACL.Interfaces;

namespace Lofn.GraphQL.Types;

[ExtendObjectType(typeof(ProductImage))]
public class ProductImageTypeExtension
{
    public async Task<string> GetImageUrl(
        [Parent] ProductImage productImage,
        [Service] IFileClient fileClient,
        [Service] ITenantResolver tenantResolver)
    {
        if (string.IsNullOrEmpty(productImage.Image)) return null;
        return await fileClient.GetFileUrlAsync(tenantResolver.BucketName, productImage.Image);
    }
}
