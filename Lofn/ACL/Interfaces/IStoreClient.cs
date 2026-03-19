using Lofn.DTO.Store;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.ACL.Interfaces
{
    public interface IStoreClient
    {
        Task<IList<StoreInfo>> ListAsync();
        Task<IList<StoreInfo>> ListActiveAsync();
        Task<StoreInfo> GetBySlugAsync(string storeSlug);
        Task<StoreInfo> GetByIdAsync(long storeId);
        Task<StoreInfo> InsertAsync(StoreInsertInfo store);
        Task<StoreInfo> UpdateAsync(StoreUpdateInfo store);
        Task DeleteAsync(long storeId);
    }
}
