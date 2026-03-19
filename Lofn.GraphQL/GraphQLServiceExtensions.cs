using HotChocolate.Execution.Configuration;
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
            .AddProjections()
            .AddFiltering()
            .AddSorting();

        services
            .AddGraphQLServer("admin")
            .AddAuthorization()
            .AddDiagnosticEventListener<GraphQLErrorLogger>()
            .AddQueryType<AdminQuery>()
            .AddTypeExtension<StoreTypeExtension>()
            .AddTypeExtension<ProductTypeExtension>()
            .AddTypeExtension<ProductImageTypeExtension>()
            .AddTypeExtension<CategoryTypeExtension>()
            .AddProjections()
            .AddFiltering()
            .AddSorting();

        return services;
    }
}
