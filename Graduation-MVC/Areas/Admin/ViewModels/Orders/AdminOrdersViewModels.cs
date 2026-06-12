using System;
using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.Orders
{
    public class AdminOrdersPageViewModel
    {
        public AdminOrderFilterViewModel Filter { get; set; } = new();
        public IReadOnlyList<AdminOrderListItemViewModel> Orders { get; set; } =
            Array.Empty<AdminOrderListItemViewModel>();
        public AdminOrderPagingViewModel Paging { get; set; } = new();
        public IReadOnlyList<AdminOrderStatusOptionViewModel> StatusOptions { get; set; } =
            Array.Empty<AdminOrderStatusOptionViewModel>();
        public IReadOnlyList<AdminVendorOptionViewModel> Vendors { get; set; } =
            Array.Empty<AdminVendorOptionViewModel>();
    }

    public class AdminOrderFilterViewModel
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

    public class AdminOrderPagingViewModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public class AdminOrderListItemViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string VendorName { get; set; }
        public string VendorNameAr { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminOrderDetailsViewModel
    {
        public AdminOrderSummaryViewModel Summary { get; set; } = new();
        public IReadOnlyList<AdminOrderItemViewModel> Items { get; set; } =
            Array.Empty<AdminOrderItemViewModel>();
        public IReadOnlyList<AdminOrderTimelineItemViewModel> Timeline { get; set; } =
            Array.Empty<AdminOrderTimelineItemViewModel>();
        public AdminOrderCustomerViewModel Customer { get; set; } = new();
        public AdminOrderVendorViewModel Vendor { get; set; } = new();
    }

    public class AdminOrderSummaryViewModel
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

    public class AdminOrderItemViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class AdminOrderTimelineItemViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusClass { get; set; } = "bg-secondary";
    }

    public class AdminOrderCustomerViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class AdminOrderVendorViewModel
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal RevenueShare { get; set; }
    }

    public class AdminOrderStatusOptionViewModel
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class AdminVendorOptionViewModel
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }
}
