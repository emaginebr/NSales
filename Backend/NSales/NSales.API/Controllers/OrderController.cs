using NSales.Domain.Impl.Services;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Services;
using NSales.DTO.Order;
using NSales.DTO.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using NAuth.ACL;
using System.Collections.Generic;

namespace NSales.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController: ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IProductDomainFactory _productFactory;
        //private readonly ISubscriptionService _subscriptionService;
        //private readonly INetworkService _networkService;
        //private readonly IStripeService _stripeService;

        public OrderController(
            IUserClient userClient, 
            IOrderService orderService,
            IProductService productService,
            IProductDomainFactory productFactory
            //ISubscriptionService subscriptionService,
            //INetworkService networkService,
            //IStripeService stripeService,
        )
        {
            _userClient = userClient;
            _orderService = orderService;
            _productService = productService;
            _productFactory = productFactory;
            //_subscriptionService = subscriptionService;
            //_networkService = networkService;
            //_stripeService = stripeService;
        }

        /*
        [Authorize]
        [HttpGet("createSubscription/{productSlug}")]
        public async Task<ActionResult<SubscriptionResult>> CreateSubscription(
            string productSlug, 
            [FromQuery] string networkSlug, 
            [FromQuery] string sellerSlug
        )
        {
            try
            {
                var userSession = _userService.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }

                var product = _productService.GetBySlug(productSlug);
                if (product == null)
                {
                    throw new Exception("Product not found");
                }
                long? networkId = null;
                if (!string.IsNullOrEmpty(networkSlug))
                {
                    var network = _networkService.GetBySlug(networkSlug);
                    if (network != null)
                    {
                        networkId = network.NetworkId;
                    }
                }
                long? sellerId = null;
                if (!string.IsNullOrEmpty(sellerSlug))
                {
                    var seller = _userService.GetBySlug(sellerSlug);
                    if (seller != null)
                    {
                        sellerId = seller.UserId;
                    }
                }
                var subscription = await _subscriptionService.CreateSubscription(product.ProductId, userSession.UserId, networkId, sellerId);

                return new SubscriptionResult()
                {
                    Order = subscription.Order,
                    ClientSecret = subscription.ClientSecret
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        */

        [Authorize]
        [HttpPost("update")]
        public async Task<ActionResult<OrderResult>> Update([FromBody] OrderInfo order)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }
                var newOrder = _orderService.Update(order);
                return new OrderResult()
                {
                    Order = await _orderService.GetOrderInfo(newOrder)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("search")]
        [Authorize]
        public async Task<ActionResult<OrderListPagedResult>> Search([FromBody] OrderSearchParam param)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }
                return await _orderService.Search(param.NetworkId, param.UserId, param.SellerId, param.PageNum);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("list")]
        public async Task<ActionResult<OrderListResult>> List([FromBody] OrderParam param)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }
                var orders = _orderService.List(param.NetworkId, param.UserId, param.Status).ToList();
                var orderInfos = new List<OrderInfo>();
                foreach (var x in orders)
                {
                    orderInfos.Add(await _orderService.GetOrderInfo(x));
                }
                return new OrderListResult
                {
                    Sucesso = true,
                    Orders = orderInfos
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("getById/{orderId}")]
        public async Task<ActionResult<OrderResult>> GetById(long orderId)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }
                return new OrderResult
                {
                    Sucesso = true,
                    Order = await _orderService.GetOrderInfo(_orderService.GetById(orderId))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
