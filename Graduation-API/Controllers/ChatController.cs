using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ChatDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
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

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new { Success = false, Message = "userId is required" });
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
    }
}
