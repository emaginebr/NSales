using Core.Domain;
using Microsoft.Extensions.Options;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using NSales.Domain.Interfaces.Services;
using NSales.DTO.Product;
using NSales.DTO.Settings;
using NTools.ACL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSales.Domain.Impl.Services
{
    public class ProductService : IProductService
    {
        private readonly IOptions<NSalesSetting> _nsalesSettings;
        private readonly IFileClient _fileClient;
        private readonly IStringClient _stringClient;
        //private readonly IUserNetworkDomainFactory _userNetworkFactory;
        private readonly IProductDomainFactory _productFactory;

        public ProductService(
            IOptions<NSalesSetting> nsalesSettings,
            IFileClient fileClient,
            IStringClient stringClient,
            //IUserNetworkDomainFactory userNetworkFactory,
            IProductDomainFactory productFactory
        )
        {
            _nsalesSettings = nsalesSettings;
            _fileClient = fileClient;
            _stringClient = stringClient;
            //_userNetworkFactory = userNetworkFactory;
            _productFactory = productFactory;
        }

        private void ValidateAccess(long? networkId, long userId)
        {
            /*
            var networkAccess = _userNetworkFactory.BuildUserNetworkModel().Get(networkId, userId, _userNetworkFactory);

            if (networkAccess == null)
            {
                throw new Exception("Your dont have access to this network");
            }

            if (networkAccess.Role != DTO.User.UserRoleEnum.NetworkManager)
            {
                var user = _userFactory.BuildUserModel().GetById(userId, _userFactory);
                if (user == null)
                {
                    throw new Exception("User not found");
                }
                if (!user.IsAdmin)
                {
                    throw new Exception("Your dont have access to this network");
                }
            }
            */
        }
        public IProductModel GetById(long productId)
        {
            return _productFactory.BuildProductModel().GetById(productId, _productFactory);
        }

        public IProductModel GetBySlug(string productSlug)
        {
            return _productFactory.BuildProductModel().GetBySlug(productSlug, _productFactory);
        }

        public async Task<ProductInfo> GetProductInfo(IProductModel md)
        {
            return new ProductInfo
            {
                ProductId = md.ProductId,
                NetworkId = md.NetworkId,
                Name = md.Name,
                Slug = md.Slug,
                Image = md.Image,
                ImageUrl = await _fileClient.GetFileUrlAsync(_nsalesSettings.Value.BucketName, md.Image),
                Description = md.Description,
                Price = md.Price,
                Frequency = md.Frequency,
                Limit = md.Limit,
                Status = md.Status
            };
        }

        private async Task<string> GenerateSlug(IProductModel md)
        {
            string newSlug;
            int c = 0;
            do
            {
                newSlug = await _stringClient.GenerateSlugAsync((!string.IsNullOrEmpty(md.Slug)) ? md.Slug : md.Name);
                if (c > 0)
                {
                    newSlug += c.ToString();
                }
                c++;
            } while (md.ExistSlug(md.ProductId, newSlug));
            return newSlug;
        }

        public async Task<IProductModel> Insert(ProductInfo product, long userId)
        {
            ValidateAccess(product.NetworkId, userId);

            if (string.IsNullOrEmpty(product.Name))
            {
                throw new Exception("Name is empty");
            }
            if (!(product.Price > 0))
            {
                throw new Exception("Price cant be 0");
            }

            var model = _productFactory.BuildProductModel();

            model.ProductId = product.ProductId;
            model.NetworkId = product.NetworkId;
            model.UserId = userId;
            model.Name = product.Name;
            model.Description = product.Description;
            model.Price = product.Price;
            model.Frequency = product.Frequency;
            model.Limit = product.Limit;
            model.Status = product.Status;
            model.Slug = await GenerateSlug(model);

            return model.Insert(_productFactory);
        }

        public async Task<IProductModel> Update(ProductInfo product, long userId)
        {
            ValidateAccess(product.NetworkId, userId);

            if (string.IsNullOrEmpty(product.Name))
            {
                throw new Exception("Name is empty");
            }
            if (!(product.Price > 0))
            {
                throw new Exception("Price cant be 0");
            }

            var model = _productFactory.BuildProductModel();

            model.ProductId = product.ProductId;
            model.NetworkId = product.NetworkId;
            model.Name = product.Name;
            model.Image = product.Image;
            model.Description = product.Description;
            model.Price = product.Price;
            model.Frequency = product.Frequency;
            model.Limit = product.Limit;
            model.Status = product.Status;
            model.Slug = await GenerateSlug(model);

            return model.Update(_productFactory);
        }

        public ProductListPagedResult Search(ProductSearchInternalParam param)
        {
            var model = _productFactory.BuildProductModel();
            int pageCount = 0;
            var products = model.Search(
                    param.NetworkId <= 0 ? null : param.NetworkId,
                    param.UserId <= 0 ? null : param.UserId,
                    param.Keyword,
                    param.OnlyActive, param.PageNum,
                    out pageCount, _productFactory
                )
                .Select(x => GetProductInfo(x).GetAwaiter().GetResult())
                .ToList();
            return new ProductListPagedResult
            {
                Sucesso = true,
                Products = products,
                PageNum = param.PageNum,
                PageCount = pageCount
            };
        }

        public IList<IProductModel> ListByNetwork(long networkId)
        {
            return _productFactory
                .BuildProductModel()
                .ListByNetwork(networkId, _productFactory)
                .OrderBy(x => x.Price)
                .ToList();
        }

        /*
        public IProductModel GetByStripeProductId(string stripeProductId)
        {
            return _productFactory.BuildProductModel().GetByStripeProductId(stripeProductId, _productFactory);
        }

        public IProductModel GetByStripePriceId(string stripePriceId)
        {
            return _productFactory.BuildProductModel().GetByStripePriceId(stripePriceId, _productFactory);
        }
        */
    }
}
