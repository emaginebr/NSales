using NSales.Domain.Interfaces.Factory;
using NSales.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Interfaces.Models
{
    public interface IOrderModel
    {
        long OrderId { get; set; }
        long? NetworkId { get; set; }
        long UserId { get; set; }
        long? SellerId { get; set; }
        string StripeId { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        OrderStatusEnum Status { get; set; }

        /*
        IUserModel GetUser(IUserDomainFactory factory);
        IUserModel GetSeller(IUserDomainFactory factory);
        */
        IList<IOrderItemModel> ListItems(IOrderItemDomainFactory factory);

        IEnumerable<IOrderModel> List(long networkId, long userId, OrderStatusEnum? status, IOrderDomainFactory factory);
        IEnumerable<IOrderModel> Search(long networkId, long? userId, long? sellerId, int pageNum, out int pageCount, IOrderDomainFactory factory);
        IOrderModel GetById(long id, IOrderDomainFactory factory);
        IOrderModel Get(long productId, long userId, long? sellerId, OrderStatusEnum status, IOrderDomainFactory factory);
        //IOrderModel GetByStripeId(string stripeId, IOrderDomainFactory factory);
        IOrderModel Insert(IOrderDomainFactory factory);
        IOrderModel Update(IOrderDomainFactory factory);
    }
}
