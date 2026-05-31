namespace Graduation_Application.DTOs.OrderDTO
{
    public class VendorOrderAnalyticsDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public int InProgressOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double AverageCompletionTimeHours { get; set; }
        public decimal CompletionRate { get; set; }
    }
}
