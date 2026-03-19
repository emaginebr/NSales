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
    public class StoreRepository : IStoreRepository<StoreModel>
    {
        private readonly LofnContext _context;

        public StoreRepository(LofnContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StoreModel>> ListAllAsync()
        {
            var rows = await _context.Stores.OrderBy(x => x.Name).ToListAsync();
            return rows.Select(StoreDbMapper.ToModel);
        }

        public async Task<IEnumerable<StoreModel>> ListActiveAsync()
        {
            var rows = await _context.Stores
                .Where(x => x.Status == 1)
                .OrderBy(x => x.Name)
                .ToListAsync();
            return rows.Select(StoreDbMapper.ToModel);
        }

        public async Task<IEnumerable<StoreModel>> ListByOwnerAsync(long ownerId)
        {
            var rows = await _context.Stores
                .Where(x => x.OwnerId == ownerId)
                .OrderBy(x => x.Name)
                .ToListAsync();
            return rows.Select(StoreDbMapper.ToModel);
        }

        public async Task<StoreModel> GetByIdAsync(long id)
        {
            var row = await _context.Stores.FindAsync(id);
            if (row == null)
                return null;
            return StoreDbMapper.ToModel(row);
        }

        public async Task<StoreModel> GetBySlugAsync(string slug)
        {
            var row = await _context.Stores
                .Where(x => x.Slug == slug)
                .FirstOrDefaultAsync();
            if (row == null)
                return null;
            return StoreDbMapper.ToModel(row);
        }

        public async Task<bool> ExistSlugAsync(long storeId, string slug)
        {
            return await _context.Stores
                .Where(x => x.Slug == slug && (storeId == 0 || x.StoreId != storeId))
                .AnyAsync();
        }

        public async Task<StoreModel> InsertAsync(StoreModel model)
        {
            var row = new Store();
            StoreDbMapper.ToEntity(model, row);
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.StoreId = row.StoreId;
            return model;
        }

        public async Task<StoreModel> UpdateAsync(StoreModel model)
        {
            var row = await _context.Stores.FindAsync(model.StoreId);
            StoreDbMapper.ToEntity(model, row);
            _context.Stores.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task DeleteAsync(long id)
        {
            var row = await _context.Stores.FindAsync(id);
            if (row != null)
            {
                _context.Stores.Remove(row);
                await _context.SaveChangesAsync();
            }
        }
    }
}
