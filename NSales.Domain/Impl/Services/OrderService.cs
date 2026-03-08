using NAuth.ACL;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using NSales.Domain.Interfaces.Services;
using NSales.DTO.Order;
using NSales.DTO.Product;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Impl.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUserClient _userClient;
        private readonly IOrderDomainFactory _orderFactory;
        private readonly IOrderItemDomainFactory _itemFactory;
        private readonly IProductService _productService;
        private readonly IProductDomainFactory _productFactory;

        public OrderService(
            IUserClient userClient,
            IOrderDomainFactory orderFactory, 
            IOrderItemDomainFactory itemFactory,
            IProductService productService,
            IProductDomainFactory productFactory
        )
        {
            _userClient = userClient;
            _orderFactory = orderFactory;
            _itemFactory = itemFactory;
            _productService = productService;
            _productFactory = productFactory;
        }

        public IOrderModel Insert(OrderInfo order)
        {
            if (!(order.NetworkId > 0))
            {
                throw new Exception("Network is empty");
            }
            if (!(order.UserId > 0))
            {
                throw new Exception("User is empty");
            }
            if (order.Items == null || order.Items.Count() <= 0)
            {
                throw new Exception("Order is empty");
            }

            var model = _orderFactory.BuildOrderModel();
            model.NetworkId = order.NetworkId;
            model.UserId = order.UserId;
            model.SellerId = order.SellerId;
            model.Status = order.Status;

            var newOrder = model.Insert(_orderFactory);

            foreach (var item in order.Items)
            {
                var mdItem = _itemFactory.BuildOrderItemModel();
                mdItem.OrderId = newOrder.OrderId;
                mdItem.ProductId = item.ProductId;
                mdItem.Quantity = item.Quantity;

                mdItem.Insert(_itemFactory);
            }
            return newOrder;
        }

        public IOrderModel Update(OrderInfo order)
        {
            if (!(order.OrderId > 0))
            {
                throw new Exception("Order ID is empty");
            }
            var model = _orderFactory.BuildOrderModel().GetById(order.OrderId, _orderFactory);

            model.Status = order.Status;

            return model.Update(_orderFactory);
        }

        public IOrderModel GetById(long orderId)
        {
            return _orderFactory.BuildOrderModel().GetById(orderId, _orderFactory);
        }

        public IOrderModel Get(long productId, long userId, long? sellerId, OrderStatusEnum status)
        {
            return _orderFactory.BuildOrderModel().Get(productId, userId, sellerId, status, _orderFactory);
        }

        public async Task<OrderInfo> GetOrderInfo(IOrderModel order)
        {
            return new OrderInfo
            {
                OrderId = order.OrderId,
                NetworkId = order.NetworkId,
                UserId = order.UserId,
                SellerId = order.SellerId,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                User = await _userClient.GetByIdAsync(order.UserId),
                Seller = order.SellerId.HasValue ? await _userClient.GetByIdAsync(order.SellerId.Value) : null,
                Items = await GetOrderItemInfosAsync(order.ListItems(_itemFactory))
            };
        }

        private async Task<List<OrderItemInfo>> GetOrderItemInfosAsync(IList<IOrderItemModel> items)
        {
            var result = new List<OrderItemInfo>();
            foreach (var x in items)
            {
                var product = await _productService.GetProductInfo(x.GetProduct(_productFactory));
                result.Add(new OrderItemInfo
                {
                    ItemId = x.ItemId,
                    OrderId = x.OrderId,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Product = product
                });
            }
            return result;
        }

        public IList<IOrderModel> List(long networkId, long userId, OrderStatusEnum? status)
        {
            return _orderFactory.BuildOrderModel().List(networkId, userId, status, _orderFactory).ToList();
        }

        /*
        public IOrderModel GetByStripeId(string stripeId)
        {
            return _orderFactory.BuildOrderModel().GetByStripeId(stripeId, _orderFactory);
        }
        */

        public async Task<OrderListPagedResult> Search(long networkId, long? userId, long? sellerId, int pageNum)
        {
            var model = _orderFactory.BuildOrderModel();
            int pageCount = 0;
            var orderModels = model.Search(networkId, userId, sellerId, pageNum, out pageCount, _orderFactory);
            var orders = await Task.WhenAll(orderModels.Select(async x => await GetOrderInfo(x)));
            return new OrderListPagedResult
            {
                Sucesso = true,
                Orders = orders.ToList(),
                PageNum = pageNum,
                PageCount = pageCount
            };
        }
    }
}
