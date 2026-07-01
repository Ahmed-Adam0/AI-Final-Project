using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.RoomDesignDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_API.Controllers
{
    [Route("api/room-design")]
    [ApiController]
    public class RoomDesignController : ControllerBase
    {
        private readonly IGeminiRoomDesignService _geminiRoomDesignService;
        private readonly IGenaricRepositories<GenerateImage> _generateImageRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RoomDesignController(
            IGeminiRoomDesignService geminiRoomDesignService,
            IGenaricRepositories<GenerateImage> generateImageRepo,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _geminiRoomDesignService = geminiRoomDesignService;
            _generateImageRepo = generateImageRepo;
            _webHostEnvironment = webHostEnvironment;
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
        [Authorize]
        public async Task<IActionResult> GenerateRoomDesignN8n([FromBody] List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return BadRequest("Product IDs are required.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            try
            {
                var lastImage = await _generateImageRepo
                    .Where(x => x.UserID == userId)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (lastImage == null || string.IsNullOrEmpty(lastImage.EmptyRoom))
                {
                    return BadRequest("No saved room image found for this user.");
                }

                string roomImageUrl = "https://home-ai.runasp.net" + lastImage.EmptyRoom;
                var result = await _geminiRoomDesignService.GenerateRoomDesignFromUrlAsync(
                    roomImageUrl,
                    productIds
                );
                if (result == null)
                {
                    return StatusCode(500, "Failed to generate room design.");
                }
                else
                {
                    lastImage.GenerateImageUrl = result.GeneratedImageUrl;
                    _generateImageRepo.Update(lastImage);
                    await _generateImageRepo.SaveChangesAsync();

                    return Ok(result);
                }
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
        public async Task<IActionResult> SaveGeneratedImage([FromForm] SaveGeneratedImageDto dto)
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
                string emptyRoomUrl = string.Empty;
                if (dto.EmptyRoom != null)
                {
                    emptyRoomUrl = await SaveImageAsync(dto.EmptyRoom);
                }

                var existingImage = await _generateImageRepo
                    .Where(x => x.UserID == userId && string.IsNullOrEmpty(x.GenerateImageUrl))
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (existingImage != null)
                {
                    if (!string.IsNullOrEmpty(emptyRoomUrl))
                    {
                        existingImage.EmptyRoom = emptyRoomUrl;
                    }
                    existingImage.Length = dto.Length;
                    existingImage.Width = dto.Width;
                    existingImage.Height = dto.Height;

                    _generateImageRepo.Update(existingImage);
                    await _generateImageRepo.SaveChangesAsync();

                    return Ok(new { message = "Image updated successfully", id = existingImage.Id });
                }
                else
                {
                    var generateImage = new GenerateImage
                    {
                        UserID = userId,
                        EmptyRoom = emptyRoomUrl,
                        Length = dto.Length,
                        Width = dto.Width,
                        Height = dto.Height,
                    };

                    await _generateImageRepo.AddAsync(generateImage);
                    await _generateImageRepo.SaveChangesAsync();

                    return Ok(new { message = "Image saved successfully", id = generateImage.Id });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while saving the image: {ex.Message}");
            }
        }

        private async Task<string> SaveImageAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var webRootPath =
                _webHostEnvironment.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRootPath, "images", "room-designs");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/room-designs/{uniqueFileName}";
        }
    }
}
