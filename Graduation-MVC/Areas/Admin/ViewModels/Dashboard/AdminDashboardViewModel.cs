using System;
using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public IReadOnlyList<AdminDashboardSummaryCardViewModel> SummaryCards { get; set; } = Array.Empty<AdminDashboardSummaryCardViewModel>();
        public IReadOnlyList<AdminDashboardActivityViewModel> LatestActivity { get; set; } = Array.Empty<AdminDashboardActivityViewModel>();
        public IReadOnlyList<AdminDashboardOrderViewModel> LatestOrders { get; set; } = Array.Empty<AdminDashboardOrderViewModel>();
        public IReadOnlyList<AdminDashboardVendorViewModel> LatestVendors { get; set; } = Array.Empty<AdminDashboardVendorViewModel>();
        public IReadOnlyList<AdminDashboardReviewViewModel> LatestReviews { get; set; } = Array.Empty<AdminDashboardReviewViewModel>();
        public IReadOnlyList<AdminDashboardReportViewModel> LatestReports { get; set; } = Array.Empty<AdminDashboardReportViewModel>();
        public IReadOnlyList<AdminDashboardQuickActionViewModel> QuickActions { get; set; } = Array.Empty<AdminDashboardQuickActionViewModel>();
        public AdminDashboardChartViewModel RevenueChart { get; set; } = new();
        public AdminDashboardChartViewModel OrdersChart { get; set; } = new();
    }

    public class AdminDashboardSummaryCardViewModel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string IconClass { get; set; }
        public string TrendText { get; set; }
        public string TrendClass { get; set; } = "text-success";
    }

    public class AdminDashboardActivityViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BadgeText { get; set; }
        public string BadgeClass { get; set; } = "bg-secondary";
    }

    public class AdminDashboardOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string VendorName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminDashboardVendorViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
    }

    public class AdminDashboardReviewViewModel
    {
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminDashboardReportViewModel
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DownloadUrl { get; set; }
    }

    public class AdminDashboardQuickActionViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string IconClass { get; set; }
        public string CssClass { get; set; } = "btn-premium-primary";
    }

    public class AdminDashboardChartViewModel
    {
        public string ChartId { get; set; }
        public string Title { get; set; }
        public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
        public IReadOnlyList<decimal> Values { get; set; } = Array.Empty<decimal>();
    }
}
