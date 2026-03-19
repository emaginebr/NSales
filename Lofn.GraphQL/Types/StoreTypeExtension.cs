using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Lofn.Domain.Interfaces;
using Lofn.Infra.Context;
using zTools.ACL.Interfaces;

namespace Lofn.GraphQL.Types;

[ExtendObjectType(typeof(Store))]
public class StoreTypeExtension
{
    public async Task<string> GetLogoUrl(
        [Parent] Store store,
        [Service] IFileClient fileClient,
        [Service] ITenantResolver tenantResolver)
    {
        if (string.IsNullOrEmpty(store.Logo)) return null;
        return await fileClient.GetFileUrlAsync(tenantResolver.BucketName, store.Logo);
    }
}
