using System;

namespace Graduation_Application.DTOs.ProductDTO
{
    /// <summary>
    /// DTO for vendor product statistics (dashboard use)
    /// </summary>
    public class VendorProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
