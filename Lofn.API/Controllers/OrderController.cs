using Lofn.Domain.Interfaces;
using Lofn.DTO.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IOrderService _orderService;

        public OrderController(
            IUserClient userClient,
            IOrderService orderService
        )
        {
            _userClient = userClient;
            _orderService = orderService;
        }

        private string GetBearerToken()
        {
            return HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Replace("Bearer ", "") ?? string.Empty;
        }

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
                var newOrder = await _orderService.UpdateAsync(order);
                return new OrderResult()
                {
                    Order = await _orderService.GetOrderInfoAsync(newOrder, GetBearerToken())
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
                return await _orderService.SearchAsync(param.NetworkId, param.UserId, param.SellerId, param.PageNum, GetBearerToken());
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
                var token = GetBearerToken();
                var orders = await _orderService.ListAsync(param.NetworkId, param.UserId, param.Status);
                var orderInfos = new List<OrderInfo>();
                foreach (var x in orders)
                {
                    orderInfos.Add(await _orderService.GetOrderInfoAsync(x, token));
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
                var order = await _orderService.GetByIdAsync(orderId);
                return new OrderResult
                {
                    Sucesso = true,
                    Order = await _orderService.GetOrderInfoAsync(order, GetBearerToken())
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
