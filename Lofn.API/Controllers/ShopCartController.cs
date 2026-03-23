using Lofn.Domain.Interfaces;
using Lofn.DTO.ShopCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Lofn.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ShopCartController : ControllerBase
    {
        private readonly IShopCartService _shopCartService;

        public ShopCartController(IShopCartService shopCartService)
        {
            _shopCartService = shopCartService;
        }

        [Authorize]
        [HttpPost("insert")]
        public async Task<ActionResult<ShopCartInfo>> Insert([FromBody] ShopCartInfo shopCart)
        {
            var result = await _shopCartService.InsertAsync(shopCart);
            return Ok(result);
        }
    }
}
