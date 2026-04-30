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

        public async Task<(IEnumerable<ProductModel> Items, int PageCount)> ListActiveByStoreAsync(long storeId, long? categoryId, int pageNum)
        {
            var q = _context.Products
                .Where(x => x.StoreId == storeId && x.Status == STATUS_ACTIVE && x.Store.Status == STATUS_ACTIVE);

            if (categoryId.HasValue)
                q = q.Where(x => x.CategoryId == categoryId.Value);

            var totalCount = await q.CountAsync();
            var pageCount = (int)Math.Ceiling((double)totalCount / PAGE_SIZE);

            var rows = await q
                .OrderBy(x => x.Name)
                .Skip((pageNum - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();

            return (rows.Select(ProductDbMapper.ToModel), pageCount);
        }

        public async Task<IEnumerable<ProductModel>> ListFeaturedByStoreAsync(long storeId, int limit)
        {
            var rows = await _context.Products
                .Where(x => x.StoreId == storeId && x.Status == STATUS_ACTIVE && x.Featured && x.Store.Status == STATUS_ACTIVE)
                .OrderBy(x => x.Name)
                .Take(limit)
                .ToListAsync();
            return rows.Select(ProductDbMapper.ToModel);
        }

        public async Task<(IList<ProductModel> Items, int PageCount, int TotalItems)> SearchByFilterValuesAsync(
            long? storeId,
            long categoryId,
            IList<long> categoryIdsRollup,
            IList<(long FilterId, string Value)> filters,
            int pageNum,
            double? priceMin = null,
            double? priceMax = null,
            bool onlyOnSale = false)
        {
            var rollup = categoryIdsRollup ?? new List<long> { categoryId };
            var q = BuildFilteredProductQuery(storeId, rollup, filters, priceMin, priceMax, onlyOnSale);

            var totalCount = await q.CountAsync();
            var pageCount = (int)Math.Ceiling((double)totalCount / PAGE_SIZE);

            var rows = await q
                .OrderBy(p => p.ProductId)
                .Skip((pageNum - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();

            IList<ProductModel> items = rows.Select(ProductDbMapper.ToModel).ToList();
            return (items, pageCount, totalCount);
        }

        public async Task<IDictionary<long, IList<string>>> GetAvailableFilterValuesAsync(
            long? storeId,
            IList<long> categoryIdsRollup,
            IList<(long FilterId, string Value)> filters,
            IList<long> filterIdsToReturn,
            double? priceMin = null,
            double? priceMax = null,
            bool onlyOnSale = false)
        {
            var result = new Dictionary<long, IList<string>>();
            if (filterIdsToReturn == null || filterIdsToReturn.Count == 0)
                return result;

            var rollup = categoryIdsRollup ?? new List<long>();
            var productQuery = BuildFilteredProductQuery(storeId, rollup, filters, priceMin, priceMax, onlyOnSale)
                .Select(p => p.ProductId);

            var grouped = await _context.ProductFilterValues
                .Where(pfv => filterIdsToReturn.Contains(pfv.FilterId))
                .Where(pfv => productQuery.Contains(pfv.ProductId))
                .GroupBy(pfv => pfv.FilterId)
                .Select(g => new { FilterId = g.Key, Values = g.Select(pfv => pfv.Value).Distinct().ToList() })
                .ToListAsync();

            foreach (var entry in grouped)
                result[entry.FilterId] = entry.Values;

            return result;
        }

        private IQueryable<Product> BuildFilteredProductQuery(
            long? storeId,
            IList<long> rollup,
            IList<(long FilterId, string Value)> filters,
            double? priceMin,
            double? priceMax,
            bool onlyOnSale)
        {
            var q = _context.Products
                .Where(p => p.Status == STATUS_ACTIVE
                    && p.Store.Status == STATUS_ACTIVE
                    && p.CategoryId.HasValue
                    && rollup.Contains(p.CategoryId.Value));

            if (storeId.HasValue && storeId.Value > 0)
                q = q.Where(p => p.StoreId == storeId.Value);

            if (priceMin.HasValue)
                q = q.Where(p => p.Price >= priceMin.Value);

            if (priceMax.HasValue)
                q = q.Where(p => p.Price <= priceMax.Value);

            if (onlyOnSale)
                q = q.Where(p => p.Discount > 0);

            if (filters != null)
            {
                foreach (var (filterId, value) in filters)
                {
                    var fId = filterId;
                    var v = value;
                    q = q.Where(p => _context.ProductFilterValues
                        .Any(pfv => pfv.ProductId == p.ProductId && pfv.FilterId == fId && pfv.Value == v));
                }
            }

            return q;
        }
    }
}
