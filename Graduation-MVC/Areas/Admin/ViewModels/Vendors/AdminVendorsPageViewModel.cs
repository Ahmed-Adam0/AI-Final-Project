using System;
using System.Collections.Generic;
using Graduation_domain.Enums;

namespace Graduation_MVC.Areas.Admin.ViewModels.Vendors
{
    public class AdminVendorsPageViewModel
    {
        public AdminVendorFilterViewModel Filter { get; set; } = new();
        public List<AdminVendorListItemViewModel> Vendors { get; set; } = new();
        public AdminVendorsPagingViewModel Paging { get; set; } = new();
        public List<AdminStatusOptionViewModel> VerificationStatusOptions { get; set; } = new();
        public List<AdminStatusOptionViewModel> AccountStatusOptions { get; set; } = new();
        public AdminVendorStatisticsViewModel Statistics { get; set; } = new();
        public bool PendingOnly { get; set; }
    }

    public class AdminVendorListItemViewModel
    {
        public int WorkshopId { get; set; }
        public string WorkshopName { get; set; }
        public string? WorkshopNameAr { get; set; }
        public string VendorName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime RegisteredAt { get; set; }
        public VendorVerificationStatus VerificationStatus { get; set; }
        public VendorAccountStatus AccountStatus { get; set; }
    }

    public class AdminVendorsPagingViewModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminStatusOptionViewModel
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class AdminVendorStatisticsViewModel
    {
        public int TotalVendors { get; set; }
        public int PendingVendors { get; set; }
        public int ApprovedVendors { get; set; }
        public int RejectedVendors { get; set; }
        public int ActiveVendors { get; set; }
        public int SuspendedVendors { get; set; }
    }
}

