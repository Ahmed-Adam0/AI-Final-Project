using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class VendorRevenueStatisticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal DailyRevenue { get; set; }
        public int CompletedOrdersCount { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
        public List<OrdersByStatusDto> OrdersByStatus { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = new();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
    }

    public class OrdersByStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }
}
