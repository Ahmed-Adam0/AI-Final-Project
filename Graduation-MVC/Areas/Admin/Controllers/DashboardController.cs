using System.Linq;
using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin", Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public DashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dto = await _adminDashboardService.GetDashboardAsync();
            return View(MapDashboard(dto));
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            var dto = await _adminDashboardService.GetDashboardAsync();
            return Json(
                new
                {
                    totalOrders = dto
                        .SummaryCards.FirstOrDefault(x => x.Title == "Total Orders")
                        ?.Value,
                    totalRevenue = dto
                        .SummaryCards.FirstOrDefault(x => x.Title == "Total Revenue")
                        ?.Value,
                    totalVendors = dto
                        .SummaryCards.FirstOrDefault(x => x.Title == "Total Vendors")
                        ?.Value,
                    totalCustomers = dto
                        .SummaryCards.FirstOrDefault(x => x.Title == "Total Customers")
                        ?.Value,
                    pendingOrders = dto
                        .SummaryCards.FirstOrDefault(x => x.Title == "Pending Orders")
                        ?.Value,
                    completedOrders = dto
                        .SummaryCards.FirstOrDefault(x => x.Title == "Completed Orders")
                        ?.Value,
                    revenueSeries = dto.RevenueChart.Values,
                    orderSeries = dto.OrdersChart.Values,
                }
            );
        }

        private static AdminDashboardViewModel MapDashboard(AdminDashboardDto dto)
        {
            return new AdminDashboardViewModel
            {
                SummaryCards = dto
                    .SummaryCards.Select(x => new AdminDashboardSummaryCardViewModel
                    {
                        Title = x.Title,
                        Value = x.Value,
                        IconClass = x.IconClass,
                        TrendText = x.TrendText,
                        TrendClass = x.TrendClass,
                    })
                    .ToList(),
                LatestActivity = dto
                    .LatestActivity.Select(x => new AdminDashboardActivityViewModel
                    {
                        Title = x.Title,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        BadgeText = x.BadgeText,
                        BadgeClass = x.BadgeClass,
                    })
                    .ToList(),
                LatestOrders = dto
                    .LatestOrders.Select(x => new AdminDashboardOrderViewModel
                    {
                        Id = x.Id,
                        OrderNumber = x.OrderNumber,
                        CustomerName = x.CustomerName,
                        VendorName = x.VendorName,
                        TotalAmount = x.TotalAmount,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt,
                    })
                    .ToList(),
                LatestVendors = dto
                    .LatestVendors.Select(x => new AdminDashboardVendorViewModel
                    {
                        Name = x.Name,
                        Email = x.Email,
                        Revenue = x.Revenue,
                        OrdersCount = x.OrdersCount,
                    })
                    .ToList(),
                LatestReviews = dto
                    .LatestReviews.Select(x => new AdminDashboardReviewViewModel
                    {
                        CustomerName = x.CustomerName,
                        ProductName = x.ProductName,
                        Rating = x.Rating,
                        Comment = x.Comment,
                        CreatedAt = x.CreatedAt,
                    })
                    .ToList(),
                LatestReports = dto
                    .LatestReports.Select(x => new AdminDashboardReportViewModel
                    {
                        Title = x.Title,
                        Type = x.Type,
                        CreatedAt = x.CreatedAt,
                        DownloadUrl = x.DownloadUrl,
                    })
                    .ToList(),
                QuickActions = new[]
                {
                    new AdminDashboardQuickActionViewModel
                    {
                        Title = "Create Vendor",
                        Description = "Register a new vendor account",
                        Url = "/Admin/Vendors/Create",
                        IconClass = "fa-solid fa-store",
                    },
                    new AdminDashboardQuickActionViewModel
                    {
                        Title = "View Orders",
                        Description = "Open the order management panel",
                        Url = "/Admin/Orders",
                        IconClass = "fa-solid fa-cart-shopping",
                        CssClass = "btn-premium-secondary",
                    },
                    new AdminDashboardQuickActionViewModel
                    {
                        Title = "Generate Report",
                        Description = "Build a new operational report",
                        Url = "/Admin/Reports",
                        IconClass = "fa-solid fa-file-lines",
                        CssClass = "btn-premium-secondary",
                    },
                    new AdminDashboardQuickActionViewModel
                    {
                        Title = "Platform Settings",
                        Description = "Update platform configuration",
                        Url = "/Admin/Settings",
                        IconClass = "fa-solid fa-gear",
                        CssClass = "btn-premium-secondary",
                    },
                },
                RevenueChart = new AdminDashboardChartViewModel
                {
                    ChartId = dto.RevenueChart.ChartId,
                    Title = dto.RevenueChart.Title,
                    Labels = dto.RevenueChart.Labels,
                    Values = dto.RevenueChart.Values,
                },
                OrdersChart = new AdminDashboardChartViewModel
                {
                    ChartId = dto.OrdersChart.ChartId,
                    Title = dto.OrdersChart.Title,
                    Labels = dto.OrdersChart.Labels,
                    Values = dto.OrdersChart.Values,
                },
            };
        }
    }
}
