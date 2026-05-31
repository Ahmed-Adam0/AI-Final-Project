namespace Graduation_Application.DTOs.OrderDTO
{
    public class VendorDashboardMetricsDto
    {
        public int TotalOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewCustomersCount { get; set; }
        public decimal OrderGrowthPercentage { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
