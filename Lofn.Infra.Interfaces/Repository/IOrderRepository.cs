using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.Repository
{
    public interface IOrderRepository<TModel> where TModel : class
    {
        Task<(IEnumerable<TModel> Items, int PageCount)> SearchAsync(long networkId, long? userId, long? sellerId, int pageNum);
        Task<IEnumerable<TModel>> ListAsync(long networkId, long userId, int status);
        Task<TModel> GetByIdAsync(long id);
        Task<TModel> GetAsync(long productId, long userId, long? sellerId, int status);
        Task<TModel> InsertAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
    }
}
