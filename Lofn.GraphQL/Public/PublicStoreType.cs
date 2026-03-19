using HotChocolate.Types;
using Lofn.Infra.Context;

namespace Lofn.GraphQL.Public;

public class PublicStoreType : ObjectType<Store>
{
    protected override void Configure(IObjectTypeDescriptor<Store> descriptor)
    {
        descriptor.Ignore(s => s.StoreUsers);
        descriptor.Ignore(s => s.OwnerId);
        descriptor.Ignore(s => s.Orders);
    }
}
