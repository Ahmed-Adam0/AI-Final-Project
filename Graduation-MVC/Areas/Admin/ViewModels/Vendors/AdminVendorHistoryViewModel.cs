using System;
using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.Vendors
{
    public class AdminVendorHistoryViewModel
    {
        public AdminVendorHistoryPagingViewModel Paging { get; set; } = new();
        public List<AdminVendorHistoryItemViewModel> Items { get; set; } = new();
    }

    public class AdminVendorHistoryItemViewModel
    {
        public DateTime CreatedAt { get; set; }
        public int WorkshopId { get; set; }
        public string WorkshopName { get; set; }
        public string Action { get; set; }
        public string? AdminName { get; set; }
        public string? Details { get; set; }
        public string BadgeClass { get; set; } = "bg-secondary";
    }

    public class AdminVendorHistoryPagingViewModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}

