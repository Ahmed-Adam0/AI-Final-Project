using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.InspirationDTOs;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspirationsController : ControllerBase
    {
        private readonly IInspirationService _inspirationService;

        public InspirationsController(IInspirationService inspirationService)
        {
            _inspirationService = inspirationService;
        }

        // GET /api/inspirations (Public endpoint)
        [HttpGet]
        public async Task<IActionResult> GetInspirations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 4)
        {
            try
            {
                var result = await _inspirationService.GetApprovedInspirationsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST /api/inspirations (Secured customer upload endpoint)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadInspirations([FromForm] UploadInspirationsDto request)
        {
            try
            {
                var userId = GetUserId();
                await _inspirationService.UploadInspirationsAsync(userId, request);
                return Ok(new { Message = "Inspiration photos uploaded successfully. Submissions are pending admin approval." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        private string GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID not found");

            return userId;
        }
    }
}
