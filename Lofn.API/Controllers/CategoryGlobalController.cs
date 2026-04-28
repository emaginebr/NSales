using Lofn.Application.Authorization;
using Lofn.Domain.Interfaces;
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
    }
}
