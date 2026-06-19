using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class VendorOrderDetailsDto
    {
        public int Id { get; set; }
        public int MasterOrderId { get; set; }
        public string UserId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<VendorOrderItemDto> Items { get; set; }
        public OrderStatusHistoryResponseDto? StatusHistory { get; set; }
    }

    public class VendorOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
