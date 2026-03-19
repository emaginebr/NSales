using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra.Context;
using Lofn.Infra.Mappers;
using Lofn.Domain.Models;
using Lofn.DTO.Product;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Infra.Repository
{
    public class ProductRepository : IProductRepository<ProductModel>
    {
        private readonly LofnContext _context;
        private const int PAGE_SIZE = 15;
        private const int STATUS_ACTIVE = 1;

        public ProductRepository(LofnContext context)
        {
            _context = context;
        }

        public async Task<ProductModel> InsertAsync(ProductModel model)
        {
            var row = new Product();
            ProductDbMapper.ToEntity(model, row);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.ProductId = row.ProductId;
            return model;
        }

        public async Task<ProductModel> UpdateAsync(ProductModel model)
        {
            var row = await _context.Products.FindAsync(model.ProductId);
            ProductDbMapper.ToEntity(model, row);
            _context.Products.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<ProductModel> GetByIdAsync(long id)
        {
            var row = await _context.Products.FindAsync(id);
            if (row == null)
                return null;
            return ProductDbMapper.ToModel(row);
        }

        public async Task<ProductModel> GetBySlugAsync(string slug)
        {
            var row = await _context.Products
                .Where(x => x.Slug == slug)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return ProductDbMapper.ToModel(row);
        }

        public async Task<(IEnumerable<ProductModel> Items, int PageCount)> SearchAsync(long? storeId, long? userId, string keyword, bool active, int pageNum)
        {
            var q = _context.Products.AsQueryable();
            if (active)
            {
                q = q.Where(x => x.Status == STATUS_ACTIVE);
            }
            if (userId.HasValue && userId.Value > 0)
            {
                q = q.Where(x => x.UserId == userId.Value);
            }
            if (storeId.HasValue && storeId.Value > 0)
            {
                q = q.Where(x => x.StoreId == storeId);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                q = q.Where(x => x.Name.Contains(keyword));
            }
            var totalCount = await q.CountAsync();
            var pageCount = (int)Math.Ceiling((double)totalCount / PAGE_SIZE);
            var rows = await q.OrderBy(x => x.Frequency)
                .ThenBy(x => x.Price)
                .Skip((pageNum - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();
            return (rows.Select(ProductDbMapper.ToModel), pageCount);
        }

        public async Task<IEnumerable<ProductModel>> ListByStoreAsync(long storeId)
        {
            var rows = await _context.Products
                .Where(x => x.StoreId == storeId)
                .ToListAsync();
            return rows.Select(ProductDbMapper.ToModel);
        }

        public async Task<bool> ExistSlugAsync(long storeId, long productId, string slug)
        {
            return await _context.Products
                .Where(x => x.StoreId == storeId && x.Slug == slug && (productId == 0 || x.ProductId != productId))
                .AnyAsync();
        }

        public async Task<IEnumerable<ProductModel>> ListActiveByCategoryAndStoreAsync(long categoryId, long storeId)
        {
            var rows = await _context.Products
                .Where(x => x.StoreId == storeId && x.CategoryId == categoryId && x.Status == STATUS_ACTIVE)
                .OrderBy(x => x.Frequency)
                .ThenBy(x => x.Price)
                .ToListAsync();
            return rows.Select(ProductDbMapper.ToModel);
        }
    }
}
