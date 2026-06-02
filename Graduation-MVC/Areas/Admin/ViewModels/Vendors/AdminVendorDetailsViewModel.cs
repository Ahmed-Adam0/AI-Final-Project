using System;
using System.Collections.Generic;
using Graduation_domain.Enums;

namespace Graduation_MVC.Areas.Admin.ViewModels.Vendors
{
    public class AdminVendorDetailsViewModel
    {
        public AdminVendorProfileViewModel Profile { get; set; } = new();
        public AdminVendorBusinessViewModel Business { get; set; } = new();
        public AdminVendorVerificationViewModel Verification { get; set; } = new();
        public AdminVendorAccountViewModel Account { get; set; } = new();
        public AdminVendorOrdersStatsViewModel OrdersStats { get; set; } = new();
        public AdminVendorRevenueStatsViewModel RevenueStats { get; set; } = new();
        public List<AdminVendorVerificationHistoryItemViewModel> VerificationHistory { get; set; } =
            new();
        public List<AdminVendorAccountStatusHistoryItemViewModel> AccountStatusHistory { get; set; } =
            new();

        public AdminVendorActionsViewModel Actions { get; set; } = new();
    }

    public class AdminVendorProfileViewModel
    {
        public int WorkshopId { get; set; }
        public string VendorUserId { get; set; }
        public string VendorName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class AdminVendorBusinessViewModel
    {
        public string WorkshopNameAr { get; set; }
        public string WorkshopNameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string? LogoUrl { get; set; }
        public decimal? Rating { get; set; }
        public string? AddressSummary { get; set; }
    }

    public class AdminVendorVerificationViewModel
    {
        public VendorVerificationStatus Status { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string? VerifiedByAdminName { get; set; }
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class AdminVendorAccountViewModel
    {
        public VendorAccountStatus Status { get; set; }
        public DateTime? StatusChangedAt { get; set; }
        public string? StatusChangedByAdminName { get; set; }
    }

    public class AdminVendorOrdersStatsViewModel
    {
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int PendingOrders { get; set; }

        public int CancelledOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ReadyforPickup { get; set; }
    }

    public class AdminVendorRevenueStatsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal DeliveredRevenue { get; set; }
    }

    public class AdminVendorVerificationHistoryItemViewModel
    {
        public DateTime CreatedAt { get; set; }
        public VendorVerificationStatus OldStatus { get; set; }
        public VendorVerificationStatus NewStatus { get; set; }
        public string? AdminName { get; set; }
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class AdminVendorAccountStatusHistoryItemViewModel
    {
        public DateTime CreatedAt { get; set; }
        public VendorAccountStatus OldStatus { get; set; }
        public VendorAccountStatus NewStatus { get; set; }
        public string? AdminName { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class AdminVendorActionsViewModel
    {
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanSuspend { get; set; }
        public bool CanActivate { get; set; }
    }
}
