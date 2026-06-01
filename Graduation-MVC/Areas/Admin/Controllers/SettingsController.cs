using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin", Policy = "AdminOnly")]
    public class SettingsController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public SettingsController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dto = await _adminDashboardService.GetSettingsAsync();
            return View(
                new AdminSettingsViewModel
                {
                    Platform = new PlatformSettingsViewModel
                    {
                        PlatformName = dto.Platform.PlatformName,
                        LogoUrl = dto.Platform.LogoUrl,
                        ContactInformation = dto.Platform.ContactInformation,
                        MaintenanceMode = dto.Platform.MaintenanceMode,
                    },
                    Commission = new CommissionSettingsViewModel
                    {
                        CommissionPercentage = dto.Commission.CommissionPercentage,
                        VendorFees = dto.Commission.VendorFees,
                        TaxPercentage = dto.Commission.TaxPercentage,
                    },
                    Support = new SupportSettingsViewModel
                    {
                        Email = dto.Support.Email,
                        Phone = dto.Support.Phone,
                        WhatsApp = dto.Support.WhatsApp,
                        SupportHours = dto.Support.SupportHours,
                    },
                    AuditLogs = dto
                        .AuditLogs.Select(x => new AdminAuditLogItemViewModel
                        {
                            CreatedAt = x.CreatedAt,
                            Action = x.Action,
                            PerformedBy = x.PerformedBy,
                            Details = x.Details,
                        })
                        .ToList(),
                }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(AdminSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "Settings saved successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
