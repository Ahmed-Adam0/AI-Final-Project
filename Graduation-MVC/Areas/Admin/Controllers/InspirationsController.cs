using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.Admin.Inspirations;
using Graduation_Application.IServices.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class InspirationsController : Controller
    {
        private readonly IAdminInspirationService _adminInspirationService;

        public InspirationsController(IAdminInspirationService adminInspirationService)
        {
            _adminInspirationService = adminInspirationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] AdminInspirationFilterDto filter)
        {
            var result = await _adminInspirationService.GetInspirationsAsync(filter);
            ViewData["Filter"] = filter;
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? returnUrl = null)
        {
            var success = await _adminInspirationService.ApproveInspirationAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Inspiration submission approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Submission could not be found or approved.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
