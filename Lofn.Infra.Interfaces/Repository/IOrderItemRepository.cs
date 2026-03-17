using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.Repository
{
    public interface IOrderItemRepository<TModel> where TModel : class
    {
        Task<IEnumerable<TModel>> ListByOrderAsync(long orderId);
        Task<TModel> InsertAsync(TModel model);
    }
}
