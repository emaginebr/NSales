using Lofn.Application.Authorization;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Mappers;
using Lofn.DTO.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.API.Controllers
{
    [Route("category-global")]
    [ApiController]
    [Authorize]
    [MarketplaceAdmin]
    public class CategoryGlobalController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryGlobalController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("insert")]
        public async Task<ActionResult<CategoryInfo>> Insert([FromBody] CategoryGlobalInsertInfo category)
        {
            var info = await _categoryService.InsertGlobalAsync(category);
            return Ok(info);
        }

        [HttpPost("update")]
        public async Task<ActionResult<CategoryInfo>> Update([FromBody] CategoryGlobalUpdateInfo category)
        {
            var info = await _categoryService.UpdateGlobalAsync(category);
            return Ok(info);
        }

        [HttpDelete("delete/{categoryId}")]
        public async Task<IActionResult> Delete(long categoryId)
        {
            await _categoryService.DeleteGlobalAsync(categoryId);
            return NoContent();
        }

        [HttpGet("list")]
        public async Task<ActionResult<IList<CategoryInfo>>> List()
        {
            var items = await _categoryService.ListGlobalAsync();
            return Ok(items);
        }

        [HttpPut("{categoryId:long}/producttype/{productTypeId:long}")]
        public async Task<IActionResult> LinkProductType(long categoryId, long productTypeId)
        {
            await _categoryService.LinkProductTypeAsync(categoryId, productTypeId);
            return Ok(new { categoryId, productTypeId });
        }

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
