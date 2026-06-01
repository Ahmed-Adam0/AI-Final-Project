using System;
using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.Analytics
{
    public class AdminAnalyticsViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public IReadOnlyList<AdminAnalyticsMetricViewModel> Metrics { get; set; } = Array.Empty<AdminAnalyticsMetricViewModel>();
        public AdminAnalyticsChartViewModel OrdersPerDayChart { get; set; } = new();
        public AdminAnalyticsChartViewModel RevenueGrowthChart { get; set; } = new();
        public AdminAnalyticsChartViewModel ForecastChart { get; set; } = new();
        public IReadOnlyList<AdminRankingItemViewModel> TopVendors { get; set; } = Array.Empty<AdminRankingItemViewModel>();
        public IReadOnlyList<AdminRankingItemViewModel> TopCategories { get; set; } = Array.Empty<AdminRankingItemViewModel>();
    }

    public class AdminAnalyticsMetricViewModel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string SubText { get; set; }
        public string IconClass { get; set; }
        public string TrendClass { get; set; } = "text-success";
    }

    public class AdminAnalyticsChartViewModel
    {
        public string ChartId { get; set; }
        public string Title { get; set; }
        public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
        public IReadOnlyList<decimal> Values { get; set; } = Array.Empty<decimal>();
    }

    public class AdminRankingItemViewModel
    {
        public int Rank { get; set; }
        public string Name { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
    }
}
