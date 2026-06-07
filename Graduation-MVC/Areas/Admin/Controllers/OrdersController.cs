using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class OrdersController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IOrderService _orderService;

        public OrdersController(
            IAdminDashboardService adminDashboardService,
            IOrderService orderService
        )
        {
            _adminDashboardService = adminDashboardService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] AdminOrderFilterViewModel filter)
        {
            var dto = await _adminDashboardService.GetOrdersPageAsync(
                new AdminOrdersFilterDto
                {
                    Search = filter.Search,
                    Status = filter.Status,
                    VendorId = filter.VendorId,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    SortBy = filter.SortBy,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                }
            );

            return View(
                new AdminOrdersPageViewModel
                {
                    Filter = filter,
                    Orders = dto
                        .Orders.Select(x => new AdminOrderListItemViewModel
                        {
                            Id = x.Id,
                            OrderNumber = x.OrderNumber,
                            CustomerName = x.CustomerName,
                            VendorName = x.VendorName,
                            TotalPrice = x.TotalAmount,
                            Status = x.Status,
                            PaymentStatus = x.PaymentStatus,
                            CreatedAt = x.CreatedAt,
                        })
                        .ToList(),
                    Paging = new AdminOrderPagingViewModel
                    {
                        Page = dto.Paging.Page,
                        PageSize = dto.Paging.PageSize,
                        TotalCount = dto.Paging.TotalCount,
                        TotalPages = dto.Paging.TotalPages,
                    },
                    StatusOptions = dto
                        .StatusOptions.Select(x => new AdminOrderStatusOptionViewModel
                        {
                            Value = x.Value,
                            Label = x.Label,
                        })
                        .ToList(),
                    Vendors = dto
                        .Vendors.Select(x => new AdminVendorOptionViewModel
                        {
                            Value = x.Value,
                            Label = x.Label,
                        })
                        .ToList(),
                }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _adminDashboardService.GetOrderDetailsAsync(id);
            if (dto == null)
            {
                return NotFound();
            }

            return View(
                new AdminOrderDetailsViewModel
                {
                    Summary = new AdminOrderSummaryViewModel
                    {
                        Id = dto.Summary.Id,
                        OrderNumber = dto.Summary.OrderNumber,
                        Status = dto.Summary.Status,
                        PaymentStatus = dto.Summary.PaymentStatus,
                        Subtotal = dto.Summary.Subtotal,
                        ShippingFee = dto.Summary.ShippingFee,
                        Tax = dto.Summary.Tax,
                        TotalPrice = dto.Summary.TotalPrice,
                        CreatedAt = dto.Summary.CreatedAt,
                    },
                    Customer = new AdminOrderCustomerViewModel
                    {
                        Name = dto.Customer.Name,
                        Email = dto.Customer.Email,
                        Phone = dto.Customer.Phone,
                        Address = dto.Customer.Address,
                    },
                    Vendor = new AdminOrderVendorViewModel
                    {
                        Name = dto.Vendor.Name,
                        Email = dto.Vendor.Email,
                        Phone = dto.Vendor.Phone,
                        RevenueShare = dto.Vendor.RevenueShare,
                    },
                    Items = dto
                        .Items.Select(x => new AdminOrderItemViewModel
                        {
                            Id = x.Id,
                            ProductName = x.ProductName,
                            Quantity = x.Quantity,
                            UnitPrice = x.UnitPrice,
                            LineTotal = x.LineTotal,
                        })
                        .ToList(),
                    Timeline = dto
                        .Timeline.Select(x => new AdminOrderTimelineItemViewModel
                        {
                            Title = x.Title,
                            Description = x.Description,
                            CreatedAt = x.CreatedAt,
                            StatusClass = x.StatusClass,
                        })
                        .ToList(),
                }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            await _orderService.UpdateOrderStatusAsync(id, "Confirmed");
            TempData["SuccessMessage"] = $"Order #{id} status changed to Confirmed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public IActionResult Export(int id)
        {
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
