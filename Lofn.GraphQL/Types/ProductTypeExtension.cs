using System.Linq;
using HotChocolate.Types;
using Lofn.Domain.Interfaces;
using Lofn.Infra.Context;
using Microsoft.EntityFrameworkCore;
using zTools.ACL.Interfaces;

namespace Lofn.GraphQL.Types;

public class ProductTypeExtension : ObjectTypeExtension<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor
            .Field("imageUrl")
            .Type<StringType>()
            .Resolve(async ctx =>
            {
                var product = ctx.Parent<Product>();
                var dbContextFactory = ctx.Service<TenantDbContextFactory>();

                string? firstImage;
                using (var dbContext = dbContextFactory.CreateDbContext())
                {
                    firstImage = await dbContext.ProductImages
                        .Where(i => i.ProductId == product.ProductId)
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.Image)
                        .FirstOrDefaultAsync();
                }

                if (string.IsNullOrEmpty(firstImage))
                    return null;

                var fileClient = ctx.Service<IFileClient>();
                var tenantResolver = ctx.Service<ITenantResolver>();
                return await fileClient.GetFileUrlAsync(tenantResolver.BucketName, firstImage);
            });
    }
}
