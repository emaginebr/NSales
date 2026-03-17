using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra.Context;
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

        private static ProductModel DbToModel(Product row)
        {
            return new ProductModel
            {
                ProductId = row.ProductId,
                UserId = row.UserId,
                NetworkId = row.NetworkId,
                Name = row.Name,
                Slug = row.Slug,
                Image = row.Image,
                Description = row.Description,
                Price = row.Price,
                Frequency = row.Frequency,
                Limit = row.Limit,
                Status = (ProductStatusEnum)row.Status
            };
        }

        private static void ModelToDb(ProductModel md, Product row)
        {
            row.ProductId = md.ProductId;
            row.UserId = md.UserId;
            row.NetworkId = md.NetworkId;
            row.Name = md.Name;
            row.Slug = md.Slug;
            row.Image = md.Image;
            row.Description = md.Description;
            row.Price = md.Price;
            row.Frequency = md.Frequency;
            row.Limit = md.Limit;
            row.Status = (int)md.Status;
        }

        public async Task<ProductModel> InsertAsync(ProductModel model)
        {
            var row = new Product();
            ModelToDb(model, row);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.ProductId = row.ProductId;
            return model;
        }

        public async Task<ProductModel> UpdateAsync(ProductModel model)
        {
            var row = await _context.Products.FindAsync(model.ProductId);
            ModelToDb(model, row);
            _context.Products.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<ProductModel> GetByIdAsync(long id)
        {
            var row = await _context.Products.FindAsync(id);
            if (row == null)
                return null;
            return DbToModel(row);
        }

        public async Task<ProductModel> GetBySlugAsync(string slug)
        {
            var row = await _context.Products
                .Where(x => x.Slug == slug)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return DbToModel(row);
        }

        public async Task<(IEnumerable<ProductModel> Items, int PageCount)> SearchAsync(long? networkId, long? userId, string keyword, bool active, int pageNum)
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
            if (networkId.HasValue && networkId.Value > 0)
            {
                q = q.Where(x => x.NetworkId == networkId);
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
            return (rows.Select(DbToModel), pageCount);
        }

        public async Task<IEnumerable<ProductModel>> ListByNetworkAsync(long networkId)
        {
            var rows = await _context.Products
                .Where(x => x.NetworkId == networkId)
                .ToListAsync();
            return rows.Select(DbToModel);
        }

        public async Task<bool> ExistSlugAsync(long productId, string slug)
        {
            return await _context.Products
                .Where(x => x.Slug == slug && (productId == 0 || x.ProductId != productId))
                .AnyAsync();
        }
    }
}
