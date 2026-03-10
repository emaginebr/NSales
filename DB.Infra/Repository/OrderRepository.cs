using Core.Domain.Repository;
using DB.Infra.Context;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using NSales.DTO.Order;
using NoobsMuc.Coinmarketcap.Client;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Infra.Repository
{
    public class OrderRepository : IOrderRepository<IOrderModel, IOrderDomainFactory>
    {
        private const int PAGE_SIZE = 15;

        private NSalesContext _ccsContext;

        public OrderRepository(NSalesContext ccsContext)
        {
            _ccsContext = ccsContext;
        }

        private IOrderModel DbToModel(IOrderDomainFactory factory, Order row)
        {
            var md = factory.BuildOrderModel();
            md.OrderId = row.OrderId;
            md.NetworkId = row.NetworkId;
            md.UserId = row.UserId;
            md.SellerId = row.SellerId;
            md.CreatedAt = row.CreatedAt;
            md.UpdatedAt = row.UpdatedAt;
            md.Status = (OrderStatusEnum) row.Status;
            return md;
        }

        private void ModelToDb(IOrderModel md, Order row)
        {
            row.OrderId = md.OrderId;
            row.NetworkId = md.NetworkId;
            row.UserId = md.UserId;
            row.SellerId = md.SellerId;
            row.CreatedAt = md.CreatedAt;
            row.UpdatedAt = md.UpdatedAt;
            row.Status = (int)md.Status;
            //row.StripeId = md.StripeId;
        }

        public IEnumerable<IOrderModel> Search(long networkId, long? userId, long? sellerId, int pageNum, out int pageCount, IOrderDomainFactory factory)
        {
            var q = _ccsContext.Orders.Where(x => x.NetworkId == networkId);
            if (userId.HasValue && userId.Value > 0)
            {
                q = q.Where(x => x.UserId == userId.Value);
            }
            if (sellerId.HasValue && sellerId.Value > 0)
            {
                q = q.Where(x => x.SellerId == sellerId.Value);
            }
            var pages = (double)q.Count() / (double)PAGE_SIZE;
            pageCount = Convert.ToInt32(Math.Ceiling(pages));
            var rows = q.Skip((pageNum - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();
            return rows.Select(x => DbToModel(factory, x));
        }

        public IOrderModel Insert(IOrderModel model, IOrderDomainFactory factory)
        {
            var row = new Order();
            ModelToDb(model, row);
            row.CreatedAt = DateTime.Now;
            row.UpdatedAt = DateTime.Now;
            _ccsContext.Add(row);
            _ccsContext.SaveChanges();
            model.OrderId = row.OrderId;
            return model;
        }

        public IOrderModel Update(IOrderModel model, IOrderDomainFactory factory)
        {
            var row = _ccsContext.Orders.Find(model.OrderId);
            ModelToDb(model, row);
            row.UpdatedAt = DateTime.Now;
            _ccsContext.Orders.Update(row);
            _ccsContext.SaveChanges();
            return model;
        }

        public IEnumerable<IOrderModel> List(long networkId, long userId, int status, IOrderDomainFactory factory)
        {
            var q = _ccsContext.Orders;
            if (networkId > 0)
            {
                q.Where(x => x.NetworkId == networkId);
            }
            if (userId > 0) {
                q.Where(x => x.UserId == userId);
            }
            if (status > 0) {
                q.Where(x => x.Status == status);
            }
            return q.ToList().Select(x => DbToModel(factory, x));
        }

        public IOrderModel GetById(long id, IOrderDomainFactory factory)
        {
            var row = _ccsContext.Orders.Find(id);
            if (row == null)
                return null;
            return DbToModel(factory, row);
        }

        public IOrderModel Get(long productId, long userId, long? sellerId, int status, IOrderDomainFactory factory)
        {
            var q = _ccsContext.Orders
                .Where(x => x.OrderItems.Where(y => y.ProductId == productId).Any()
                    && x.UserId == userId && x.Status == status);
            if (sellerId.HasValue && sellerId.Value >= 0)
            {
                q = q.Where(x => x.SellerId == sellerId.Value);
            }
            var row = q.FirstOrDefault();
            if (row == null)
                return null;
            return DbToModel(factory, row);
        }

        /*
        public IOrderModel GetByStripeId(string stripeId, IOrderDomainFactory factory)
        {
            var row = _ccsContext.Orders
                //.Where(x => x.StripeId == stripeId)
                .FirstOrDefault();
            if (row == null)
                return null;
            return DbToModel(factory, row);
        }
        */
    }
}
