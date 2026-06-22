using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VendorOrdersController : ControllerBase
    {
        private readonly IVendorOrderService _vendorOrderService;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly IAdminAuditLogsService _adminAuditLogsService;

        public VendorOrdersController(
            IVendorOrderService vendorOrderService,
            IGenaricRepositories<Workshop> workshopRepository,
            IAdminAuditLogsService adminAuditLogsService
        )
        {
            _vendorOrderService = vendorOrderService;
            _workshopRepository = workshopRepository;
            _adminAuditLogsService = adminAuditLogsService;
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

        private async Task<int> GetVendorWorkshopIdAsync()
        {
            var workshopIdClaim = User.FindFirstValue("WorkshopId");
            if (int.TryParse(workshopIdClaim, out var workshopId))
                return workshopId;

            var userId = GetUserId();
            var workshop = await _workshopRepository.FirstOrDefaultAsync(w => w.UserId == userId);

            if (workshop == null)
                throw new KeyNotFoundException("Workshop not found for the authenticated user");

            return workshop.Id;
        }

        private string GetCurrentUserName() =>
            User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? User.Identity?.Name
            ?? "Unknown";

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "Vendor";

        private string GetCurrentUserRoleAr() =>
            GetCurrentUserRole() switch
            {
                "SuperAdmin" => "مشرف عام",
                "Customer" => "عميل",
                "Vendor" => "بائع",
                _ => GetCurrentUserRole(),
            };

        private string GetLocalizedStatusAr(string status)
        {
            return status?.ToLower().Replace(" ", "") switch
            {
                "pending" => "قيد الانتظار",
                "awaitingcustomerapproval" => "في انتظار موافقة العميل",
                "confirmed" => "مؤكد",
                "inprogress" => "قيد التنفيذ",
                "processing" => "جاري المعالجة",
                "partiallydelivered" => "تم التوصيل جزئياً",
                "readyforpickup" => "جاهز للاستلام",
                "shipped" => "تم الشحن",
                "delivered" => "تم التوصيل",
                "cancelled" => "ملغي",
                "rejected" => "مرفوض",
                _ => status ?? "غير معروف"
            };
        }

        private string GetLocalizedStatusEn(string status)
        {
            return status?.ToLower().Replace(" ", "") switch
            {
                "pending" => "Pending",
                "awaitingcustomerapproval" => "Awaiting Customer Approval",
                "confirmed" => "Confirmed",
                "inprogress" => "In Progress",
                "processing" => "Processing",
                "partiallydelivered" => "Partially Delivered",
                "readyforpickup" => "Ready for Pickup",
                "shipped" => "Shipped",
                "delivered" => "Delivered",
                "cancelled" => "Cancelled",
                "rejected" => "Rejected",
                _ => status ?? "Unknown"
            };
        }

        /// <summary>
        /// Get vendor orders with filtering and pagination
        /// </summary>
        [HttpPost("orders/filter")]
        public async Task<IActionResult> GetVendorOrders([FromBody] VendorOrdersFilterDto filter)
        {
            try
            {
                var workshopId = await GetVendorWorkshopIdAsync();
                var (orders, totalCount) = await _vendorOrderService.GetVendorOrdersAsync(
                    workshopId,
                    filter
                );

                return Ok(
                    new
                    {
                        data = orders,
                        totalCount,
                        pageNumber = filter.PageNumber,
                        pageSize = filter.PageSize,
                        totalPages = (int)System.Math.Ceiling((double)totalCount / filter.PageSize),
                    }
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get specific order details
        /// </summary>
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var workshopId = await GetVendorWorkshopIdAsync();
                var orderDetails = await _vendorOrderService.GetVendorOrderDetailsAsync(
                    orderId,
                    workshopId
                );
                return Ok(orderDetails);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("orders/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            [FromBody] UpdateStatusDto request
        )
        {
            try
            {
                var workshopId = await GetVendorWorkshopIdAsync();
                await _vendorOrderService.UpdateVendorOrderStatusAsync(
                    orderId,
                    workshopId,
                    request.NewStatus
                );
                await _adminAuditLogsService.CreateLogAsync(
                    GetUserId(),
                    GetCurrentUserName(),
                    GetCurrentUserRole(),
                    "UpdateOrderStatus",
                    "Order",
                    orderId.ToString(),
                    $"Vendor updated order #{orderId} status to '{GetLocalizedStatusEn(request.NewStatus)}'.",
                    GetCurrentUserRoleAr(),
                    "تحديث حالة الطلب",
                    "طلب",
                    $"قام البائع بتحديث حالة الطلب رقم {orderId} إلى '{GetLocalizedStatusAr(request.NewStatus)}'."
                );
                return Ok(new { message = "Status updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get revenue statistics
        /// </summary>
        [HttpGet("analytics/revenue")]
        public async Task<IActionResult> GetRevenueStatistics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate
        )
        {
            try
            {
                var workshopId = await GetVendorWorkshopIdAsync();
                var statistics = await _vendorOrderService.GetVendorRevenueStatisticsAsync(
                    workshopId,
                    startDate,
                    endDate
                );
                return Ok(statistics);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get orders analytics
        /// </summary>
        [HttpGet("analytics/orders")]
        public async Task<IActionResult> GetOrdersAnalytics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate
        )
        {
            try
            {
                var workshopId = await GetVendorWorkshopIdAsync();
                var analytics = await _vendorOrderService.GetVendorOrderAnalyticsAsync(
                    workshopId,
                    startDate,
                    endDate
                );
                return Ok(analytics);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get dashboard metrics
        /// </summary>
        [HttpGet("dashboard/metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var workshopId = await GetVendorWorkshopIdAsync();
                var metrics = await _vendorOrderService.GetVendorDashboardMetricsAsync(workshopId);
                return Ok(metrics);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get activity report
        /// </summary>
        [HttpGet("reports/activity")]
        public async Task<IActionResult> GetActivityReport(
            [FromQuery] string reportType,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate
        )
        {
            try
            {
                var workshopId = await GetVendorWorkshopIdAsync();
                var report = await _vendorOrderService.GetVendorActivityReportAsync(
                    workshopId,
                    reportType,
                    startDate,
                    endDate
                );
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Propose a delivery date for a vendor order
        /// </summary>
        [HttpPut("orders/{orderId}/propose-date")]
        public async Task<IActionResult> ProposeDeliveryDate(
            int orderId,
            [FromBody] ProposeDeliveryDateRequestDto request
        )
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body cannot be null" });

                var workshopId = await GetVendorWorkshopIdAsync();
                await _vendorOrderService.ProposeDeliveryDateAsync(orderId, workshopId, request);

                await _adminAuditLogsService.CreateLogAsync(
                    GetUserId(),
                    GetCurrentUserName(),
                    GetCurrentUserRole(),
                    "ProposeDeliveryDate",
                    "VendorOrder",
                    orderId.ToString(),
                    $"Vendor proposed delivery date of {request.EstimatedDeliveryDate:yyyy-MM-dd} for order #{orderId}.",
                    GetCurrentUserRoleAr(),
                    "مقترح تاريخ التوصيل",
                    "طلب بائع",
                    $"قام البائع بتقديم مقترح تاريخ توصيل {request.EstimatedDeliveryDate:yyyy-MM-dd} للطلب رقم {orderId}."
                );

                return Ok(new { message = "Delivery date proposed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
