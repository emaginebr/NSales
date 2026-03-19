using Lofn.Domain.Interfaces;
using Lofn.Domain.Mappers;
using Lofn.DTO.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using zTools.ACL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IUserClient _userClient;
        private readonly IStoreService _storeService;
        private readonly IFileClient _fileClient;
        private readonly ITenantResolver _tenantResolver;

        public StoreController(
            IUserClient userClient,
            IStoreService storeService,
            IFileClient fileClient,
            ITenantResolver tenantResolver
        )
        {
            _userClient = userClient;
            _storeService = storeService;
            _fileClient = fileClient;
            _tenantResolver = tenantResolver;
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<ActionResult<IList<StoreInfo>>> List()
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                return Ok(await _storeService.ListByOwnerAsync(userSession.UserId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("listActive")]
        public async Task<ActionResult<IList<StoreInfo>>> ListActive()
        {
            try
            {
                return Ok(await _storeService.ListActiveAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getBySlug/{storeSlug}")]
        public async Task<ActionResult<StoreInfo>> GetBySlug(string storeSlug)
        {
            try
            {
                var model = await _storeService.GetBySlugAsync(storeSlug);
                if (model == null)
                    return NotFound();

                if (model.Status != StoreStatusEnum.Active)
                    return BadRequest("Store is not active");

                return Ok(StoreMapper.ToInfo(model));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("getById/{storeId}")]
        public async Task<ActionResult<StoreInfo>> GetById(long storeId)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var model = await _storeService.GetByIdAsync(storeId);
                if (model == null)
                    return NotFound();

                return Ok(StoreMapper.ToInfo(model));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("insert")]
        public async Task<ActionResult<StoreInfo>> Insert([FromBody] StoreInsertInfo store)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var model = await _storeService.InsertAsync(store, userSession.UserId);
                return Ok(StoreMapper.ToInfo(model));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("update")]
        public async Task<ActionResult<StoreInfo>> Update([FromBody] StoreUpdateInfo store)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                var model = await _storeService.UpdateAsync(store, userSession.UserId);
                return Ok(StoreMapper.ToInfo(model));
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
        [RequestSizeLimit(100_000_000)]
        [HttpPost("uploadLogo/{storeId}")]
        public async Task<ActionResult<StoreInfo>> UploadLogo(long storeId, IFormFile file)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                var fileName = await _fileClient.UploadFileAsync(_tenantResolver.BucketName, file);
                var model = await _storeService.UploadLogoAsync(storeId, fileName, userSession.UserId);
                var logoUrl = await _fileClient.GetFileUrlAsync(_tenantResolver.BucketName, fileName);

                var info = StoreMapper.ToInfo(model);
                info.LogoUrl = logoUrl;
                return Ok(info);
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
        [HttpDelete("delete/{storeId}")]
        public async Task<IActionResult> Delete(long storeId)
        {
            try
            {
                var userSession = _userClient.GetUserInSession(HttpContext);
                if (userSession == null)
                    return Unauthorized();

                await _storeService.DeleteAsync(storeId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
