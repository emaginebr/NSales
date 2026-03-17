using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra.Context;
using Lofn.Domain.Models;
using Lofn.DTO.Order;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Infra.Repository
{
    public class OrderRepository : IOrderRepository<OrderModel>
    {
        private const int PAGE_SIZE = 15;
        private readonly LofnContext _context;

        public OrderRepository(LofnContext context)
        {
            _context = context;
        }

        private static OrderModel DbToModel(Order row)
        {
            return new OrderModel
            {
                OrderId = row.OrderId,
                NetworkId = row.NetworkId,
                UserId = row.UserId,
                SellerId = row.SellerId,
                CreatedAt = row.CreatedAt,
                UpdatedAt = row.UpdatedAt,
                Status = (OrderStatusEnum)row.Status
            };
        }

        private static void ModelToDb(OrderModel md, Order row)
        {
            row.OrderId = md.OrderId;
            row.NetworkId = md.NetworkId;
            row.UserId = md.UserId;
            row.SellerId = md.SellerId;
            row.CreatedAt = md.CreatedAt;
            row.UpdatedAt = md.UpdatedAt;
            row.Status = (int)md.Status;
        }

        public async Task<(IEnumerable<OrderModel> Items, int PageCount)> SearchAsync(long networkId, long? userId, long? sellerId, int pageNum)
        {
            var q = _context.Orders.Where(x => x.NetworkId == networkId);
            if (userId.HasValue && userId.Value > 0)
            {
                q = q.Where(x => x.UserId == userId.Value);
            }
            if (sellerId.HasValue && sellerId.Value > 0)
            {
                q = q.Where(x => x.SellerId == sellerId.Value);
            }
            var totalCount = await q.CountAsync();
            var pageCount = (int)Math.Ceiling((double)totalCount / PAGE_SIZE);
            var rows = await q
                .Skip((pageNum - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();
            return (rows.Select(DbToModel), pageCount);
        }

        public async Task<OrderModel> InsertAsync(OrderModel model)
        {
            var row = new Order();
            ModelToDb(model, row);
            row.CreatedAt = DateTime.Now;
            row.UpdatedAt = DateTime.Now;
            _context.Add(row);
            await _context.SaveChangesAsync();
            model.OrderId = row.OrderId;
            return model;
        }

        public async Task<OrderModel> UpdateAsync(OrderModel model)
        {
            var row = await _context.Orders.FindAsync(model.OrderId);
            ModelToDb(model, row);
            row.UpdatedAt = DateTime.Now;
            _context.Orders.Update(row);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<IEnumerable<OrderModel>> ListAsync(long networkId, long userId, int status)
        {
            var q = _context.Orders.AsQueryable();
            if (networkId > 0)
            {
                q = q.Where(x => x.NetworkId == networkId);
            }
            if (userId > 0)
            {
                q = q.Where(x => x.UserId == userId);
            }
            if (status > 0)
            {
                q = q.Where(x => x.Status == status);
            }
            var rows = await q.ToListAsync();
            return rows.Select(DbToModel);
        }

        public async Task<OrderModel> GetByIdAsync(long id)
        {
            var row = await _context.Orders.FindAsync(id);
            if (row == null)
                return null;
            return DbToModel(row);
        }

        public async Task<OrderModel> GetAsync(long productId, long userId, long? sellerId, int status)
        {
            var q = _context.Orders
                .Where(x => x.OrderItems.Any(y => y.ProductId == productId)
                    && x.UserId == userId && x.Status == status);
            if (sellerId.HasValue && sellerId.Value >= 0)
            {
                q = q.Where(x => x.SellerId == sellerId.Value);
            }
            var row = await q.FirstOrDefaultAsync();
            if (row == null)
                return null;
            return DbToModel(row);
        }
    }
}
