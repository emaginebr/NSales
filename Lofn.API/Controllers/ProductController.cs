using Lofn.Domain.Interfaces;
using Lofn.Domain.Services;
using Lofn.DTO.Product;
using Lofn.DTO.ProductType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
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
        private readonly ProductPriceCalculator _priceCalculator;

        public ProductController(
            IUserClient userClient,
            IProductService productService,
            IStoreService storeService,
            ProductPriceCalculator priceCalculator
        )
        {
            _userClient = userClient;
            _productService = productService;
            _storeService = storeService;
            _priceCalculator = priceCalculator;
        }

        [Authorize]
        [HttpPost("{storeSlug}/insert")]
        public async Task<ActionResult<ProductInfo>> Insert(string storeSlug, [FromBody] ProductInsertInfo product)
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

        [Authorize]
        [HttpPost("{storeSlug}/update")]
        public async Task<ActionResult<ProductInfo>> Update(string storeSlug, [FromBody] ProductUpdateInfo product)
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

        [HttpPost("search")]
        public async Task<ActionResult<ProductListPagedResult>> Search([FromBody] ProductSearchParam param)
        {
            if (!string.IsNullOrEmpty(param.UserSlug) && !(param.UserId.HasValue && param.UserId.Value > 0))
            {
                var user = await _userClient.GetBySlugAsync(param.UserSlug);
                if (user != null)
                    param.UserId = user.UserId;
            }
            return Ok(await _productService.SearchAsync(param));
        }

        [AllowAnonymous]
        [HttpPost("{productId:long}/price")]
        public async Task<ActionResult<ProductPriceCalculationResult>> CalculatePrice(long productId, [FromBody] ProductPriceCalculationRequest request)
        {
            var optionIds = request?.OptionIds ?? new System.Collections.Generic.List<long>();
            var result = await _priceCalculator.CalculateAsync(productId, optionIds);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("search-filtered")]
        public async Task<ActionResult<ProductSearchFilteredResult>> SearchFiltered([FromBody] ProductSearchFilteredParam param)
        {
            try
            {
                var result = await _productService.SearchFilteredAsync(param);
                return Ok(result);
            }
            catch (System.Exception ex) when (ex.Message == "Store not found" || ex.Message == "Category not found")
            {
                return NotFound(ex.Message);
            }
        }

    }
}
