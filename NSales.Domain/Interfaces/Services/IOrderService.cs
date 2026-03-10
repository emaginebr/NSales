using NSales.Domain.Interfaces.Models;
using NSales.DTO.Order;
using NSales.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Interfaces.Services
{
    public interface IOrderService
    {
        IList<IOrderModel> List(long networkId, long userId, OrderStatusEnum? status);
        Task<OrderListPagedResult> Search(long networkId, long? userId, long? sellerId, int pageNum);
        IOrderModel GetById(long orderId);
        IOrderModel Get(long productId, long userId, long? sellerId, OrderStatusEnum status);
       // IOrderModel GetByStripeId(string stripeId);
        Task<OrderInfo> GetOrderInfo(IOrderModel order);
        IOrderModel Insert(OrderInfo order);
        IOrderModel Update(OrderInfo order);
    }
}
