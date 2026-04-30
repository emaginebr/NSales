using Lofn.Application.Authorization;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Mappers;
using Lofn.DTO.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using System.Threading.Tasks;

namespace Lofn.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly ICategoryService _categoryService;
        private readonly IStoreService _storeService;
        private readonly ITenantResolver _tenantResolver;

        public CategoryController(
            IUserClient userClient,
            ICategoryService categoryService,
            IStoreService storeService,
            ITenantResolver tenantResolver
        )
        {
            _userClient = userClient;
            _categoryService = categoryService;
            _storeService = storeService;
            _tenantResolver = tenantResolver;
        }

        [Authorize]
        [HttpPost("{storeSlug}/insert")]
        public async Task<ActionResult<CategoryInfo>> Insert(string storeSlug, [FromBody] CategoryInsertInfo category)
        {
            if (_tenantResolver.Marketplace)
                return Forbid();

            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var store = await _storeService.GetBySlugAsync(storeSlug);
            if (store == null)
                return NotFound("Store not found");

            var model = await _categoryService.InsertAsync(category, store.StoreId, userSession.UserId);
            return Ok(CategoryMapper.ToInfo(model));
        }

        [Authorize]
        [HttpPost("{storeSlug}/update")]
        public async Task<ActionResult<CategoryInfo>> Update(string storeSlug, [FromBody] CategoryUpdateInfo category)
        {
            if (_tenantResolver.Marketplace)
                return Forbid();

            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var store = await _storeService.GetBySlugAsync(storeSlug);
            if (store == null)
                return NotFound("Store not found");

            var model = await _categoryService.UpdateAsync(category, store.StoreId, userSession.UserId);
            return Ok(CategoryMapper.ToInfo(model));
        }

        [Authorize]
        [HttpDelete("{storeSlug}/delete/{categoryId}")]
        public async Task<IActionResult> Delete(string storeSlug, long categoryId)
        {
            if (_tenantResolver.Marketplace)
                return Forbid();

            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var store = await _storeService.GetBySlugAsync(storeSlug);
            if (store == null)
                return NotFound("Store not found");

            await _categoryService.DeleteAsync(categoryId, store.StoreId, userSession.UserId);
            return NoContent();
        }

        [Authorize]
        [TenantAdmin]
        [HttpPut("{categoryId:long}/producttype/{productTypeId:long}")]
        public async Task<IActionResult> LinkProductType(long categoryId, long productTypeId)
        {
            await _categoryService.LinkProductTypeAsync(categoryId, productTypeId);
            return Ok(new { categoryId, productTypeId });
        }

        [Authorize]
        [TenantAdmin]
        [HttpDelete("{categoryId:long}/producttype")]
        public async Task<IActionResult> UnlinkProductType(long categoryId)
        {
            await _categoryService.UnlinkProductTypeAsync(categoryId);
            return Ok(new { categoryId, productTypeId = (long?)null });
        }

        [AllowAnonymous]
        [HttpGet("{categoryId:long}/producttype/applied")]
        public async Task<IActionResult> GetAppliedProductType(long categoryId)
        {
            var resolution = await _categoryService.GetAppliedProductTypeAsync(categoryId);
            if (resolution == null)
                return Ok((object)null);

            return Ok(new
            {
                appliedProductTypeId = resolution.ProductType.ProductTypeId,
                originCategoryId = resolution.OriginCategoryId,
                productType = ProductTypeMapper.ToInfo(resolution.ProductType)
            });
        }
    }
}
