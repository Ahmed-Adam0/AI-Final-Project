using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.Admin.AdminDashboardDTO
{
    public class AdminDashboardDto
    {
        public List<AdminSummaryCardDto> SummaryCards { get; set; } = new();
        public List<AdminActivityDto> LatestActivity { get; set; } = new();
        public List<AdminOrderListItemDto> LatestOrders { get; set; } = new();
        public List<AdminVendorListItemDto> LatestVendors { get; set; } = new();
        public List<AdminReviewListItemDto> LatestReviews { get; set; } = new();
        public List<AdminReportListItemDto> LatestReports { get; set; } = new();
        public AdminChartDto RevenueChart { get; set; } = new();
        public AdminChartDto OrdersChart { get; set; } = new();
    }

    public class AdminSummaryCardDto
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string IconClass { get; set; }
        public string TrendText { get; set; }
        public string TrendClass { get; set; } = "text-success";
    }

    public class AdminActivityDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BadgeText { get; set; }
        public string BadgeClass { get; set; } = "bg-secondary";
    }

    public class AdminOrderListItemDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string VendorName { get; set; }
        public string VendorNameAr { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }  // Payment status as string
        public DateTime CreatedAt { get; set; }
    }

    public class AdminVendorListItemDto
    {
        public int WorkshopId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Email { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
    }

    public class AdminReviewListItemDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminReportListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string VendorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DownloadUrl { get; set; }
    }

    public class AdminChartDto
    {
        public string ChartId { get; set; }
        public string Title { get; set; }
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
    }
}
