using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    return Unauthorized();

                var result = await _profileService.GetProfileAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    return Unauthorized();

                var result = await _profileService.UpdateProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("image")]
        public async Task<IActionResult> UpdateProfileImage(IFormFile file)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    return Unauthorized();
                if (file == null || file.Length == 0)
                    return BadRequest(new { Message = "No file provided" });

                var result = await _profileService.UpdateProfileImageAsync(userId, file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    return Unauthorized();

                await _profileService.ChangePasswordAsync(userId, dto);
                return Ok(new { Message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
