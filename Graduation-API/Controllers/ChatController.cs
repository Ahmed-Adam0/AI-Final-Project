using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ChatDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IVoiceChatService _voiceChatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            IVoiceChatService voiceChatService,
            ILogger<ChatController> logger
        )
        {
            _chatService = chatService;
            _voiceChatService = voiceChatService;
            _logger = logger;
        }

        private string GetUserId()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("Authenticated user ID not found");

            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageAsync(
            [FromBody] ChatRequestDto request,
            CancellationToken cancellationToken
        )
        {
            if (request == null)
            {
                return BadRequest(new { Success = false, Message = "Request body is required" });
            }

            try
            {
                request.UserId = GetUserId();
                request.Token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { Success = false, Message = "message is required" });
            }

            try
            {
                var result = await _chatService.SendMessageAsync(request, cancellationToken);

                return Ok(
                    new
                    {
                        Success = true,
                        Message = "Response received successfully",
                        Data = result,
                    }
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid chat request payload.");
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout while processing chat request.");
                return StatusCode(504, new { Success = false, Message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network failure while processing chat request.");
                return StatusCode(503, new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "n8n response handling failure.");
                return StatusCode(502, new { Success = false, Message = ex.Message });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Chat request cancelled by client.");
                return StatusCode(
                    408,
                    new { Success = false, Message = "The request was cancelled." }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing chat request.");
                return StatusCode(
                    500,
                    new { Success = false, Message = "An unexpected error occurred." }
                );
            }
        }

        [HttpPost("voice")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendVoiceMessageAsync(
            IFormFile audioFile,
            [FromForm] string? conversationId,
            CancellationToken cancellationToken
        )
        {
            string userId;
            string token;
            try
            {
                userId = GetUserId();
                token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }

            try
            {
                var result = await _voiceChatService.ProcessVoiceMessageAsync(
                    audioFile,
                    userId,
                    conversationId,
                    token,
                    cancellationToken
                );

                return Ok(
                    new
                    {
                        Success = true,
                        Message = "Response received successfully",
                        Data = result,
                    }
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid voice chat request payload.");
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Unprocessable entity in voice chat request.");
                return StatusCode(422, new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Service failure while processing voice chat request.");
                // We're returning 502 Bad Gateway if n8n fails, or 500 if speech provider fails.
                // The VoiceChatService throws InvalidOperationException for both, but ideally we'd separate them.
                // For now, mapping InvalidOperationException to 502 as per original setup for n8n failure.
                return StatusCode(502, new { Success = false, Message = ex.Message });
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout while processing voice chat request.");
                return StatusCode(504, new { Success = false, Message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network failure while processing voice chat request.");
                return StatusCode(503, new { Success = false, Message = ex.Message });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Voice chat request cancelled by client.");
                return StatusCode(
                    408,
                    new { Success = false, Message = "The request was cancelled." }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing voice chat request.");
                return StatusCode(
                    500,
                    new { Success = false, Message = "An unexpected error occurred." }
                );
            }
        }
    }
}
