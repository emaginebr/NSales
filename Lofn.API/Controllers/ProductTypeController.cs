using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lofn.Application.Authorization;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Mappers;
using Lofn.DTO.ProductType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lofn.API.Controllers
{
    [Route("producttype")]
    [ApiController]
    [Authorize]
    [TenantAdmin]
    public class ProductTypeController : ControllerBase
    {
        private readonly IProductTypeService _service;

        public ProductTypeController(IProductTypeService service)
        {
            _service = service;
        }

        [HttpPost("insert")]
        public async Task<ActionResult<ProductTypeInfo>> Insert([FromBody] ProductTypeInsertInfo info)
        {
            var model = await _service.InsertAsync(info);
            return Ok(ProductTypeMapper.ToInfo(model));
        }

        [HttpPost("update")]
        public async Task<ActionResult<ProductTypeInfo>> Update([FromBody] ProductTypeUpdateInfo info)
        {
            var model = await _service.UpdateAsync(info);
            return Ok(ProductTypeMapper.ToInfo(model));
        }

        [HttpDelete("delete/{productTypeId}")]
        public async Task<IActionResult> Delete(long productTypeId)
        {
            await _service.DeleteAsync(productTypeId);
            return NoContent();
        }

        [HttpGet("list")]
        public async Task<ActionResult<IList<ProductTypeInfo>>> List()
        {
            var items = await _service.ListAllAsync();
            return Ok(items.Select(ProductTypeMapper.ToInfo).ToList());
        }

        [HttpGet("{productTypeId}")]
        public async Task<ActionResult<ProductTypeInfo>> GetById(long productTypeId)
        {
            var model = await _service.GetByIdAsync(productTypeId);
            if (model == null) return NotFound();
            return Ok(ProductTypeMapper.ToInfo(model));
        }

        [HttpPost("{productTypeId}/filter/insert")]
        public async Task<ActionResult<ProductTypeFilterInfo>> InsertFilter(long productTypeId, [FromBody] ProductTypeFilterInsertInfo info)
        {
            var model = await _service.InsertFilterAsync(productTypeId, info);
            return Ok(ProductTypeMapper.ToFilterInfo(model));
        }

        [HttpPost("filter/update")]
        public async Task<ActionResult<ProductTypeFilterInfo>> UpdateFilter([FromBody] ProductTypeFilterUpdateInfo info)
        {
            var model = await _service.UpdateFilterAsync(info);
            return Ok(ProductTypeMapper.ToFilterInfo(model));
        }

        [HttpDelete("filter/delete/{filterId}")]
        public async Task<IActionResult> DeleteFilter(long filterId)
        {
            await _service.DeleteFilterAsync(filterId);
            return NoContent();
        }

        // ----- US5 Customizations -----

        [HttpPost("{productTypeId}/customization/group/insert")]
        public async Task<ActionResult<CustomizationGroupInfo>> InsertGroup(long productTypeId, [FromBody] CustomizationGroupInsertInfo info)
        {
            var model = await _service.InsertGroupAsync(productTypeId, info);
            return Ok(ProductTypeMapper.ToCustomizationGroupInfo(model));
        }

        [HttpPost("customization/group/update")]
        public async Task<ActionResult<CustomizationGroupInfo>> UpdateGroup([FromBody] CustomizationGroupUpdateInfo info)
        {
            var model = await _service.UpdateGroupAsync(info);
            return Ok(ProductTypeMapper.ToCustomizationGroupInfo(model));
        }

        [HttpDelete("customization/group/delete/{groupId}")]
        public async Task<IActionResult> DeleteGroup(long groupId)
        {
            await _service.DeleteGroupAsync(groupId);
            return NoContent();
        }

        [HttpPost("customization/group/{groupId}/option/insert")]
        public async Task<ActionResult<CustomizationOptionInfo>> InsertOption(long groupId, [FromBody] CustomizationOptionInsertInfo info)
        {
            var model = await _service.InsertOptionAsync(groupId, info);
            return Ok(ProductTypeMapper.ToCustomizationOptionInfo(model));
        }

        [HttpPost("customization/option/update")]
        public async Task<ActionResult<CustomizationOptionInfo>> UpdateOption([FromBody] CustomizationOptionUpdateInfo info)
        {
            var model = await _service.UpdateOptionAsync(info);
            return Ok(ProductTypeMapper.ToCustomizationOptionInfo(model));
        }

        [HttpDelete("customization/option/delete/{optionId}")]
        public async Task<IActionResult> DeleteOption(long optionId)
        {
            await _service.DeleteOptionAsync(optionId);
            return NoContent();
        }
    }
}
