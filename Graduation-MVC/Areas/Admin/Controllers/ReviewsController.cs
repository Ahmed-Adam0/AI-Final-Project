using System.Security.Claims;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.Admin.Reviews;
using Graduation_Application.IServices.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class ReviewsController : Controller
    {
        private readonly IAdminReviewService _adminReviewService;
        private readonly ILocalizationService _localizationService;

        public ReviewsController(
            IAdminReviewService adminReviewService,
            ILocalizationService localizationService
        )
        {
            _adminReviewService = adminReviewService;
            _localizationService = localizationService;
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
            if (review == null)
                return NotFound();

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
            TempData["SuccessMessage"] = _localizationService.Get("admin.reviews.msg.resolve");
            return RedirectToAction(nameof(Reported));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IgnoreReport(int id)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _adminReviewService.IgnoreReportAsync(id, adminUserId);
            TempData["SuccessMessage"] = _localizationService.Get("admin.reviews.msg.ignore");
            return RedirectToAction(nameof(Reported));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _adminReviewService.DeleteReviewAsync(id, adminUserId);
            TempData["SuccessMessage"] = _localizationService.Get("admin.reviews.msg.delete");

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
