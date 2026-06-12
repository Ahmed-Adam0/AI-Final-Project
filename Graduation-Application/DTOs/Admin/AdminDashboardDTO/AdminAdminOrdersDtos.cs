using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.Admin.AdminDashboardDTO
{
    public class AdminOrdersFilterDto
    {
        public string Search { get; set; }
        public string Status { get; set; }
        public string VendorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminOrdersPageDto
    {
        public AdminOrdersFilterDto Filter { get; set; } = new();
        public List<AdminOrderListItemDto> Orders { get; set; } = new();
        public AdminPagingDto Paging { get; set; } = new();
        public List<AdminStatusOptionDto> StatusOptions { get; set; } = new();
        public List<AdminVendorOptionDto> Vendors { get; set; } = new();
    }

    public class AdminPagingDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public class AdminOrderDetailsDto
    {
        public AdminOrderSummaryDto Summary { get; set; } = new();
        public List<AdminOrderItemDto> Items { get; set; } = new();
        public List<AdminOrderTimelineItemDto> Timeline { get; set; } = new();
        public AdminOrderCustomerDto Customer { get; set; } = new();
        public AdminOrderVendorDto Vendor { get; set; } = new();
    }

    public class AdminOrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminOrderItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class AdminOrderTimelineItemDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusClass { get; set; } = "bg-secondary";
    }

    public class AdminOrderCustomerDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class AdminOrderVendorDto
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal RevenueShare { get; set; }
    }

    public class AdminStatusOptionDto
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class AdminVendorOptionDto
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }
}
