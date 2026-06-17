using Graduation_Application.DTOs.VendorMaterialsDTO;
using Graduation_Application.IServices.Vendor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Graduation_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendor")]
    public class VendorMaterialsController : ControllerBase
    {
        private readonly IVendorMaterialService _vendorMaterialService;

        public VendorMaterialsController(IVendorMaterialService vendorMaterialService)
        {
            _vendorMaterialService = vendorMaterialService;
        }

        private int GetWorkshopId()
        {
            var workshopIdClaim = User.FindFirst("WorkshopId");
            if (workshopIdClaim != null && int.TryParse(workshopIdClaim.Value, out int workshopId))
            {
                return workshopId;
            }
            throw new System.UnauthorizedAccessException("Workshop ID not found for vendor.");
        }

        [HttpGet]
        public async Task<IActionResult> GetVendorMaterials()
        {
            try
            {
                var workshopId = GetWorkshopId();
                var materials = await _vendorMaterialService.GetVendorMaterialsAsync(workshopId);
                return Ok(materials);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("Groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateVendorMaterialGroupDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var workshopId = GetWorkshopId();
                var group = await _vendorMaterialService.CreateGroupAsync(workshopId, dto);
                return Ok(group);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("Groups/{groupId}/Options")]
        public async Task<IActionResult> AddOption(int groupId, [FromBody] CreateVendorMaterialOptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var workshopId = GetWorkshopId();
                var option = await _vendorMaterialService.AddOptionAsync(workshopId, groupId, dto);
                return Ok(option);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("Options/{optionId}")]
        public async Task<IActionResult> UpdateOption(int optionId, [FromBody] UpdateVendorMaterialOptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var workshopId = GetWorkshopId();
                var option = await _vendorMaterialService.UpdateOptionAsync(workshopId, optionId, dto);
                return Ok(option);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("Groups/{groupId}")]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            try
            {
                var workshopId = GetWorkshopId();
                await _vendorMaterialService.DeleteGroupAsync(workshopId, groupId);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("Options/{optionId}")]
        public async Task<IActionResult> DeleteOption(int optionId)
        {
            try
            {
                var workshopId = GetWorkshopId();
                await _vendorMaterialService.DeleteOptionAsync(workshopId, optionId);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
