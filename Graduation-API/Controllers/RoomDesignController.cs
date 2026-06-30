using Graduation_domain.Entities;
using Graduation_Application.IRepositories;
using Graduation_Application.DTOs.RoomDesignDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace Graduation_API.Controllers
{
    [Route("api/room-design")]
    [ApiController]
    public class RoomDesignController : ControllerBase
    {
        private readonly IGeminiRoomDesignService _geminiRoomDesignService;
        private readonly IGenaricRepositories<GenerateImage> _generateImageRepo;

        public RoomDesignController(
            IGeminiRoomDesignService geminiRoomDesignService,
            IGenaricRepositories<GenerateImage> generateImageRepo)
        {
            _geminiRoomDesignService = geminiRoomDesignService;
            _generateImageRepo = generateImageRepo;
        }

        [HttpPost("generate")]
        [AllowAnonymous] // User specified not to restrict to authenticated users
        public async Task<IActionResult> GenerateRoomDesign([FromForm] RoomDesignRequestDto request)
        {
            if (request.RoomImage == null || request.RoomImage.Length == 0)
            {
                return BadRequest("Room image is required.");
            }

            try
            {
                var result = await _geminiRoomDesignService.GenerateRoomDesignAsync(request);
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("generate-n8n")]
        [AllowAnonymous] // User specified not to restrict to authenticated users
        public async Task<IActionResult> GenerateRoomDesignN8n([FromBody] List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return BadRequest("Product IDs are required.");
            }

            try
            {
                string hardcodedRoomImageUrl = "https://t4.ftcdn.net/jpg/06/42/15/89/360_F_642158981_wXsDWMxlUwSLU0McBzlef98eHZR5YDGa.jpg";
                var result = await _geminiRoomDesignService.GenerateRoomDesignFromUrlAsync(hardcodedRoomImageUrl, productIds);
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("save-image")]
        [Authorize]
        public async Task<IActionResult> SaveGeneratedImage([FromBody] SaveGeneratedImageDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            try
            {
                var generateImage = new GenerateImage
                {
                    UserID = userId,
                    EmptyRoom = dto.EmptyRoom,

                    Length = dto.Length,
                    Width = dto.Width,
                    Height = dto.Height
                };

                await _generateImageRepo.AddAsync(generateImage);
                await _generateImageRepo.SaveChangesAsync();

                return Ok(new { message = "Image saved successfully", id = generateImage.Id });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while saving the image: {ex.Message}");
            }
        }
    }
}
