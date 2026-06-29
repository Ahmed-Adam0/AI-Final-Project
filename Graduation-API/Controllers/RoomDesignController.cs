using Graduation_Application.DTOs.RoomDesignDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Graduation_API.Controllers
{
    [Route("api/room-design")]
    [ApiController]
    public class RoomDesignController : ControllerBase
    {
        private readonly IGeminiRoomDesignService _geminiRoomDesignService;

        public RoomDesignController(IGeminiRoomDesignService geminiRoomDesignService)
        {
            _geminiRoomDesignService = geminiRoomDesignService;
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


    }
}
