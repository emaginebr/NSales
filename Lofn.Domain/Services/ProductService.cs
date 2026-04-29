using Lofn.Infra.Interfaces.Repository;
using Lofn.Domain.Core;
using Lofn.Domain.Mappers;
using Lofn.Domain.Models;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Product;
using zTools.ACL.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lofn.DTO.Store;

namespace Lofn.Domain.Services
{
    public class ProductService : IProductService
    {
        private readonly ITenantResolver _tenantResolver;
        private readonly IFileClient _fileClient;
        private readonly ISlugGenerator _slugGenerator;
        private readonly IProductRepository<ProductModel> _productRepository;
        private readonly IProductImageService _productImageService;
        private readonly IStoreUserRepository<StoreUserModel> _storeUserRepository;
        private readonly IStoreRepository<StoreModel> _storeRepository;
        private readonly ICategoryRepository<CategoryModel> _categoryRepository;

        public ProductService(
            ITenantResolver tenantResolver,
            IFileClient fileClient,
            ISlugGenerator slugGenerator,
            IProductRepository<ProductModel> productRepository,
            IProductImageService productImageService,
            IStoreUserRepository<StoreUserModel> storeUserRepository,
            IStoreRepository<StoreModel> storeRepository,
            ICategoryRepository<CategoryModel> categoryRepository
        )
        {
            _tenantResolver = tenantResolver;
            _fileClient = fileClient;
            _slugGenerator = slugGenerator;
            _productRepository = productRepository;
            _productImageService = productImageService;
            _storeUserRepository = storeUserRepository;
            _storeRepository = storeRepository;
            _categoryRepository = categoryRepository;
        }

        private async Task ValidateStoreUserAsync(long storeId, long userId)
        {
            if (storeId <= 0)
                throw new Exception("StoreId is required");

            if (!await _storeUserRepository.ExistsAsync(storeId, userId))
                throw new UnauthorizedAccessException("Access denied: user does not belong to this store");
        }

        private async Task AssertCategoryAllowedAsync(long? categoryId, long storeId)
        {
            if (categoryId is null) return;

            var cat = await _categoryRepository.GetByIdAsync(categoryId.Value)
                ?? throw BuildValidationException($"Category {categoryId} not found");

            if (_tenantResolver.Marketplace)
            {
                if (cat.StoreId != null)
                    throw BuildValidationException("CategoryId must reference a tenant-global category in marketplace mode");
            }
            else
            {
                if (cat.StoreId != storeId)
                    throw BuildValidationException("CategoryId does not belong to this store");
            }
        }

        private static ValidationException BuildValidationException(string message)
        {
            return new ValidationException(message, new[] { new ValidationFailure(string.Empty, message) });
        }

