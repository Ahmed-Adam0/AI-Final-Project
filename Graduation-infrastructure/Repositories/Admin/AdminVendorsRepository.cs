using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.VendorManagementDTO;
using Graduation_Application.IRepositories.Admin;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Graduation_infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.Repositories.Admin
{
    public class AdminVendorsRepository : IAdminVendorsRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminVendorsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AdminVendorStatisticsDto> GetStatisticsAsync()
        {
            var workshops = _db.Workshops.Include(w => w.User).AsNoTracking();

            var total = await workshops.CountAsync();
            var pending = await workshops.CountAsync(w => w.User.IsActive == false);
            var approved = await workshops.CountAsync(w => w.User.IsActive == true);
            var rejected = await workshops.CountAsync(w =>
                w.VerificationStatus == VendorVerificationStatus.inActive
            );
            var active = await workshops.CountAsync(w => w.IsActive == true);
            var suspended = await workshops.CountAsync(w => w.IsActive == false);

            return new AdminVendorStatisticsDto
            {
                TotalVendors = total,
                PendingVendors = pending,
                ApprovedVendors = approved,
                RejectedVendors = rejected,
                ActiveVendors = active,
                SuspendedVendors = suspended,
            };
        }

        public async Task<AdminVendorsPageDto> GetVendorsPageAsync(
            AdminVendorsFilterDto filter,
            bool pendingOnly
        )
        {
            filter ??= new AdminVendorsFilterDto();

            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            var page = filter.Page <= 0 ? 1 : filter.Page;

            var query = _db.Workshops.AsNoTracking().Include(w => w.User).AsQueryable();

            // Backward-compatibility for existing data that only used IsVerified
            query = query.Select(w => new Workshop
            {
                Id = w.Id,
                UserId = w.UserId,
                User = w.User,
                WorkshopNameAr = w.WorkshopNameAr,
                WorkshopNameEn = w.WorkshopNameEn,
                DescriptionAr = w.DescriptionAr,
                DescriptionEn = w.DescriptionEn,
                LogoUrl = w.LogoUrl,
                Rating = w.Rating,
                IsVerified = w.IsVerified,
                CreatedAt = w.CreatedAt,
                VerificationStatus =
                    w.User.IsActive == false
                        ? VendorVerificationStatus.inActive
                        : VendorVerificationStatus.Active,

                AccountStatus =
                    w.User.IsActive == true
                        ? VendorAccountStatus.Approved
                        : VendorAccountStatus.Pending,
            });

            if (pendingOnly)
            {
                query = query.Where(w => w.VerificationStatus == VendorVerificationStatus.inActive);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(w =>
                    w.WorkshopNameEn.Contains(s)
                    || w.WorkshopNameAr.Contains(s)
                    || (
                        w.User != null
                        && (
                            w.User.FullName.Contains(s)
                            || (w.User.Email != null && w.User.Email.Contains(s))
                            || (w.User.PhoneNumber != null && w.User.PhoneNumber.Contains(s))
                        )
                    )
                );
            }

            if (
                !string.IsNullOrWhiteSpace(filter.VerificationStatus)
                && Enum.TryParse<VendorVerificationStatus>(
                    filter.VerificationStatus,
                    true,
                    out var verification
                )
            )
            {
                query = query.Where(w => w.VerificationStatus == verification);
            }

            if (
                !string.IsNullOrWhiteSpace(filter.AccountStatus)
                && Enum.TryParse<VendorAccountStatus>(filter.AccountStatus, true, out var account)
            )
            {
                query = query.Where(w => w.AccountStatus == account);
            }

            // Sorting
            query = (filter.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "name_asc" => query.OrderBy(w => w.WorkshopNameEn),
                "name_desc" => query.OrderByDescending(w => w.WorkshopNameEn),
                "date_asc" => query.OrderBy(w => w.CreatedAt),
                "date_desc" => query.OrderByDescending(w => w.CreatedAt),
                _ => query.OrderByDescending(w => w.CreatedAt),
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var vendors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new AdminVendorListItemDto
                {
                    WorkshopId = w.Id,
                    WorkshopName = w.WorkshopNameEn,
                    VendorName = w.User != null ? w.User.FullName : w.UserId,
                    Email = w.User != null ? w.User.Email : null,
                    Phone = w.User != null ? w.User.PhoneNumber : null,
                    RegisteredAt = w.CreatedAt,
                    VerificationStatus = w.VerificationStatus,
                    AccountStatus = w.AccountStatus,
                    IsVerified = w.IsVerified,
                })
                .ToListAsync();

            var stats = await GetStatisticsAsync();

            return new AdminVendorsPageDto
            {
                Filter = filter,
                Vendors = vendors,
                Statistics = stats,
                Paging = new Graduation_Application.DTOs.Admin.AdminDashboardDTO.AdminPagingDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                },
                VerificationStatusOptions =
                    new List<Graduation_Application.DTOs.Admin.AdminDashboardDTO.AdminStatusOptionDto>
                    {
                        new() { Value = string.Empty, Label = "All Verification Statuses" },
                        new()
                        {
                            Value = VendorVerificationStatus.inActive.ToString(),
                            Label = "Inactive",
                        },
                        new()
                        {
                            Value = VendorVerificationStatus.Active.ToString(),
                            Label = "Active",
                        },
                    },
                AccountStatusOptions =
                    new List<Graduation_Application.DTOs.Admin.AdminDashboardDTO.AdminStatusOptionDto>
                    {
                        new() { Value = string.Empty, Label = "All Account Statuses" },
                        new() { Value = VendorAccountStatus.Approved.ToString(), Label = "Active" },
                        new()
                        {
                            Value = VendorAccountStatus.Suspended.ToString(),
                            Label = "Suspended",
                        },
                    },
            };
        }

        public async Task<AdminVendorDetailsDto?> GetVendorDetailsAsync(int workshopId)
        {
            var workshop = await _db
                .Workshops.AsNoTracking()
                .Include(w => w.User)
                .Include(w => w.WorkshopAddress)
                .Include(w => w.VerifiedByAdmin)
                .Include(w => w.AccountStatusChangedByAdmin)
                .FirstOrDefaultAsync(w => w.Id == workshopId);

            if (workshop == null)
                return null;

            // Backward-compatibility: IsVerified previously used alone
            var verificationStatus =
                workshop.User.IsActive == false
                    ? VendorVerificationStatus.inActive
                    : VendorVerificationStatus.Active;

            var ordersQuery = _db.Orders.AsNoTracking().Where(o => o.WorkshopId == workshopId);
            var totalOrders = await ordersQuery.CountAsync();
            var deliveredOrders = await ordersQuery.CountAsync(o => o.Status == "Delivered");
            var pendingOrders = await ordersQuery.CountAsync(o => o.Status == "Pending");
            var CancelledOrders = await ordersQuery.CountAsync(o => o.Status == "Cancelled");
            var InProgressOrders = await ordersQuery.CountAsync(o => o.Status == "InProgress");
            var ConfirmedOrders = await ordersQuery.CountAsync(o => o.Status == "Confirmed");
            var ReadyforPickup = await ordersQuery.CountAsync(o => o.Status == "ReadyforPickup");

            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;
            var deliveredRevenue =
                await ordersQuery
                    .Where(o => o.Status == "Delivered")
                    .SumAsync(o => (decimal?)o.TotalPrice)
                ?? 0m;

            var verificationHistory = await _db
                .VendorVerificationHistory.AsNoTracking()
                .Where(h => h.WorkshopId == workshopId)
                .Include(h => h.PerformedByAdmin)
                .OrderByDescending(h => h.CreatedAt)
                .Take(50)
                .Select(h => new AdminVendorVerificationHistoryItemDto
                {
                    Id = h.Id,
                    CreatedAt = h.CreatedAt,
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    AdminName =
                        h.PerformedByAdmin != null
                            ? h.PerformedByAdmin.FullName
                            : h.PerformedByAdminId,
                    Notes = h.Notes,
                    RejectionReason = h.RejectionReason,
                })
                .ToListAsync();

            var accountHistory = await _db
                .VendorAccountStatusHistory.AsNoTracking()
                .Where(h => h.WorkshopId == workshopId)
                .Include(h => h.PerformedByAdmin)
                .OrderByDescending(h => h.CreatedAt)
                .Take(50)
                .Select(h => new AdminVendorAccountStatusHistoryItemDto
                {
                    Id = h.Id,
                    CreatedAt = h.CreatedAt,
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    AdminName =
                        h.PerformedByAdmin != null
                            ? h.PerformedByAdmin.FullName
                            : h.PerformedByAdminId,
                    Reason = h.Reason,
                    Notes = h.Notes,
                })
                .ToListAsync();

            var address = workshop.WorkshopAddress;
            var addressSummary =
                address == null
                    ? null
                    : string.Join(
                        ", ",
                        new[]
                        {
                            address.City,
                            address.Area,
                            address.Street,
                            address.BuildingNumber,
                        }.Where(x => !string.IsNullOrWhiteSpace(x))
                    );

            return new AdminVendorDetailsDto
            {
                Profile = new AdminVendorProfileDto
                {
                    WorkshopId = workshop.Id,
                    VendorUserId = workshop.UserId,
                    VendorName = workshop.User?.FullName ?? workshop.UserId,
                    Email = workshop.User?.Email,
                    Phone = workshop.User?.PhoneNumber,
                    ProfileImage = workshop.User?.ProfileImage,
                    RegisteredAt = workshop.CreatedAt,
                },
                Business = new AdminVendorBusinessDto
                {
                    WorkshopNameAr = workshop.WorkshopNameAr,
                    WorkshopNameEn = workshop.WorkshopNameEn,
                    DescriptionAr = workshop.DescriptionAr,
                    DescriptionEn = workshop.DescriptionEn,
                    LogoUrl = workshop.LogoUrl,
                    Rating = workshop.Rating,
                    AddressSummary = addressSummary,
                },
                Contact = new AdminVendorContactDto
                {
                    Email = workshop.User?.Email,
                    Phone = workshop.User?.PhoneNumber,
                },
                Verification = new AdminVendorVerificationDto
                {
                    Status = verificationStatus,
                    VerificationDate = workshop.VerificationDate,
                    VerifiedByAdminName =
                        workshop.VerifiedByAdmin != null
                            ? workshop.VerifiedByAdmin.FullName
                            : workshop.VerifiedByAdminId,
                    Notes = workshop.VerificationNotes,
                    RejectionReason = workshop.RejectionReason,
                },
                Account = new AdminVendorAccountDto
                {
                    Status =
                        workshop.User.IsActive == false
                            ? VendorAccountStatus.Pending
                            : VendorAccountStatus.Approved,
                    StatusChangedAt = workshop.AccountStatusChangedAt,
                    StatusChangedByAdminName =
                        workshop.AccountStatusChangedByAdmin != null
                            ? workshop.AccountStatusChangedByAdmin.FullName
                            : workshop.AccountStatusChangedByAdminId,
                },
                OrdersStats = new AdminVendorOrdersStatsDto
                {
                    TotalOrders = totalOrders,
                    DeliveredOrders = deliveredOrders,
                    PendingOrders = pendingOrders,
                    CancelledOrders = CancelledOrders,
                    InProgressOrders = InProgressOrders,
                    ConfirmedOrders = ConfirmedOrders,
                    ReadyforPickup = ReadyforPickup,
                },
                RevenueStats = new AdminVendorRevenueStatsDto
                {
                    TotalRevenue = totalRevenue,
                    DeliveredRevenue = deliveredRevenue,
                },
                VerificationHistory = verificationHistory,
                AccountStatusHistory = accountHistory,
            };
        }

        public async Task<AdminVendorHistoryPageDto> GetHistoryAsync(int page, int pageSize)
        {
            pageSize = pageSize <= 0 ? 20 : pageSize;
            page = page <= 0 ? 1 : page;

            var verificationQuery = _db
                .VendorVerificationHistory.AsNoTracking()
                .Include(h => h.Workshop)
                .Include(h => h.PerformedByAdmin)
                .Select(h => new AdminVendorUnifiedHistoryItemDto
                {
                    CreatedAt = h.CreatedAt,
                    WorkshopId = h.WorkshopId,
                    WorkshopName =
                        h.Workshop != null ? h.Workshop.WorkshopNameEn : $"#{h.WorkshopId}",
                    Action = $"Verification: {h.OldStatus} → {h.NewStatus}",
                    AdminName =
                        h.PerformedByAdmin != null
                            ? h.PerformedByAdmin.FullName
                            : h.PerformedByAdminId,
                    Details = string.IsNullOrWhiteSpace(h.RejectionReason)
                        ? h.Notes
                        : $"Reason: {h.RejectionReason}",
                    BadgeClass =
                        h.NewStatus == VendorVerificationStatus.Active ? "bg-success"
                        : h.NewStatus == VendorVerificationStatus.inActive ? "bg-danger"
                        : "bg-warning",
                });

            var accountQuery = _db
                .VendorAccountStatusHistory.AsNoTracking()
                .Include(h => h.Workshop)
                .Include(h => h.PerformedByAdmin)
                .Select(h => new AdminVendorUnifiedHistoryItemDto
                {
                    CreatedAt = h.CreatedAt,
                    WorkshopId = h.WorkshopId,
                    WorkshopName =
                        h.Workshop != null ? h.Workshop.WorkshopNameEn : $"#{h.WorkshopId}",
                    Action = $"Account: {h.OldStatus} → {h.NewStatus}",
                    AdminName =
                        h.PerformedByAdmin != null
                            ? h.PerformedByAdmin.FullName
                            : h.PerformedByAdminId,
                    Details = string.IsNullOrWhiteSpace(h.Reason) ? h.Notes : $"Reason: {h.Reason}",
                    BadgeClass =
                        h.NewStatus == VendorAccountStatus.Approved ? "bg-success" : "bg-danger",
                });

            var unified = verificationQuery.Concat(accountQuery);

            var totalCount = await unified.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await unified
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new AdminVendorHistoryPageDto
            {
                Page = page,
                PageSize = pageSize,
                Items = items,
                Paging = new Graduation_Application.DTOs.Admin.AdminDashboardDTO.AdminPagingDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                },
            };
        }
    }
}
