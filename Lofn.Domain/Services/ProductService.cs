using Lofn.Infra.Interfaces.Repository;
using Microsoft.Extensions.Options;
using Lofn.Domain.Models;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Product;
using Lofn.DTO.Settings;
using zTools.ACL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Domain.Services
{
    public class ProductService : IProductService
    {
        private readonly IOptions<LofnSetting> _nsalesSettings;
        private readonly IFileClient _fileClient;
        private readonly IStringClient _stringClient;
        private readonly IProductRepository<ProductModel> _productRepository;

        public ProductService(
            IOptions<LofnSetting> nsalesSettings,
            IFileClient fileClient,
            IStringClient stringClient,
            IProductRepository<ProductModel> productRepository
        )
        {
            _nsalesSettings = nsalesSettings;
            _fileClient = fileClient;
            _stringClient = stringClient;
            _productRepository = productRepository;
        }

        public async Task<ProductModel> GetByIdAsync(long productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        public async Task<ProductModel> GetBySlugAsync(string productSlug)
        {
            return await _productRepository.GetBySlugAsync(productSlug);
        }

        public async Task<ProductInfo> GetProductInfoAsync(ProductModel md)
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

        private async Task<string> GenerateSlugAsync(long productId, string slug, string name)
        {
            string newSlug;
            int c = 0;
            do
            {
                newSlug = await _stringClient.GenerateSlugAsync(!string.IsNullOrEmpty(slug) ? slug : name);
                if (c > 0)
                {
                    newSlug += c.ToString();
                }
                c++;
            } while (await _productRepository.ExistSlugAsync(productId, newSlug));
            return newSlug;
        }

        public async Task<ProductModel> InsertAsync(ProductInfo product, long userId)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                throw new Exception("Name is empty");
            }
            if (!(product.Price > 0))
            {
                throw new Exception("Price cant be 0");
            }

            var model = new ProductModel
            {
                ProductId = product.ProductId,
                NetworkId = product.NetworkId,
                UserId = userId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Frequency = product.Frequency,
                Limit = product.Limit,
                Status = product.Status,
                Slug = await GenerateSlugAsync(product.ProductId, product.Slug, product.Name)
            };

            return await _productRepository.InsertAsync(model);
        }

        public async Task<ProductModel> UpdateAsync(ProductInfo product, long userId)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                throw new Exception("Name is empty");
            }
            if (!(product.Price > 0))
            {
                throw new Exception("Price cant be 0");
            }

            var model = new ProductModel
            {
                ProductId = product.ProductId,
                NetworkId = product.NetworkId,
                Name = product.Name,
                Image = product.Image,
                Description = product.Description,
                Price = product.Price,
                Frequency = product.Frequency,
                Limit = product.Limit,
                Status = product.Status,
                Slug = await GenerateSlugAsync(product.ProductId, product.Slug, product.Name)
            };

            return await _productRepository.UpdateAsync(model);
        }

        public async Task<ProductListPagedResult> SearchAsync(ProductSearchInternalParam param)
        {
            var (items, pageCount) = await _productRepository.SearchAsync(
                param.NetworkId <= 0 ? null : param.NetworkId,
                param.UserId <= 0 ? null : param.UserId,
                param.Keyword,
                param.OnlyActive,
                param.PageNum
            );

            var products = new List<ProductInfo>();
            foreach (var item in items)
            {
                products.Add(await GetProductInfoAsync(item));
            }

            return new ProductListPagedResult
            {
                Sucesso = true,
                Products = products,
                PageNum = param.PageNum,
                PageCount = pageCount
            };
        }

        public async Task<IList<ProductModel>> ListByNetworkAsync(long networkId)
        {
            var items = await _productRepository.ListByNetworkAsync(networkId);
            return items.OrderBy(x => x.Price).ToList();
        }
    }
}
