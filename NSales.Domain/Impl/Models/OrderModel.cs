using Core.Domain.Repository;
using Core.Domain;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using NSales.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Impl.Models
{
    public class OrderModel : IOrderModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository<IOrderModel, IOrderDomainFactory> _repositoryOrder;

        public OrderModel(
            IUnitOfWork unitOfWork, 
            IOrderRepository<IOrderModel, IOrderDomainFactory> repositoryOrder
        )
        {
            _unitOfWork = unitOfWork;
            _repositoryOrder = repositoryOrder;
        }

        public long OrderId { get; set; }
        public long? NetworkId { get; set; }
        public long UserId { get; set; }
        public long? SellerId { get; set; }
        public OrderStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string StripeId { get; set; }

        /*
        public IUserModel GetUser(IUserDomainFactory factory)
        {
            if (UserId <= 0)
            {
                return null;
            }
            return factory.BuildUserModel().GetById(UserId, factory);
        }

        public IUserModel GetSeller(IUserDomainFactory factory)
        {
            if (!SellerId.HasValue || SellerId.Value <= 0)
            {
                return null;
            }
            return factory.BuildUserModel().GetById(SellerId.Value, factory);
        }
        */

        public IList<IOrderItemModel> ListItems(IOrderItemDomainFactory factory)
        {
            if (OrderId <= 0)
            {
                return new List<IOrderItemModel>();
            }
            return factory.BuildOrderItemModel().ListItems(OrderId, factory);
        }

        public IOrderModel Insert(IOrderDomainFactory factory)
        {
            return _repositoryOrder.Insert(this, factory);
        }

        public IOrderModel Update(IOrderDomainFactory factory)
        {
            return _repositoryOrder.Update(this, factory);
        }

        public IEnumerable<IOrderModel> List(long networkId, long userId, OrderStatusEnum? status, IOrderDomainFactory factory)
        {
            return _repositoryOrder.List(networkId, userId, (status.HasValue ? (int) status : 0), factory);
        }

        public IOrderModel GetById(long id, IOrderDomainFactory factory)
        {
            return _repositoryOrder.GetById(id, factory);
        }

        public IOrderModel Get(long productId, long userId, long? sellerId, OrderStatusEnum status, IOrderDomainFactory factory)
        {
            return _repositoryOrder.Get(productId, userId, sellerId, (int)status, factory);
        }

        /*
        public IOrderModel GetByStripeId(string stripeId, IOrderDomainFactory factory)
        {
            return _repositoryOrder.GetByStripeId(stripeId, factory);
        }
        */

        public IEnumerable<IOrderModel> Search(long networkId, long? userId, long? sellerId, int pageNum, out int pageCount, IOrderDomainFactory factory)
        {
            return _repositoryOrder.Search(networkId, userId, sellerId, pageNum, out pageCount, factory);
        }
    }
}
