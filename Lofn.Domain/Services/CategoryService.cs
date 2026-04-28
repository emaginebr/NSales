using Lofn.Infra.Interfaces.Repository;
using Lofn.Domain.Mappers;
using Lofn.Domain.Models;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Category;
using zTools.ACL.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Domain.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IStringClient _stringClient;
        private readonly ICategoryRepository<CategoryModel> _categoryRepository;
        private readonly IStoreRepository<StoreModel> _storeRepository;
        private readonly IValidator<CategoryGlobalInsertInfo> _globalInsertValidator;
        private readonly IValidator<CategoryGlobalUpdateInfo> _globalUpdateValidator;

        public CategoryService(
            IStringClient stringClient,
            ICategoryRepository<CategoryModel> categoryRepository,
            IStoreRepository<StoreModel> storeRepository,
            IValidator<CategoryGlobalInsertInfo> globalInsertValidator,
            IValidator<CategoryGlobalUpdateInfo> globalUpdateValidator
        )
        {
            _stringClient = stringClient;
            _categoryRepository = categoryRepository;
            _storeRepository = storeRepository;
            _globalInsertValidator = globalInsertValidator;
            _globalUpdateValidator = globalUpdateValidator;
        }

        public async Task<IList<CategoryInfo>> ListAllAsync()
        {
            var items = await _categoryRepository.ListAllAsync();
            return items.Select(CategoryMapper.ToInfo).ToList();
        }

        public async Task<IList<CategoryInfo>> ListByStoreAsync(long storeId)
        {
            var items = await _categoryRepository.ListByStoreAsync(storeId);
            return items.Select(CategoryMapper.ToInfo).ToList();
        }

        public async Task<IList<CategoryInfo>> ListGlobalAsync()
        {
            var items = await _categoryRepository.ListGlobalAsync();
            return items.Select(CategoryMapper.ToInfo).ToList();
        }

        public async Task<IList<CategoryInfo>> ListActiveByStoreSlugAsync(string storeSlug)
        {
            var store = await _storeRepository.GetBySlugAsync(storeSlug);
            if (store == null)
                throw new Exception("Store not found");

            var items = await _categoryRepository.ListByStoreAsync(store.StoreId);
            var counts = await _categoryRepository.CountActiveProductsByStoreAsync(store.StoreId);

            return items
                .Where(x => counts.ContainsKey(x.CategoryId) && counts[x.CategoryId] > 0)
                .Select(x =>
                {
                    var info = CategoryMapper.ToInfo(x);
                    info.ProductCount = counts[x.CategoryId];
                    return info;
                }).ToList();
        }

        public async Task<CategoryInfo> GetBySlugAndStoreSlugAsync(string storeSlug, string categorySlug)
        {
            var store = await _storeRepository.GetBySlugAsync(storeSlug);
            if (store == null)
                throw new Exception("Store not found");

            var model = await _categoryRepository.GetBySlugAndStoreAsync(store.StoreId, categorySlug);
            if (model == null)
                return null;

            return CategoryMapper.ToInfo(model);
        }

        public async Task<IList<CategoryInfo>> ListWithProductCountAsync()
        {
            var items = await _categoryRepository.ListAllAsync();
            var counts = await _categoryRepository.CountProductsByCategoryAsync();
            return items.Select(x =>
            {
                var info = CategoryMapper.ToInfo(x);
                info.ProductCount = counts.ContainsKey(x.CategoryId) ? counts[x.CategoryId] : 0;
                return info;
            }).ToList();
        }

        private async Task ValidateStoreOwnerAsync(long storeId, long userId)
        {
            if (storeId <= 0)
                throw new Exception("StoreId is required");

            var store = await _storeRepository.GetByIdAsync(storeId);
            if (store == null)
                throw new Exception("Store not found");

            if (store.OwnerId != userId)
                throw new UnauthorizedAccessException("Access denied: user is not the owner of this store");
        }

        public async Task<CategoryModel> GetByIdAsync(long categoryId, long storeId, long userId)
        {
            await ValidateStoreOwnerAsync(storeId, userId);

            var model = await _categoryRepository.GetByIdAsync(categoryId);
            if (model == null)
                return null;

            if (model.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: category does not belong to this store");

            return model;
        }

        private async Task<string> GenerateSlugAsync(long? exceptCategoryId, string name)
        {
            string newSlug;
            int c = 0;
            do
            {
                newSlug = await _stringClient.GenerateSlugAsync(name);
                if (c > 0)
                {
                    newSlug += c.ToString();
                }
                c++;
            } while (await _categoryRepository.ExistSlugInTenantAsync(exceptCategoryId, newSlug));
            return newSlug;
        }

        public async Task<CategoryModel> InsertAsync(CategoryInsertInfo category, long storeId, long userId)
        {
            if (string.IsNullOrEmpty(category.Name))
                throw new Exception("Name is required");

            await ValidateStoreOwnerAsync(storeId, userId);

            var model = new CategoryModel
            {
                Name = category.Name,
                StoreId = storeId
            };
            model.Slug = await GenerateSlugAsync(null, category.Name);

            return await _categoryRepository.InsertAsync(model);
        }

        public async Task<CategoryModel> UpdateAsync(CategoryUpdateInfo category, long storeId, long userId)
        {
            if (string.IsNullOrEmpty(category.Name))
                throw new Exception("Name is required");

            await ValidateStoreOwnerAsync(storeId, userId);

            var existing = await _categoryRepository.GetByIdAsync(category.CategoryId);
            if (existing == null)
                throw new Exception("Category not found");

            if (existing.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: category does not belong to this store");

            existing.Name = category.Name;
            existing.Slug = await GenerateSlugAsync(category.CategoryId, category.Name);

            return await _categoryRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(long categoryId, long storeId, long userId)
        {
            await ValidateStoreOwnerAsync(storeId, userId);

            var model = await _categoryRepository.GetByIdAsync(categoryId);
            if (model == null)
                throw new Exception("Category not found");

            if (model.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: category does not belong to this store");

            await _categoryRepository.DeleteAsync(categoryId);
        }

        public async Task<CategoryInfo> InsertGlobalAsync(CategoryGlobalInsertInfo category)
        {
            await _globalInsertValidator.ValidateAndThrowAsync(category);

            var model = new CategoryModel
            {
                Name = category.Name,
                StoreId = null
            };
            model.Slug = await GenerateSlugAsync(null, category.Name);

            var inserted = await _categoryRepository.InsertAsync(model);
            return CategoryMapper.ToInfo(inserted);
        }

        public async Task<CategoryInfo> UpdateGlobalAsync(CategoryGlobalUpdateInfo category)
        {
            await _globalUpdateValidator.ValidateAndThrowAsync(category);

            var existing = await _categoryRepository.GetByIdAsync(category.CategoryId);
            if (existing == null)
                throw BuildValidationException($"Category {category.CategoryId} not found");

            if (existing.StoreId != null)
                throw BuildValidationException($"Category {category.CategoryId} is not global");

            existing.Name = category.Name;
            existing.Slug = await GenerateSlugAsync(category.CategoryId, category.Name);

            var updated = await _categoryRepository.UpdateAsync(existing);
            return CategoryMapper.ToInfo(updated);
        }

        public async Task DeleteGlobalAsync(long categoryId)
        {
            var existing = await _categoryRepository.GetByIdAsync(categoryId);
            if (existing == null)
                throw BuildValidationException($"Category {categoryId} not found");

            if (existing.StoreId != null)
                throw BuildValidationException($"Category {categoryId} is not global");

            await _categoryRepository.DeleteAsync(categoryId);
        }

        private static ValidationException BuildValidationException(string message)
        {
            return new ValidationException(message, new[] { new ValidationFailure(string.Empty, message) });
        }
    }
}
