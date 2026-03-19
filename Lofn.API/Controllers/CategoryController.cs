using Lofn.Domain.Interfaces;
using Lofn.Domain.Mappers;
using Lofn.DTO.Category;
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
    public class CategoryController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly ICategoryService _categoryService;
        private readonly IStoreService _storeService;

        public CategoryController(
            IUserClient userClient,
            ICategoryService categoryService,
            IStoreService storeService
        )
        {
            _userClient = userClient;
            _categoryService = categoryService;
            _storeService = storeService;
        }

        [Authorize]
        [HttpPost("{storeSlug}/insert")]
        public async Task<ActionResult<CategoryInfo>> Insert(string storeSlug, [FromBody] CategoryInsertInfo category)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var store = await _storeService.GetBySlugAsync(storeSlug);
                if (store == null)
                    return NotFound("Store not found");

                var model = await _categoryService.InsertAsync(category, store.StoreId, userSession.UserId);
                return Ok(CategoryMapper.ToInfo(model));
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
        public async Task<ActionResult<CategoryInfo>> Update(string storeSlug, [FromBody] CategoryUpdateInfo category)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var store = await _storeService.GetBySlugAsync(storeSlug);
                if (store == null)
                    return NotFound("Store not found");

                var model = await _categoryService.UpdateAsync(category, store.StoreId, userSession.UserId);
                return Ok(CategoryMapper.ToInfo(model));
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
        [HttpDelete("{storeSlug}/delete/{categoryId}")]
        public async Task<IActionResult> Delete(string storeSlug, long categoryId)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var store = await _storeService.GetBySlugAsync(storeSlug);
                if (store == null)
                    return NotFound("Store not found");

                await _categoryService.DeleteAsync(categoryId, store.StoreId, userSession.UserId);
                return NoContent();
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
    }
}
