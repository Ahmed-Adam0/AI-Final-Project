using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.Admin.AdminDashboardDTO
{
    public class AdminAnalyticsFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class AdminAnalyticsDto
    {
        public AdminAnalyticsFilterDto Filter { get; set; } = new();
        public List<AdminAnalyticsMetricDto> Metrics { get; set; } = new();
        public AdminChartDto OrdersPerDayChart { get; set; } = new();
        public AdminChartDto RevenueGrowthChart { get; set; } = new();
        public AdminChartDto ForecastChart { get; set; } = new();
        public List<AdminRankingItemDto> TopVendors { get; set; } = new();
        public List<AdminRankingItemDto> TopCategories { get; set; } = new();
    }

    public class AdminAnalyticsMetricDto
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string SubText { get; set; }
        public string IconClass { get; set; }
        public string TrendClass { get; set; } = "text-success";
    }

    public class AdminRankingItemDto
    {
        public int Rank { get; set; }
        public string Name { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
    }
}
