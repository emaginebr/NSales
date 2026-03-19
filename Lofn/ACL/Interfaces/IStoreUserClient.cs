using Lofn.DTO.Store;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.ACL.Interfaces
{
    public interface IStoreUserClient
    {
        Task<IList<StoreUserInfo>> ListAsync(string storeSlug);
        Task<StoreUserInfo> InsertAsync(string storeSlug, StoreUserInsertInfo storeUser);
        Task DeleteAsync(string storeSlug, long storeUserId);
    }
}
