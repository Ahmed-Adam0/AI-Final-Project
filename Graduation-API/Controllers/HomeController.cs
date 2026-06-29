using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ShowcaseDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IShowcaseService _showcaseService;

        public HomeController(IShowcaseService showcaseService)
        {
            _showcaseService = showcaseService;
        }

        [HttpGet("showcase")]
        public async Task<IActionResult> GetShowcase()
        {
            try
            {
                var showcase = await _showcaseService.GetActiveShowcaseAsync();
                return Ok(showcase);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("showcase")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> CreateShowcase([FromForm] CreateShowcaseSlideRequest request)
        {
            try
            {
                var workshopId = GetWorkshopId();
                var result = await _showcaseService.CreateShowcaseSlideAsync(workshopId, request);
                return CreatedAtAction(nameof(GetShowcase), result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("showcase/{id}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateShowcase(int id, [FromForm] UpdateShowcaseSlideRequest request)
        {
            try
            {
                var workshopId = GetWorkshopId();
                var result = await _showcaseService.UpdateShowcaseSlideAsync(id, workshopId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("showcase/{id}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> DeleteShowcase(int id)
        {
            try
            {
                var workshopId = GetWorkshopId();
                var success = await _showcaseService.DeleteShowcaseSlideAsync(id, workshopId);
                return success ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetWorkshopId()
        {
            var workshopIdClaim = User.FindFirst("WorkshopId");
            if (workshopIdClaim != null && int.TryParse(workshopIdClaim.Value, out int workshopId))
            {
                return workshopId;
            }
            throw new UnauthorizedAccessException("Workshop ID claim not found for vendor.");
        }
    }
}
