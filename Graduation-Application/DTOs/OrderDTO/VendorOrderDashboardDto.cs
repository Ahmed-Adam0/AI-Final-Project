using System;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class VendorOrderDashboardDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ItemCount { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
    }
}
