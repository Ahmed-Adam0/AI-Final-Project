using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ReportsController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public ReportsController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] AdminReportsFilterViewModel filter)
        {
            var dto = await _adminDashboardService.GetReportsAsync(
                new AdminReportsFilterDto
                {
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    VendorId = filter.VendorId,
                    CategoryId = filter.CategoryId,
                    ReportType = filter.ReportType,
                }
            );

            return View(
                new AdminReportsPageViewModel
                {
                    Filter = filter,
                    Reports = dto
                        .Reports.Select(x => new AdminReportListItemViewModel
                        {
                            Id = x.Id,
                            Title = x.Title,
                            Type = x.Type,
                            VendorName = x.VendorName,
                            CreatedAt = x.CreatedAt,
                            DownloadUrl = x.DownloadUrl,
                            ExportPdfUrl = $"/Admin/Reports/ExportPdf?reportId={x.Id}",
                            ExportExcelUrl = $"/Admin/Reports/ExportExcel?reportId={x.Id}",
                        })
                        .ToList(),
                }
            );
        }

        [HttpGet]
        public IActionResult Daily() =>
            RedirectToAction(nameof(Index), new { reportType = "Daily" });

        [HttpGet]
        public IActionResult Weekly() =>
            RedirectToAction(nameof(Index), new { reportType = "Weekly" });

        [HttpGet]
        public IActionResult Monthly() =>
            RedirectToAction(nameof(Index), new { reportType = "Monthly" });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Generate(string reportType)
        {
            TempData["SuccessMessage"] = $"{reportType} report generation started.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Download(int reportId) => RedirectToAction(nameof(Index));

        [HttpGet]
        public IActionResult ExportPdf(int reportId) => RedirectToAction(nameof(Index));

        [HttpGet]
        public IActionResult ExportExcel(int reportId) => RedirectToAction(nameof(Index));
    }
}
