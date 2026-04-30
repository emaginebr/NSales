using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.Repository
{
    public interface IProductFilterValueRepository<TModel> where TModel : class
    {
        Task<IList<TModel>> GetByProductAsync(long productId);
        Task ReplaceForProductAsync(long productId, IList<TModel> values);
    }
}
