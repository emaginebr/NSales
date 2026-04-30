using System;
using Lofn.Infra.Interfaces;
using Lofn.Infra.Interfaces.AppService;
using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra;
using Lofn.Infra.AppService;
using Lofn.Infra.Context;
using Lofn.Infra.Repository;
using Lofn.Domain.Core;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Lofn.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Lofn.ACL;
using Lofn.ACL.Handlers;
using Lofn.ACL.Interfaces;
using Lofn.Domain;
using Microsoft.Extensions.Configuration;
using NAuth.ACL;
using NAuth.ACL.Interfaces;
using FluentValidation;
using Lofn.Domain.Validators;
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

        public static void ConfigureLofn(this IServiceCollection services, bool scoped = true)
        {
            #region Tenant
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantResolver, TenantResolver>();
            services.AddScoped<TenantDbContextFactory>();
            services.AddScoped(sp => sp.GetRequiredService<TenantDbContextFactory>().CreateDbContext());
            services.AddTransient<TenantHeaderHandler>();
            #endregion

            #region Infra
            injectDependency(typeof(IUnitOfWork), typeof(UnitOfWork), services, scoped);
            injectDependency(typeof(ILogCore), typeof(LogCore), services, scoped);
            injectDependency(typeof(IRabbitMQAppService), typeof(RabbitMQAppService), services, scoped);
            #endregion

            #region Repository
            injectDependency(typeof(IProductRepository<ProductModel>), typeof(ProductRepository), services, scoped);
            injectDependency(typeof(IProductImageRepository<ProductImageModel>), typeof(ProductImageRepository), services, scoped);
            injectDependency(typeof(ICategoryRepository<CategoryModel>), typeof(CategoryRepository), services, scoped);
            injectDependency(typeof(IStoreRepository<StoreModel>), typeof(StoreRepository), services, scoped);
            injectDependency(typeof(IStoreUserRepository<StoreUserModel>), typeof(StoreUserRepository), services, scoped);
            injectDependency(
                typeof(IProductTypeRepository<ProductTypeModel, ProductTypeFilterModel, ProductTypeCustomizationGroupModel, ProductTypeCustomizationOptionModel>),
                typeof(ProductTypeRepository), services, scoped);
            injectDependency(typeof(IProductFilterValueRepository<ProductFilterValueModel>), typeof(ProductFilterValueRepository), services, scoped);
            #endregion

            #region Client
            services.AddHttpClient();
            injectDependency(typeof(IUserClient), typeof(UserClient), services, scoped);
            injectDependency(typeof(IChatGPTClient), typeof(ChatGPTClient), services, scoped);
            injectDependency(typeof(IMailClient), typeof(MailClient), services, scoped);
            injectDependency(typeof(IFileClient), typeof(FileClient), services, scoped);
            injectDependency(typeof(IDocumentClient), typeof(DocumentClient), services, scoped);
            injectDependency(typeof(IProductClient), typeof(ProductClient), services, scoped);
            injectDependency(typeof(IStoreClient), typeof(StoreClient), services, scoped);
            injectDependency(typeof(ICategoryClient), typeof(CategoryClient), services, scoped);
            injectDependency(typeof(IStoreUserClient), typeof(StoreUserClient), services, scoped);
            injectDependency(typeof(IImageClient), typeof(ImageClient), services, scoped);
            #endregion

            #region Core
            injectDependency(typeof(ISlugGenerator), typeof(SlugGenerator), services, scoped);
            #endregion

            #region Service
            injectDependency(typeof(IProductService), typeof(ProductService), services, scoped);
            injectDependency(typeof(IProductImageService), typeof(ProductImageService), services, scoped);
            injectDependency(typeof(ICategoryService), typeof(CategoryService), services, scoped);
            injectDependency(typeof(IStoreService), typeof(StoreService), services, scoped);
            injectDependency(typeof(IStoreUserService), typeof(StoreUserService), services, scoped);
            injectDependency(typeof(IShopCartService), typeof(ShopCartService), services, scoped);
            injectDependency(typeof(IProductTypeService), typeof(ProductTypeService), services, scoped);
            services.AddScoped<ProductFilterValueResolver>();
            services.AddScoped<ProductPriceCalculator>();
            #endregion

            #region Validators
            services.AddValidatorsFromAssemblyContaining<ShopCartInfoValidator>(ServiceLifetime.Scoped);
            #endregion

            services.AddScoped<ITenantSecretProvider, NAuthTenantSecretProvider>();
            services.AddNAuth<NAuthTenantProvider>();
            services.AddNAuthAuthentication("BasicAuthentication");
        }
    }
}
