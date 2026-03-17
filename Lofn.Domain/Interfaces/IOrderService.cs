using Lofn.Domain.Models;
using Lofn.DTO.Order;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Domain.Interfaces
{
    public interface IOrderService
    {
        Task<IList<OrderModel>> ListAsync(long networkId, long userId, OrderStatusEnum? status);
        Task<OrderListPagedResult> SearchAsync(long networkId, long? userId, long? sellerId, int pageNum, string token);
        Task<OrderModel> GetByIdAsync(long orderId);
        Task<OrderModel> GetAsync(long productId, long userId, long? sellerId, OrderStatusEnum status);
        Task<OrderInfo> GetOrderInfoAsync(OrderModel order, string token);
        Task<OrderModel> InsertAsync(OrderInfo order);
        Task<OrderModel> UpdateAsync(OrderInfo order);
    }
}
