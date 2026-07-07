using System;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.IServices.Admin;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class WithdrawalsController : Controller
    {
        private readonly IAdminWithdrawalsService _withdrawalsService;
        private readonly ILocalizationService _localizationService;

        public WithdrawalsController(
            IAdminWithdrawalsService withdrawalsService,
            ILocalizationService localizationService)
        {
            _withdrawalsService  = withdrawalsService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var withdrawals = await _withdrawalsService.GetAllWithdrawalsAsync();
            return View(withdrawals);
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var success = await _withdrawalsService.CompleteWithdrawalAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = _localizationService.Get("auth.payout.completeSuccess");
            }
            else
            {
                TempData["ErrorMessage"] = _localizationService.Get("auth.payout.completeFail");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var success = await _withdrawalsService.RejectWithdrawalAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = _localizationService.Get("auth.payout.rejectSuccess");
            }
            else
            {
                TempData["ErrorMessage"] = _localizationService.Get("auth.payout.rejectFail");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
