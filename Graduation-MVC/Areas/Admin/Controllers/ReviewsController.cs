using System.Security.Claims;
using Graduation_Application.DTOs.Admin.Reviews;
using Graduation_Application.IServices.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
  //  [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly IAdminReviewService _adminReviewService;

        public ReviewsController(IAdminReviewService adminReviewService)
        {
            _adminReviewService = adminReviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] AdminReviewFilterDto filter)
        {
            var result = await _adminReviewService.GetReviewsAsync(filter);

            ViewData["Filter"] = filter;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var review = await _adminReviewService.GetReviewDetailsAsync(id);
            if (review == null) return NotFound();

            return View(review);
        }

        [HttpGet]
        public async Task<IActionResult> Reported()
        {
            var items = await _adminReviewService.GetReportedReviewsAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(int id)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _adminReviewService.ResolveReportAsync(id, adminUserId);
            TempData["SuccessMessage"] = "Report resolved.";
            return RedirectToAction(nameof(Reported));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IgnoreReport(int id)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _adminReviewService.IgnoreReportAsync(id, adminUserId);
            TempData["SuccessMessage"] = "Report ignored.";
            return RedirectToAction(nameof(Reported));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _adminReviewService.DeleteReviewAsync(id, adminUserId);
            TempData["SuccessMessage"] = "Review deleted (deactivated).";

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}

