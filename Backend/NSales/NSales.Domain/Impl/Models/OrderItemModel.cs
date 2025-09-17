using Core.Domain.Repository;
using Core.Domain;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Impl.Models
{

    public class OrderItemModel : IOrderItemModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderItemRepository<IOrderItemModel, IOrderItemDomainFactory> _repositoryItem;

        public OrderItemModel(
            IUnitOfWork unitOfWork,
            IOrderItemRepository<IOrderItemModel, IOrderItemDomainFactory> repositoryItem
        )
        {
            _unitOfWork = unitOfWork;
            _repositoryItem = repositoryItem;
        }

        public long ItemId {  get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }

        public IProductModel GetProduct(IProductDomainFactory factory)
        {
            if (ProductId <= 0)
            {
                return null;
            }
            return factory.BuildProductModel().GetById(ProductId, factory);
        }

        public IOrderItemModel Insert(IOrderItemDomainFactory factory)
        {
            return _repositoryItem.Insert(this, factory);
        }

        public IList<IOrderItemModel> ListItems(long orderId, IOrderItemDomainFactory factory)
        {
            return _repositoryItem.ListByOrder(orderId, factory).ToList();
        }
    }
}
