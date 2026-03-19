using Lofn.Domain.Interfaces;
using Lofn.DTO.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;

        public ProductController(
            IUserClient userClient,
            IProductService productService,
            IStoreService storeService
        )
        {
            _userClient = userClient;
            _productService = productService;
            _storeService = storeService;
        }

        [Authorize]
        [HttpPost("{storeSlug}/insert")]
        public async Task<ActionResult<ProductInfo>> Insert(string storeSlug, [FromBody] ProductInsertInfo product)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var store = await _storeService.GetBySlugAsync(storeSlug);
                if (store == null)
                    return NotFound("Store not found");

                var newProduct = await _productService.InsertAsync(product, store.StoreId, userSession.UserId);
                return Ok(await _productService.GetProductInfoAsync(newProduct));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{storeSlug}/update")]
        public async Task<ActionResult<ProductInfo>> Update(string storeSlug, [FromBody] ProductUpdateInfo product)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var store = await _storeService.GetBySlugAsync(storeSlug);
                if (store == null)
                    return NotFound("Store not found");

                var updatedProduct = await _productService.UpdateAsync(product, store.StoreId, userSession.UserId);
                return Ok(await _productService.GetProductInfoAsync(updatedProduct));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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
                        param.UserId = user.UserId;
                }
                return Ok(await _productService.SearchAsync(param));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{storeSlug}/getById/{productId}")]
        public async Task<ActionResult<ProductInfo>> GetById(string storeSlug, long productId)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var store = await _storeService.GetBySlugAsync(storeSlug);
                if (store == null)
                    return NotFound("Store not found");

                var product = await _productService.GetByIdAsync(productId, store.StoreId, userSession.UserId);
                if (product == null)
                    return NotFound();

                return Ok(await _productService.GetProductInfoAsync(product));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{storeSlug}/category/{categorySlug}/listActive")]
        public async Task<ActionResult<IList<ProductInfo>>> ListActiveByCategory(string storeSlug, string categorySlug)
        {
            try
            {
                return Ok(await _productService.ListActiveByCategorySlugAsync(storeSlug, categorySlug));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getBySlug/{productSlug}")]
        public async Task<ActionResult<ProductInfo>> GetBySlug(string productSlug)
        {
            try
            {
                var product = await _productService.GetBySlugAsync(productSlug);
                if (product == null)
                    return NotFound();

                return Ok(await _productService.GetProductInfoAsync(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
