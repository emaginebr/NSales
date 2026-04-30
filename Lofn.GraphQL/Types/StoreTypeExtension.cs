using HotChocolate.Types;
using Lofn.Infra.Context;

namespace Lofn.GraphQL.Types;

public class StoreTypeExtension : ObjectTypeExtension<Store>
{
    protected override void Configure(IObjectTypeDescriptor<Store> descriptor)
    {
        descriptor.Field(t => t.Logo).IsProjected(true);

        descriptor
            .Field("logoUrl")
            .Type<StringType>()
            .Resolve(ctx =>
            {
                var store = ctx.Parent<Store>();
                return string.IsNullOrEmpty(store.Logo) ? null : store.Logo;
            });
    }
}
