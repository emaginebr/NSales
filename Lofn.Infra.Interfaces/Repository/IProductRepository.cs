using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.Repository
{
    public interface IProductRepository<TModel> where TModel : class
    {
        Task<(IEnumerable<TModel> Items, int PageCount)> SearchAsync(long? storeId, long? userId, string keyword, bool active, int pageNum);
        Task<IEnumerable<TModel>> ListByStoreAsync(long storeId);
        Task<TModel> GetByIdAsync(long id);
        Task<TModel> GetBySlugAsync(string slug);
        Task<TModel> InsertAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task<bool> ExistSlugAsync(long storeId, long productId, string slug);
        Task<IEnumerable<TModel>> ListActiveByCategoryAndStoreAsync(long categoryId, long storeId);
    }
}
