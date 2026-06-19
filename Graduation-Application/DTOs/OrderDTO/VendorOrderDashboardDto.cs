using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class VendorOrderDashboardDto
    {
        public int Id { get; set; }
        public int MasterOrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ItemCount { get; set; }
        public List<VendorOrderItemDto> Items { get; set; }
        public OrderStatusHistoryResponseDto? StatusHistory { get; set; }
    }
}
