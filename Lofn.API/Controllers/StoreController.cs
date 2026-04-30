using Lofn.Domain.Interfaces;
using Lofn.Domain.Mappers;
using Lofn.DTO.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using zTools.ACL.Interfaces;
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
        [HttpPost("insert")]
        public async Task<ActionResult<StoreInfo>> Insert([FromBody] StoreInsertInfo store)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var model = await _storeService.InsertAsync(store, userSession.UserId);
            return Ok(StoreMapper.ToInfo(model));
        }

        [Authorize]
        [HttpPost("update")]
        public async Task<ActionResult<StoreInfo>> Update([FromBody] StoreUpdateInfo store)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var model = await _storeService.UpdateAsync(store, userSession.UserId);
            return Ok(StoreMapper.ToInfo(model));
        }

        [Authorize]
        [RequestSizeLimit(100_000_000)]
        [HttpPost("uploadLogo/{storeId}")]
        public async Task<ActionResult<StoreInfo>> UploadLogo(long storeId, IFormFile file)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var fileName = await _fileClient.UploadFileAsync(_tenantResolver.BucketName, file);
            var logoUrl = await _fileClient.GetFileUrlAsync(_tenantResolver.BucketName, fileName);
            var model = await _storeService.UploadLogoAsync(storeId, logoUrl, userSession.UserId);

            var info = StoreMapper.ToInfo(model);
            info.LogoUrl = model.Logo;
            return Ok(info);
        }

        [Authorize]
        [HttpDelete("delete/{storeId}")]
        public async Task<IActionResult> Delete(long storeId)
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            await _storeService.DeleteAsync(storeId);
            return NoContent();
        }
    }
}
