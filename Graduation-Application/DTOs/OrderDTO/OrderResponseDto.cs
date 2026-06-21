using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CustomerVendorOrderDto> VendorOrders { get; set; } = [];
        public OrderStatusHistoryResponseDto? StatusHistory { get; set; }
        public string? PaymentUrl { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid";
    }

    public class CustomerVendorOrderDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public bool CanApprove { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = [];
    }
}
