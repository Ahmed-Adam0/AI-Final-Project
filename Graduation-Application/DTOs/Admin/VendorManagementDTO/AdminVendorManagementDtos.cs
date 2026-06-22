using System;
using System.Collections.Generic;
using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_domain.Enums;

namespace Graduation_Application.DTOs.Admin.VendorManagementDTO
{
    public class AdminVendorsFilterDto
    {
        public string? Search { get; set; }
        public string? VerificationStatus { get; set; }
        public string? AccountStatus { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminVendorsPageDto
    {
        public AdminVendorsFilterDto Filter { get; set; } = new();
        public List<AdminVendorListItemDto> Vendors { get; set; } = new();
        public AdminPagingDto Paging { get; set; } = new();
        public List<AdminStatusOptionDto> VerificationStatusOptions { get; set; } = new();
        public List<AdminStatusOptionDto> AccountStatusOptions { get; set; } = new();
        public AdminVendorStatisticsDto Statistics { get; set; } = new();
    }

    public class AdminVendorListItemDto
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
        public bool IsVerified { get; set; }
    }

    public class AdminVendorStatisticsDto
    {
        public int TotalVendors { get; set; }
        public int PendingVendors { get; set; }
        public int ApprovedVendors { get; set; }
        public int RejectedVendors { get; set; }
        public int ActiveVendors { get; set; }
        public int SuspendedVendors { get; set; }
    }

    public class AdminVendorDetailsDto
    {
        public AdminVendorProfileDto Profile { get; set; } = new();
        public AdminVendorBusinessDto Business { get; set; } = new();
        public AdminVendorContactDto Contact { get; set; } = new();
        public AdminVendorVerificationDto Verification { get; set; } = new();
        public AdminVendorAccountDto Account { get; set; } = new();
        public AdminVendorOrdersStatsDto OrdersStats { get; set; } = new();
        public AdminVendorRevenueStatsDto RevenueStats { get; set; } = new();
        public List<AdminVendorVerificationHistoryItemDto> VerificationHistory { get; set; } =
            new();
        public List<AdminVendorAccountStatusHistoryItemDto> AccountStatusHistory { get; set; } =
            new();
    }

    public class AdminVendorProfileDto
    {
        public int WorkshopId { get; set; }
        public string VendorUserId { get; set; }
        public string VendorName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class AdminVendorBusinessDto
    {
        public string WorkshopNameAr { get; set; }
        public string WorkshopNameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string? LogoUrl { get; set; }
        public decimal? Rating { get; set; }
        public string? AddressSummary { get; set; }
    }

    public class AdminVendorContactDto
    {
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class AdminVendorVerificationDto
    {
        public VendorVerificationStatus Status { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string? VerifiedByAdminName { get; set; }
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class AdminVendorAccountDto
    {
        public VendorAccountStatus Status { get; set; }
        public DateTime? StatusChangedAt { get; set; }
        public string? StatusChangedByAdminName { get; set; }
    }

    public class AdminVendorOrdersStatsDto
    {
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ReadyforPickupOrders { get; set; }
    }

    public class AdminVendorRevenueStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal DeliveredRevenue { get; set; }
    }

    public class AdminVendorVerificationHistoryItemDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public VendorVerificationStatus OldStatus { get; set; }
        public VendorVerificationStatus NewStatus { get; set; }
        public string? AdminName { get; set; }
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class AdminVendorAccountStatusHistoryItemDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public VendorAccountStatus OldStatus { get; set; }
        public VendorAccountStatus NewStatus { get; set; }
        public string? AdminName { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class AdminVendorHistoryPageDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public AdminPagingDto Paging { get; set; } = new();
        public List<AdminVendorUnifiedHistoryItemDto> Items { get; set; } = new();
    }

    public class AdminVendorUnifiedHistoryItemDto
    {
        public DateTime CreatedAt { get; set; }
        public int WorkshopId { get; set; }
        public string WorkshopName { get; set; }
        public string Action { get; set; }
        public string? AdminName { get; set; }
        public string? Details { get; set; }
        public string BadgeClass { get; set; } = "bg-secondary";
    }
}
