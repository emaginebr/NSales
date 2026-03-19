using Lofn.DTO.Order;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.ACL.Interfaces
{
    public interface IOrderClient
    {
        Task<OrderListPagedResult> SearchAsync(OrderSearchParam param);
        Task<IList<OrderInfo>> ListAsync(OrderParam param);
        Task<OrderInfo> GetByIdAsync(long orderId);
        Task<OrderInfo> UpdateAsync(OrderInfo order);
    }
}
