using Lofn.Domain.Models;
using Lofn.DTO.Store;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Domain.Interfaces
{
    public interface IStoreService
    {
        Task<IList<StoreInfo>> ListAllAsync();
        Task<IList<StoreInfo>> ListActiveAsync();
        Task<IList<StoreInfo>> ListByOwnerAsync(long ownerId);
        Task<StoreModel> GetByIdAsync(long storeId);
        Task<StoreModel> GetBySlugAsync(string slug);
        Task<StoreModel> InsertAsync(StoreInsertInfo store, long ownerId);
        Task<StoreModel> UpdateAsync(StoreUpdateInfo store, long ownerId);
        Task<StoreModel> UploadLogoAsync(long storeId, string logo, long ownerId);
        Task DeleteAsync(long storeId);
    }
}
