using Lofn.Domain.Interfaces;
using Lofn.DTO.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System;
using System.Threading.Tasks;

namespace Lofn.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IProductService _productService;

        public ProductController(
            IUserClient userClient,
            IProductService productService
        )
        {
            _userClient = userClient;
            _productService = productService;
        }

        [Authorize]
        [HttpPost("insert")]
        public async Task<ActionResult<ProductResult>> Insert([FromBody] ProductInfo product)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }
                var newProduct = await _productService.InsertAsync(product, userSession.UserId);
                return new ProductResult()
                {
                    Product = await _productService.GetProductInfoAsync(newProduct)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("update")]
        public async Task<ActionResult<ProductResult>> Update([FromBody] ProductInfo product)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }
                var newProduct = await _productService.UpdateAsync(product, userSession.UserId);
                return new ProductResult()
                {
                    Product = await _productService.GetProductInfoAsync(newProduct)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<ProductListPagedResult>> Search([FromBody] ProductSearchParam param)
        {
            try
            {
                if (!string.IsNullOrEmpty(param.UserSlug) && !(param.UserId.HasValue && param.UserId.Value > 0))
                {
                    var user = await _userClient.GetBySlugAsync(param.UserSlug);
                    if (user != null)
                    {
                        param.UserId = user.UserId;
                    }
                }
                return await _productService.SearchAsync(param);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("getById/{productId}")]
        public async Task<ActionResult<ProductResult>> GetById(long productId)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                {
                    return StatusCode(401, "Not Authorized");
                }
                var product = await _productService.GetByIdAsync(productId);
                return new ProductResult
                {
                    Sucesso = true,
                    Product = await _productService.GetProductInfoAsync(product)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getBySlug/{productSlug}")]
        public async Task<ActionResult<ProductResult>> GetBySlug(string productSlug)
        {
            try
            {
                var product = await _productService.GetBySlugAsync(productSlug);
                return new ProductResult
                {
                    Sucesso = true,
                    Product = await _productService.GetProductInfoAsync(product)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
