using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAdminAuditLogsService _adminAuditLogsService;

        public OrderController(
            IOrderService orderService,
            IAdminAuditLogsService adminAuditLogsService
        )
        {
            _orderService = orderService;
            _adminAuditLogsService = adminAuditLogsService;
        }

        private string GetUserId()
        {
            // Try standard NameIdentifier claim first, then fallback to JWT 'sub', then name or email
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(
                    System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub
                )?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID not found");

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            try
            {
                var userId = GetUserId();
                var order = await _orderService.CreateOrderAsync(userId, request, User);
                await _adminAuditLogsService.CreateLogAsync(
                    userId,
                    GetCurrentUserName(),
                    GetCurrentUserRole(),
                    "CreateOrder",
                    "Order",
                    order.Id.ToString(),
                    $"Created order #{order.Id}.",
                    GetCurrentUserRoleAr(),
                    "إنشاء طلب",
                    "طلب",
                    $"تم إنشاء الطلب رقم {order.Id}."
                );
                return Ok(new { Message = "Order created successfully", Data = order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userId = GetUserId();
                var orders = await _orderService.GetMyOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            [FromBody] UpdateOrderStatusRequest request
        )
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Status))
                    return BadRequest(new { Message = "Status is required" });

                await _orderService.UpdateOrderStatusAsync(id, request.Status);
                await _adminAuditLogsService.CreateLogAsync(
                    GetUserId(),
                    GetCurrentUserName(),
                    GetCurrentUserRole(),
                    "UpdateOrderStatus",
                    "Order",
                    id.ToString(),
                    $"Updated order #{id} status to '{request.Status}'.",
                    GetCurrentUserRoleAr(),
                    "تحديث حالة الطلب",
                    "طلب",
                    $"تم تحديث حالة الطلب رقم {id} إلى '{request.Status}'."
                );
                return Ok(new { Message = "Order status updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = GetUserId();
                await _orderService.CancelOrderAsync(id, userId);
                await _adminAuditLogsService.CreateLogAsync(
                    userId,
                    GetCurrentUserName(),
                    GetCurrentUserRole(),
                    "CancelOrder",
                    "Order",
                    id.ToString(),
                    $"Cancelled order #{id}.",
                    GetCurrentUserRoleAr(),
                    "إلغاء طلب",
                    "طلب",
                    $"تم إلغاء الطلب رقم {id}."
                );
                return Ok(new { Message = "Order cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/items")]
        public async Task<IActionResult> UpdateOrderItems(
            int id,
            [FromBody] UpdateOrderItemsDto request
        )
        {
            try
            {
                var userId = GetUserId();
                var updatedOrder = await _orderService.UpdateOrderItemsAsync(id, userId, request);
                return Ok(
                    new { Message = "Order items updated successfully", Data = updatedOrder }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        private string GetCurrentUserName() =>
            User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? User.Identity?.Name
            ?? "Unknown";

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

        private string GetCurrentUserRoleAr() =>
            GetCurrentUserRole() switch
            {
                "SuperAdmin" => "مشرف عام",
                "Customer" => "عميل",
                "Vendor" => "بائع",
                _ => GetCurrentUserRole(),
            };
    }

    public class UpdateOrderStatusRequest
    {
        public string? Status { get; set; }
    }
}
