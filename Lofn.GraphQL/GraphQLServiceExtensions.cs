using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Options;
using HotChocolate.Types.Pagination;
using Lofn.GraphQL.Admin;
using Lofn.GraphQL.Public;
using Lofn.GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Lofn.GraphQL;

public static class GraphQLServiceExtensions
{
    public static IServiceCollection AddLofnGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddDiagnosticEventListener<GraphQLErrorLogger>()
            .AddQueryType<PublicQuery>()
            .AddType<PublicStoreType>()
            .AddTypeExtension<StoreTypeExtension>()
            .AddTypeExtension<ProductTypeExtension>()
            .AddTypeExtension<ProductImageTypeExtension>()
            .AddTypeExtension<CategoryTypeExtension>()
            .SetPagingOptions(new PagingOptions
            {
                MaxPageSize = 50,
                DefaultPageSize = 10,
                IncludeTotalCount = true
            })
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .ModifyCostOptions(o => o.MaxFieldCost = 8000);

        services
            .AddGraphQLServer("admin")
            .AddAuthorization()
            .AddDiagnosticEventListener<GraphQLErrorLogger>()
            .AddQueryType<AdminQuery>()
            .AddTypeExtension<StoreTypeExtension>()
            .AddTypeExtension<ProductTypeExtension>()
            .AddTypeExtension<ProductImageTypeExtension>()
            .AddTypeExtension<CategoryTypeExtension>()
            .SetPagingOptions(new PagingOptions
            {
                MaxPageSize = 50,
                DefaultPageSize = 10,
                IncludeTotalCount = true
            })
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .ModifyCostOptions(o => o.MaxFieldCost = 8000);

        return services;
    }
}
