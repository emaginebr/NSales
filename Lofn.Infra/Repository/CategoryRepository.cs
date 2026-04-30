using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra.Context;
using Lofn.Infra.Mappers;
using Lofn.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Infra.Repository
{
    public class CategoryRepository : ICategoryRepository<CategoryModel>
    {
        private readonly LofnContext _context;

        public CategoryRepository(LofnContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryModel>> ListAllAsync()
        {
            var rows = await _context.Categories
                .OrderBy(x => x.Name)
                .ToListAsync();
            return rows.Select(CategoryDbMapper.ToModel);
        }

        public async Task<IEnumerable<CategoryModel>> ListByStoreAsync(long storeId)
        {
            var rows = await _context.Categories
                .Where(x => x.StoreId == storeId)
                .OrderBy(x => x.Name)
                .ToListAsync();
            return rows.Select(CategoryDbMapper.ToModel);
        }

        public async Task<IEnumerable<CategoryModel>> ListGlobalAsync()
        {
            var rows = await _context.Categories
                .Where(x => x.StoreId == null)
                .OrderBy(x => x.Name)
                .ToListAsync();
            return rows.Select(CategoryDbMapper.ToModel);
        }

        public async Task<CategoryModel> GetByIdAsync(long id)
        {
            var row = await _context.Categories.FindAsync(id);
            if (row == null)
                return null;
            return CategoryDbMapper.ToModel(row);
        }

        public async Task<CategoryModel> GetBySlugAsync(string slug)
        {
            var row = await _context.Categories
                .Where(x => x.Slug == slug)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return CategoryDbMapper.ToModel(row);
        }

        public async Task<CategoryModel> InsertAsync(CategoryModel model)
        {
            var row = new Category();
            CategoryDbMapper.ToEntity(model, row);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.CategoryId = row.CategoryId;
            return model;
        }

        public async Task<CategoryModel> UpdateAsync(CategoryModel model)
        {
            var row = await _context.Categories.FindAsync(model.CategoryId);
            CategoryDbMapper.ToEntity(model, row);
            _context.Categories.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.Categories.FindAsync(id);
            if (row != null)
            {
                _context.Categories.Remove(row);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistSlugAsync(long storeId, long categoryId, string slug)
        {
            return await _context.Categories
                .Where(x => x.StoreId == storeId && x.Slug == slug && (categoryId == 0 || x.CategoryId != categoryId))
                .AnyAsync();
        }

        public async Task<bool> ExistSlugInTenantAsync(long? exceptCategoryId, string slug)
        {
            return await _context.Categories
                .Where(x => x.Slug == slug && (exceptCategoryId == null || x.CategoryId != exceptCategoryId.Value))
                .AnyAsync();
        }

        public async Task<IDictionary<long, int>> CountProductsByCategoryAsync()
        {
            return await _context.Products
                .Where(x => x.CategoryId.HasValue)
                .GroupBy(x => x.CategoryId.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IDictionary<long, int>> CountActiveProductsByStoreAsync(long storeId)
        {
            return await _context.Products
                .Where(x => x.CategoryId.HasValue && x.StoreId == storeId && x.Status == 1)
                .GroupBy(x => x.CategoryId.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<CategoryModel> GetBySlugAndStoreAsync(long storeId, string slug)
        {
            var row = await _context.Categories
                .Where(x => x.StoreId == storeId && x.Slug == slug)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return CategoryDbMapper.ToModel(row);
        }

        public async Task<IList<CategoryModel>> GetAncestorChainAsync(long categoryId)
        {
            var chain = new List<CategoryModel>();
            var current = await _context.Categories.FindAsync(categoryId);
            if (current == null) return chain;

            chain.Add(CategoryDbMapper.ToModel(current));

            // Bounded by FR-004 max depth (5). Defensive cap of 32 to avoid infinite loop on corrupted data.
            for (var i = 0; i < 32; i++)
            {
                if (current.ParentId == null) break;
                var parent = await _context.Categories.FindAsync(current.ParentId.Value);
                if (parent == null) break;
                chain.Add(CategoryDbMapper.ToModel(parent));
                current = parent;
            }
            return chain;
        }

        public async Task<bool> ExistSiblingNameAsync(long? parentId, long? storeId, string name, long? excludeCategoryId)
        {
            var lowered = (name ?? string.Empty).ToLowerInvariant();
            return await _context.Categories
                .Where(x => x.ParentId == parentId
                    && x.StoreId == storeId
                    && x.Name.ToLower() == lowered
                    && (excludeCategoryId == null || x.CategoryId != excludeCategoryId.Value))
                .AnyAsync();
        }

        public async Task<bool> HasChildrenAsync(long categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.ParentId == categoryId);
        }

        public async Task<IList<CategoryModel>> ListByScopeAsync(long? storeId)
        {
            var rows = await _context.Categories
                .Where(x => x.StoreId == storeId)
                .ToListAsync();
            return rows.Select(CategoryDbMapper.ToModel).ToList();
        }

        public async Task<IList<CategoryModel>> GetDescendantsAsync(long categoryId)
        {
            var descendants = new List<CategoryModel>();
            var frontier = new List<long> { categoryId };

            // Bounded BFS — depth 5 means at most 4 hops down from the starting node.
            for (var i = 0; i < 32 && frontier.Count > 0; i++)
            {
                var nextLevel = await _context.Categories
                    .Where(x => x.ParentId.HasValue && frontier.Contains(x.ParentId.Value))
                    .ToListAsync();
                if (nextLevel.Count == 0) break;
                descendants.AddRange(nextLevel.Select(CategoryDbMapper.ToModel));
                frontier = nextLevel.Select(x => x.CategoryId).ToList();
            }
            return descendants;
        }

        public async Task UpdateManyAsync(IEnumerable<CategoryModel> models)
        {
            foreach (var model in models)
            {
                var row = await _context.Categories.FindAsync(model.CategoryId);
                if (row == null) continue;
                CategoryDbMapper.ToEntity(model, row);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(long? AppliedProductTypeId, long? OriginCategoryId)> GetAppliedProductTypeAsync(long categoryId)
        {
            var current = await _context.Categories.FindAsync(categoryId);
            if (current == null) return (null, null);

            for (var i = 0; i < 32; i++)
            {
                if (current.ProductTypeId.HasValue)
                    return (current.ProductTypeId, current.CategoryId);

                if (current.ParentId == null) break;

                var parent = await _context.Categories.FindAsync(current.ParentId.Value);
                if (parent == null) break;
                current = parent;
            }
            return (null, null);
        }

        public async Task UpdateProductTypeIdAsync(long categoryId, long? productTypeId)
        {
            var row = await _context.Categories.FindAsync(categoryId);
            if (row == null) return;
            row.ProductTypeId = productTypeId;
            await _context.SaveChangesAsync();
        }
    }
}
