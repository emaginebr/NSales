using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Repository
{
    public interface IOrderItemRepository<TModel, TFactory>
    {
        IEnumerable<TModel> ListByOrder(long orderId, TFactory factory);
        TModel Insert(TModel model, TFactory factory);
    }
}
