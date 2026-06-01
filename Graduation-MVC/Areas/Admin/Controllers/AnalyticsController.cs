using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin", Policy = "AdminOnly")]
    public class AnalyticsController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AnalyticsController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var dto = await _adminDashboardService.GetAnalyticsAsync(
                new AdminAnalyticsFilterDto { FromDate = fromDate, ToDate = toDate }
            );
            return View(Map(dto, fromDate, toDate));
        }

        [HttpGet]
        public async Task<IActionResult> Data(DateTime? fromDate, DateTime? toDate)
        {
            var dto = await _adminDashboardService.GetAnalyticsAsync(
                new AdminAnalyticsFilterDto { FromDate = fromDate, ToDate = toDate }
            );
            return Json(
                new
                {
                    metrics = dto.Metrics,
                    ordersPerDay = dto.OrdersPerDayChart,
                    revenueGrowth = dto.RevenueGrowthChart,
                    forecast = dto.ForecastChart,
                    topVendors = dto.TopVendors,
                    topCategories = dto.TopCategories,
                }
            );
        }

        [HttpGet]
        public IActionResult Export(DateTime? fromDate, DateTime? toDate)
        {
            TempData["SuccessMessage"] = "Analytics export requested successfully.";
            return RedirectToAction(nameof(Index), new { fromDate, toDate });
        }

        private static AdminAnalyticsViewModel Map(
            AdminAnalyticsDto dto,
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            return new AdminAnalyticsViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Metrics = dto
                    .Metrics.Select(x => new AdminAnalyticsMetricViewModel
                    {
                        Title = x.Title,
                        Value = x.Value,
                        SubText = x.SubText,
                        IconClass = x.IconClass,
                        TrendClass = x.TrendClass,
                    })
                    .ToList(),
                OrdersPerDayChart = new AdminAnalyticsChartViewModel
                {
                    ChartId = dto.OrdersPerDayChart.ChartId,
                    Title = dto.OrdersPerDayChart.Title,
                    Labels = dto.OrdersPerDayChart.Labels,
                    Values = dto.OrdersPerDayChart.Values,
                },
                RevenueGrowthChart = new AdminAnalyticsChartViewModel
                {
                    ChartId = dto.RevenueGrowthChart.ChartId,
                    Title = dto.RevenueGrowthChart.Title,
                    Labels = dto.RevenueGrowthChart.Labels,
                    Values = dto.RevenueGrowthChart.Values,
                },
                ForecastChart = new AdminAnalyticsChartViewModel
                {
                    ChartId = dto.ForecastChart.ChartId,
                    Title = dto.ForecastChart.Title,
                    Labels = dto.ForecastChart.Labels,
                    Values = dto.ForecastChart.Values,
                },
                TopVendors = dto
                    .TopVendors.Select(x => new AdminRankingItemViewModel
                    {
                        Rank = x.Rank,
                        Name = x.Name,
                        Revenue = x.Revenue,
                        OrdersCount = x.OrdersCount,
                    })
                    .ToList(),
                TopCategories = dto
                    .TopCategories.Select(x => new AdminRankingItemViewModel
                    {
                        Rank = x.Rank,
                        Name = x.Name,
                        Revenue = x.Revenue,
                        OrdersCount = x.OrdersCount,
                    })
                    .ToList(),
            };
        }
    }
}
