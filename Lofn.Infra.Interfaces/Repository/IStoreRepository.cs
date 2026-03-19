using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.Repository
{
    public interface IStoreRepository<TModel> where TModel : class
    {
        Task<IEnumerable<TModel>> ListAllAsync();
        Task<IEnumerable<TModel>> ListActiveAsync();
        Task<IEnumerable<TModel>> ListByOwnerAsync(long ownerId);
        Task<TModel> GetByIdAsync(long id);
        Task<TModel> GetBySlugAsync(string slug);
        Task<bool> ExistSlugAsync(long storeId, string slug);
        Task<TModel> InsertAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task DeleteAsync(long id);
    }
}
