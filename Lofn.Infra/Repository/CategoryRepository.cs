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
    }
}
