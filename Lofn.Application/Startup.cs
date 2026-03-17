using System;
using Lofn.Infra.Interfaces;
using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra;
using Lofn.Infra.Context;
using Lofn.Infra.Repository;
using Lofn.Domain.Core;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Lofn.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Lofn.Domain;
using NAuth.ACL;
using NAuth.ACL.Interfaces;
using zTools.ACL.Interfaces;
using zTools.ACL;

namespace Lofn.Application
{
    public static class Startup
    {
        private static void injectDependency(Type serviceType, Type implementationType, IServiceCollection services, bool scoped = true)
        {
            if (scoped)
                services.AddScoped(serviceType, implementationType);
            else
                services.AddTransient(serviceType, implementationType);
        }

        public static void ConfigureLofn(this IServiceCollection services, string connectionString, bool scoped = true)
        {
            if (scoped)
                services.AddDbContext<LofnContext>(x => x.UseLazyLoadingProxies().UseNpgsql(connectionString));
            else
                services.AddDbContextFactory<LofnContext>(x => x.UseLazyLoadingProxies().UseNpgsql(connectionString));

            #region Infra
            injectDependency(typeof(LofnContext), typeof(LofnContext), services, scoped);
            injectDependency(typeof(IUnitOfWork), typeof(UnitOfWork), services, scoped);
            injectDependency(typeof(ILogCore), typeof(LogCore), services, scoped);
            #endregion

            #region Repository
            injectDependency(typeof(IOrderRepository<OrderModel>), typeof(OrderRepository), services, scoped);
            injectDependency(typeof(IOrderItemRepository<OrderItemModel>), typeof(OrderItemRepository), services, scoped);
            injectDependency(typeof(IProductRepository<ProductModel>), typeof(ProductRepository), services, scoped);
            #endregion

            #region Client
            services.AddHttpClient();
            injectDependency(typeof(IUserClient), typeof(UserClient), services, scoped);
            injectDependency(typeof(IChatGPTClient), typeof(ChatGPTClient), services, scoped);
            injectDependency(typeof(IMailClient), typeof(MailClient), services, scoped);
            injectDependency(typeof(IFileClient), typeof(FileClient), services, scoped);
            injectDependency(typeof(IStringClient), typeof(StringClient), services, scoped);
            injectDependency(typeof(IDocumentClient), typeof(DocumentClient), services, scoped);
            #endregion

            #region Service
            injectDependency(typeof(IProductService), typeof(ProductService), services, scoped);
            injectDependency(typeof(IOrderService), typeof(OrderService), services, scoped);
            #endregion

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, NAuthHandler>("BasicAuthentication", null);
        }
    }
}
