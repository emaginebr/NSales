using Core.Domain.Repository;
using Core.Domain;
using NSales.Domain.Impl.Models;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Impl.Factory
{
    public class OrderItemDomainFactory : IOrderItemDomainFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderItemRepository<IOrderItemModel, IOrderItemDomainFactory> _repositoryOrder;

        public OrderItemDomainFactory(IUnitOfWork unitOfWork, IOrderItemRepository<IOrderItemModel, IOrderItemDomainFactory> repositoryOrder)
        {
            _unitOfWork = unitOfWork;
            _repositoryOrder = repositoryOrder;
        }

        public IOrderItemModel BuildOrderItemModel()
        {
            return new OrderItemModel(_unitOfWork, _repositoryOrder);
        }
    }
}
