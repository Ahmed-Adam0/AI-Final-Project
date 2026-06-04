using System.Security.Claims;
using Graduation_Application.DTOs.Admin.VendorManagementDTO;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Enums;
using Graduation_MVC.Areas.Admin.ViewModels.Vendors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class VendorsController : Controller
    {
        private readonly IAdminVendorsService _adminVendorsService;

        public VendorsController(IAdminVendorsService adminVendorsService)
        {
            _adminVendorsService = adminVendorsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] AdminVendorFilterViewModel filter)
        {
            var dto = await _adminVendorsService.GetVendorsPageAsync(
                new AdminVendorsFilterDto
                {
                    Search = filter.Search,
                    VerificationStatus = filter.VerificationStatus,
                    AccountStatus = filter.AccountStatus,
                    SortBy = filter.SortBy,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                }
            );

            return View(MapPage(dto, filter, pendingOnly: false));
        }

        [HttpGet]
        public async Task<IActionResult> Pending([FromQuery] AdminVendorFilterViewModel filter)
        {
            filter.VerificationStatus ??= VendorVerificationStatus.inActive.ToString();

            var dto = await _adminVendorsService.GetVendorsPageAsync(
                new AdminVendorsFilterDto
                {
                    Search = filter.Search,
                    VerificationStatus = filter.VerificationStatus,
                    AccountStatus = filter.AccountStatus,
                    SortBy = filter.SortBy,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                },
                pendingOnly: true
            );

            return View(MapPage(dto, filter, pendingOnly: true));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _adminVendorsService.GetVendorDetailsAsync(id);
            if (dto == null)
                return NotFound();

            return View(
                new AdminVendorDetailsViewModel
                {
                    Profile = new AdminVendorProfileViewModel
                    {
                        WorkshopId = dto.Profile.WorkshopId,
                        VendorUserId = dto.Profile.VendorUserId,
                        VendorName = dto.Profile.VendorName,
                        Email = dto.Profile.Email,
                        Phone = dto.Profile.Phone,
                        ProfileImage = dto.Profile.ProfileImage,
                        RegisteredAt = dto.Profile.RegisteredAt,
                    },
                    Business = new AdminVendorBusinessViewModel
                    {
                        WorkshopNameAr = dto.Business.WorkshopNameAr,
                        WorkshopNameEn = dto.Business.WorkshopNameEn,
                        DescriptionAr = dto.Business.DescriptionAr,
                        DescriptionEn = dto.Business.DescriptionEn,
                        LogoUrl = dto.Business.LogoUrl,
                        Rating = dto.Business.Rating,
                        AddressSummary = dto.Business.AddressSummary,
                    },
                    Verification = new AdminVendorVerificationViewModel
                    {
                        Status = dto.Verification.Status,
                        VerificationDate = dto.Verification.VerificationDate,
                        VerifiedByAdminName = dto.Verification.VerifiedByAdminName,
                        Notes = dto.Verification.Notes,
                        RejectionReason = dto.Verification.RejectionReason,
                    },
                    Account = new AdminVendorAccountViewModel
                    {
                        Status = dto.Account.Status,
                        StatusChangedAt = dto.Account.StatusChangedAt,
                        StatusChangedByAdminName = dto.Account.StatusChangedByAdminName,
                    },
                    OrdersStats = new AdminVendorOrdersStatsViewModel
                    {
                        TotalOrders = dto.OrdersStats.TotalOrders,
                        DeliveredOrders = dto.OrdersStats.DeliveredOrders,
                        PendingOrders = dto.OrdersStats.PendingOrders,
                        CancelledOrders = dto.OrdersStats.CancelledOrders,
                        InProgressOrders = dto.OrdersStats.InProgressOrders,
                        ConfirmedOrders = dto.OrdersStats.ConfirmedOrders,
                        ReadyforPickupOrders = dto.OrdersStats.ReadyforPickupOrders,
                    },
                    RevenueStats = new AdminVendorRevenueStatsViewModel
                    {
                        TotalRevenue = dto.RevenueStats.TotalRevenue,
                        DeliveredRevenue = dto.RevenueStats.DeliveredRevenue,
                    },
                    VerificationHistory = dto
                        .VerificationHistory.Select(
                            x => new AdminVendorVerificationHistoryItemViewModel
                            {
                                CreatedAt = x.CreatedAt,
                                OldStatus = x.OldStatus,
                                NewStatus = x.NewStatus,
                                AdminName = x.AdminName,
                                Notes = x.Notes,
                                RejectionReason = x.RejectionReason,
                            }
                        )
                        .ToList(),
                    AccountStatusHistory = dto
                        .AccountStatusHistory.Select(
                            x => new AdminVendorAccountStatusHistoryItemViewModel
                            {
                                CreatedAt = x.CreatedAt,
                                OldStatus = x.OldStatus,
                                NewStatus = x.NewStatus,
                                AdminName = x.AdminName,
                                Reason = x.Reason,
                                Notes = x.Notes,
                            }
                        )
                        .ToList(),
                    Actions = new AdminVendorActionsViewModel
                    {
                        CanApprove =
                            dto.Verification.Status == VendorVerificationStatus.inActive
                            || dto.Verification.Status == VendorVerificationStatus.inActive,
                        CanReject =
                            dto.Verification.Status == VendorVerificationStatus.inActive
                            || dto.Verification.Status == VendorVerificationStatus.Active,
                        CanSuspend = dto.Account.Status == VendorAccountStatus.Approved,
                        CanActivate = dto.Account.Status == VendorAccountStatus.Suspended,
                    },
                }
            );
        }

        [HttpGet]
        public async Task<IActionResult> History(int page = 1, int pageSize = 20)
        {
            var dto = await _adminVendorsService.GetHistoryAsync(page, pageSize);
            return View(
                new AdminVendorHistoryViewModel
                {
                    Items = dto
                        .Items.Select(x => new AdminVendorHistoryItemViewModel
                        {
                            CreatedAt = x.CreatedAt,
                            WorkshopId = x.WorkshopId,
                            WorkshopName = x.WorkshopName,
                            Action = x.Action,
                            AdminName = x.AdminName,
                            Details = x.Details,
                            BadgeClass = x.BadgeClass,
                        })
                        .ToList(),
                    Paging = new AdminVendorHistoryPagingViewModel
                    {
                        Page = dto.Paging.Page,
                        PageSize = dto.Paging.PageSize,
                        TotalCount = dto.Paging.TotalCount,
                        TotalPages = dto.Paging.TotalPages,
                    },
                }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int workshopId, string? notes = null)
        {
            //var adminId = GetAdminId();
            //if (string.IsNullOrEmpty(adminId))
            //{
            //    TempData["ErrorMessage"] = "Admin user not authenticated.";
            //    return RedirectToAction(nameof(Details), new { id = workshopId });
            //}

            try
            {
                await _adminVendorsService.ApproveVendorAsync(workshopId, notes);
                TempData["SuccessMessage"] = "Vendor approved successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = workshopId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(AdminVendorRejectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid rejection request.";
                return RedirectToAction(nameof(Details), new { id = model.WorkshopId });
            }

            var adminId = GetAdminId();
            if (string.IsNullOrEmpty(adminId))
            {
                TempData["ErrorMessage"] = "Admin user not authenticated.";
                return RedirectToAction(nameof(Details), new { id = model.WorkshopId });
            }

            try
            {
                await _adminVendorsService.RejectVendorAsync(
                    model.WorkshopId,
                    adminId,
                    model.RejectionReason,
                    model.Notes
                );
                TempData["SuccessMessage"] = "Vendor rejected successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = model.WorkshopId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(AdminVendorSuspendViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid suspension request.";
                return RedirectToAction(nameof(Details), new { id = model.WorkshopId });
            }

            var adminId = GetAdminId();
            if (string.IsNullOrEmpty(adminId))
            {
                TempData["ErrorMessage"] = "Admin user not authenticated.";
                return RedirectToAction(nameof(Details), new { id = model.WorkshopId });
            }

            try
            {
                await _adminVendorsService.SuspendVendorAsync(
                    model.WorkshopId,
                    adminId,
                    model.Reason,
                    model.Notes
                );
                TempData["SuccessMessage"] = "Vendor suspended successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = model.WorkshopId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int workshopId, string? notes = null)
        {
            //var adminId = GetAdminId();
            //if (string.IsNullOrEmpty(adminId))
            //{
            //    TempData["ErrorMessage"] = "Admin user not authenticated.";
            //    return RedirectToAction(nameof(Details), new { id = workshopId });
            //}

            try
            {
                //await _adminVendorsService.ActivateVendorAsync(workshopId, adminId, notes);
                await _adminVendorsService.ActivateVendorAsync(workshopId, notes);
                TempData["SuccessMessage"] = "Vendor activated successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = workshopId });
        }

        private string GetAdminId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        private static AdminVendorsPageViewModel MapPage(
            AdminVendorsPageDto dto,
            AdminVendorFilterViewModel filter,
            bool pendingOnly
        )
        {
            return new AdminVendorsPageViewModel
            {
                PendingOnly = pendingOnly,
                Filter = filter,
                Vendors = dto
                    .Vendors.Select(x => new AdminVendorListItemViewModel
                    {
                        WorkshopId = x.WorkshopId,
                        WorkshopName = x.WorkshopName,
                        VendorName = x.VendorName,
                        Email = x.Email,
                        Phone = x.Phone,
                        RegisteredAt = x.RegisteredAt,
                        VerificationStatus = x.VerificationStatus,
                        AccountStatus = x.AccountStatus,
                    })
                    .ToList(),
                Paging = new AdminVendorsPagingViewModel
                {
                    Page = dto.Paging.Page,
                    PageSize = dto.Paging.PageSize,
                    TotalCount = dto.Paging.TotalCount,
                    TotalPages = dto.Paging.TotalPages,
                },
                VerificationStatusOptions = dto
                    .VerificationStatusOptions.Select(x => new AdminStatusOptionViewModel
                    {
                        Value = x.Value,
                        Label = x.Label,
                    })
                    .ToList(),
                AccountStatusOptions = dto
                    .AccountStatusOptions.Select(x => new AdminStatusOptionViewModel
                    {
                        Value = x.Value,
                        Label = x.Label,
                    })
                    .ToList(),
                Statistics = new AdminVendorStatisticsViewModel
                {
                    TotalVendors = dto.Statistics.TotalVendors,
                    PendingVendors = dto.Statistics.PendingVendors,
                    ApprovedVendors = dto.Statistics.ApprovedVendors,
                    RejectedVendors = dto.Statistics.RejectedVendors,
                    ActiveVendors = dto.Statistics.ActiveVendors,
                    SuspendedVendors = dto.Statistics.SuspendedVendors,
                },
            };
        }
    }
}
