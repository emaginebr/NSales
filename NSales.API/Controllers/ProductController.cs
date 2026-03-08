using NSales.Domain.Impl.Services;
using NSales.Domain.Interfaces.Services;
using NSales.DTO.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using NAuth.ACL;
using System.Threading.Tasks;

namespace NSales.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController: ControllerBase
    {
        private readonly IUserClient _userClient;
        //private readonly INetworkService _networkService;
        private readonly IProductService _productService;

        public ProductController(
            IUserClient userClient, 
            //INetworkService networkService, 
            IProductService productService
        )
        {
            _userClient = userClient;
            //_networkService = networkService;
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
                var newProfile = await _productService.Insert(product, userSession.UserId);
                return new ProductResult()
                {
                    Product = await _productService.GetProductInfo(newProfile)
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
                var newProduct = await _productService.Update(product, userSession.UserId);
                return new ProductResult()
                {
                    Product = await _productService.GetProductInfo(newProduct)
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
                /*
                if (!string.IsNullOrEmpty(param.NetworkSlug) && !(param.NetworkId.HasValue && param.NetworkId.Value > 0))
                {
                    var network = _networkService.GetBySlug(param.NetworkSlug);
                    if (network != null)
                    {
                        param.NetworkId = network.NetworkId;
                    }
                }
                */
                if (!string.IsNullOrEmpty(param.UserSlug) && !(param.UserId.HasValue && param.UserId.Value > 0))
                {
                    var user = await _userClient.GetBySlugAsync(param.UserSlug);
                    if (user != null)
                    {
                        param.UserId = user.UserId;
                    }
                }
                return _productService.Search(param);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /*
        [HttpGet("listByNetwork/{networkId}")]
        public ActionResult<ProductListResult> ListByNetwork(long networkId)
        {
            try
            {
                var products = _productService
                    .ListByNetwork(networkId)
                    .Select(x => _productService.GetProductInfo(x))
                    .ToList();

                // Corrigir: aguardar todas as tasks antes de atribuir à lista de produtos
                var productInfos = products.Select(task => task.Result).ToList();

                return new ProductListResult
                {
                    Sucesso = true,
                    Products = productInfos
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        */

        /*
        [HttpGet("listByNetworkSlug/{networkSlug}")]
        public ActionResult<ProductListResult> ListByNetworkSlug(string networkSlug)
        {
            try
            {
                var network = _networkService.GetBySlug(networkSlug);
                if (network == null)
                {
                    throw new Exception("Network not found");
                }

                var products = _productService
                    .ListByNetwork(network.NetworkId)
                    .Select(x => _productService.GetProductInfo(x))
                    .ToList();

                // Corrigir: aguardar todas as tasks antes de atribuir à lista de produtos
                var productInfos = products.Select(task => task.Result).ToList();

                return new ProductListResult
                {
                    Sucesso = true,
                    Products = productInfos
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        */

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
                return new ProductResult
                {
                    Sucesso = true,
                    Product = await _productService.GetProductInfo(_productService.GetById(productId))
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
                return new ProductResult
                {
                    Sucesso = true,
                    Product = await _productService.GetProductInfo(_productService.GetBySlug(productSlug))
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
