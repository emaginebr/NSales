using Lofn.Domain.Models;
using Lofn.Infra.Context;
using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Infra.Repository
{
    public class ProductFilterValueRepository : IProductFilterValueRepository<ProductFilterValueModel>
    {
        private readonly LofnContext _context;

        public ProductFilterValueRepository(LofnContext context)
        {
            _context = context;
        }

        public async Task<IList<ProductFilterValueModel>> GetByProductAsync(long productId)
        {
            var rows = await _context.ProductFilterValues
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            var filterIds = rows.Select(r => r.FilterId).Distinct().ToList();
            var filters = await _context.ProductTypeFilters
                .Where(f => filterIds.Contains(f.FilterId))
                .ToDictionaryAsync(f => f.FilterId);

            return rows
                .Select(r => ProductFilterValueDbMapper.ToModel(r, filters.TryGetValue(r.FilterId, out var f) ? f : null))
                .ToList();
        }

        public async Task ReplaceForProductAsync(long productId, IList<ProductFilterValueModel> values)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var existing = await _context.ProductFilterValues
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            var incoming = values ?? new List<ProductFilterValueModel>();
            var incomingByFilter = incoming.ToDictionary(v => v.FilterId, v => v.Value);

            foreach (var row in existing)
            {
                if (incomingByFilter.TryGetValue(row.FilterId, out var newValue))
                {
                    if (row.Value != newValue)
                        row.Value = newValue;
                    incomingByFilter.Remove(row.FilterId);
                }
                else
                {
                    _context.ProductFilterValues.Remove(row);
                }
            }

            foreach (var (filterId, value) in incomingByFilter)
            {
                _context.ProductFilterValues.Add(new ProductFilterValue
                {
                    ProductId = productId,
                    FilterId = filterId,
                    Value = value
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