        public async Task<ProductModel> GetByIdAsync(long productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        public async Task<ProductModel> GetByIdAsync(long productId, long storeId, long userId)
        {
            await ValidateStoreUserAsync(storeId, userId);

            var model = await _productRepository.GetByIdAsync(productId);
            if (model == null)
                return null;

            if (model.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: product does not belong to this store");

            return model;
        }

        public async Task<ProductModel> GetBySlugAsync(string productSlug)
        {
            return await _productRepository.GetBySlugAsync(productSlug);
        }

        public async Task<ProductInfo> GetProductInfoAsync(ProductModel md)
        {
            var info = ProductMapper.ToInfo(md);
            info.Images = await _productImageService.ListByProductAsync(md.ProductId);
            return info;
        }

        private async Task<string> GenerateSlugAsync(long storeId, long productId, string name)
        {
            var baseSlug = _slugGenerator.Generate(name);
            string newSlug;
            int c = 0;
            do
            {
                newSlug = baseSlug;
                if (c > 0)
                {
                    newSlug += c.ToString();
                }
                c++;
            } while (await _productRepository.ExistSlugAsync(storeId, productId, newSlug));
            return newSlug;
        }

        public async Task<ProductModel> InsertAsync(ProductInsertInfo product, long storeId, long userId)
        {
            if (string.IsNullOrEmpty(product.Name))
                throw new Exception("Name is required");

            if (!(product.Price > 0))
                throw new Exception("Price is required");

            await ValidateStoreUserAsync(storeId, userId);
            await AssertCategoryAllowedAsync(product.CategoryId, storeId);

            var model = new ProductModel
            {
                StoreId = storeId,
                CategoryId = product.CategoryId,
                UserId = userId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Discount = product.Discount,
                Frequency = product.Frequency,
                Limit = product.Limit,
                Status = product.Status,
                ProductType = product.ProductType,
                Featured = product.Featured,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            model.Slug = await GenerateSlugAsync(storeId, 0, product.Name);

            return await _productRepository.InsertAsync(model);
        }

        public async Task<ProductModel> UpdateAsync(ProductUpdateInfo product, long storeId, long userId)
        {
            if (string.IsNullOrEmpty(product.Name))
                throw new Exception("Name is required");

            if (!(product.Price > 0))
                throw new Exception("Price is required");

            await ValidateStoreUserAsync(storeId, userId);
            await AssertCategoryAllowedAsync(product.CategoryId, storeId);

            var existing = await _productRepository.GetByIdAsync(product.ProductId);
            if (existing == null)
                throw new Exception("Product not found");

            if (existing.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: product does not belong to this store");

            existing.CategoryId = product.CategoryId;
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Discount = product.Discount;
            existing.Frequency = product.Frequency;
            existing.Limit = product.Limit;
            existing.Status = product.Status;
            existing.ProductType = product.ProductType;
            existing.Featured = product.Featured;
            existing.UpdatedAt = DateTime.Now;
            existing.Slug = await GenerateSlugAsync(storeId, product.ProductId, product.Name);

            return await _productRepository.UpdateAsync(existing);
        }

        public async Task<ProductListPagedResult> SearchAsync(ProductSearchInternalParam param)
        {
            var (items, pageCount) = await _productRepository.SearchAsync(
                param.StoreId <= 0 ? null : param.StoreId,
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
                Products = products,
                PageNum = param.PageNum,
                PageCount = pageCount
            };
        }

        public async Task<ProductListPagedResult> ListActiveByStoreSlugAsync(string storeSlug, string categorySlug, int pageNum)
        {
            var store = await _storeRepository.GetBySlugAsync(storeSlug);
            if (store == null)
                throw new Exception("Store not found");

            long? categoryId = null;
            if (!string.IsNullOrEmpty(categorySlug))
            {
                var category = await _categoryRepository.GetBySlugAndStoreAsync(store.StoreId, categorySlug);
                if (category == null)
                    throw new Exception("Category not found");
                categoryId = category.CategoryId;
            }

            var (items, pageCount) = await _productRepository.ListActiveByStoreAsync(store.StoreId, categoryId, pageNum);

            var products = new List<ProductInfo>();
            foreach (var item in items)
            {
                products.Add(await GetProductInfoAsync(item));
            }

            return new ProductListPagedResult
            {
                Products = products,
                PageNum = pageNum,
                PageCount = pageCount
            };
        }

        public async Task<IList<ProductInfo>> ListFeaturedByStoreSlugAsync(string storeSlug, int limit)
        {
            var store = await _storeRepository.GetBySlugAsync(storeSlug);
            if (store == null)
                throw new Exception("Store not found");

            var items = await _productRepository.ListFeaturedByStoreAsync(store.StoreId, limit);

            var products = new List<ProductInfo>();
            foreach (var item in items)
            {
                products.Add(await GetProductInfoAsync(item));
            }
            return products;
        }

        public async Task<IList<ProductModel>> ListByStoreAsync(long storeId)
        {
            var items = await _productRepository.ListByStoreAsync(storeId);
            return items.OrderBy(x => x.Price).ToList();
        }
    }
}
