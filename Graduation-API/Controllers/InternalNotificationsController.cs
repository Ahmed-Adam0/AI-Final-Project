using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/internal-notifications")]
    [Authorize]
    public class InternalNotificationsController : ControllerBase
    {
        private readonly IInternalNotificationService _internalNotificationService;

        public InternalNotificationsController(IInternalNotificationService internalNotificationService)
        {
            _internalNotificationService = internalNotificationService;
        }

        private string GetUserId()
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID not found");

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { Message = "Page and PageSize must be greater than 0" });

                var userId = GetUserId();
                var result = await _internalNotificationService.GetNotificationsAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _internalNotificationService.GetUnreadCountAsync(userId);
                return Ok(new { UnreadCount = count });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetUserId();
                await _internalNotificationService.MarkAsReadAsync(userId, id);
                return Ok(new { Message = "Notification marked as read" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                await _internalNotificationService.MarkAllAsReadAsync(userId);
                return Ok(new { Message = "All notifications marked as read" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
