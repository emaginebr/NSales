using Core.Domain.Repository;
using DB.Infra.Context;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using NSales.DTO.Order;
using NoobsMuc.Coinmarketcap.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Infra.Repository
{
    public class OrderItemRepository : IOrderItemRepository<IOrderItemModel, IOrderItemDomainFactory>
    {
        private NSalesContext _ccsContext;

        public OrderItemRepository(NSalesContext ccsContext)
        {
            _ccsContext = ccsContext;
        }

        private IOrderItemModel DbToModel(IOrderItemDomainFactory factory, OrderItem row)
        {
            var md = factory.BuildOrderItemModel();
            md.ItemId = row.ItemId;
            md.OrderId = row.OrderId;
            md.ProductId = row.ProductId;
            md.Quantity = row.Quantity;
            return md;
        }

        private void ModelToDb(IOrderItemModel md, OrderItem row)
        {
            row.ItemId = md.ItemId;
            row.OrderId = md.OrderId;
            row.ProductId = md.ProductId;
            row.Quantity = md.Quantity;
        }

        public IOrderItemModel Insert(IOrderItemModel model, IOrderItemDomainFactory factory)
        {
            var row = new OrderItem();
            ModelToDb(model, row);
            _ccsContext.Add(row);
            _ccsContext.SaveChanges();
            model.ItemId = row.ItemId;
            return model;
        }

        public IEnumerable<IOrderItemModel> ListByOrder(long orderId, IOrderItemDomainFactory factory)
        {
            return _ccsContext.OrderItems
                .Where(x => x.OrderId == orderId)
                .ToList()
                .Select(x => DbToModel(factory, x));
        }
    }
}
