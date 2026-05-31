using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Graduation_Application.IServices;
using Graduation_Application.DTOs.VendorDTO;
using System.Security.Claims;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> VendorLogin([FromBody] VendorLoginDto dto)
        {
            try
            {
                var result = await _vendorService.VendorLoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("create")]
        
        public async Task<IActionResult> CreateVendorAsync([FromBody] CreateVendorDto dto)
        {
            try
            {
                await _vendorService.CreateVendorAsync(dto);
                return Created(string.Empty, new { Success = true, Message = "Vendor created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("logo")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateLogo([FromForm] IFormFile logo)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();
                if (logo == null || logo.Length == 0) return BadRequest(new { Message = "No file provided" });

                await _vendorService.UpdateVendorLogoAsync(userId, logo);
                return Ok(new { Message = "Logo updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                var profile = await _vendorService.GetVendorProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateVendorProfileDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                var updated = await _vendorService.UpdateVendorProfileAsync(userId, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
