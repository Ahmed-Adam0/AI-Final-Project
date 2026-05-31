using System;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class VendorOrdersFilterDto
    {
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomerName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt"; // CreatedAt, TotalPrice
        public bool SortDescending { get; set; } = true;
    }
}
