using System;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.FaqDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.FAQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class FaqController : Controller
    {
        private readonly IFaqService _faqService;
        private readonly ILocalizationService _localizationService;

        public FaqController(IFaqService faqService, ILocalizationService localizationService)
        {
            _faqService = faqService;
            _localizationService = localizationService;
        }

        // GET: /Admin/Faq
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? search)
        {
            var faqs = await _faqService.GetAllFaqsAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                faqs = faqs.Where(f =>
                        f.QuestionEn.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || f.QuestionAr.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || f.AnswerEn.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || f.AnswerAr.Contains(search, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            var viewModel = new AdminFaqPageViewModel { Faqs = faqs, Search = search };

            return View(viewModel);
        }

        // GET: /Admin/Faq/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AdminFaqFormViewModel();
            return View(model);
        }

        // POST: /Admin/Faq/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminFaqFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dto = new CreateFaqDto
                    {
                        QuestionAr = model.QuestionAr,
                        QuestionEn = model.QuestionEn,
                        AnswerAr = model.AnswerAr,
                        AnswerEn = model.AnswerEn,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActive,
                    };

                    await _faqService.CreateFaqAsync(dto);

                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.faq.create.success"
                    );
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        _localizationService.Get("admin.faq.create.error") + ": " + ex.Message
                    );
                }
            }

            return View(model);
        }

        // GET: /Admin/Faq/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _faqService.GetFaqByIdAsync(id);
            if (faq == null)
            {
                TempData["ErrorMessage"] = _localizationService.Get("admin.faq.notfound");
                return RedirectToAction(nameof(Index));
            }

            var model = new AdminFaqFormViewModel
            {
                Id = faq.Id,
                QuestionAr = faq.QuestionAr,
                QuestionEn = faq.QuestionEn,
                AnswerAr = faq.AnswerAr,
                AnswerEn = faq.AnswerEn,
                DisplayOrder = faq.DisplayOrder,
                IsActive = faq.IsActive,
            };

            return View(model);
        }

        // POST: /Admin/Faq/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminFaqFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dto = new UpdateFaqDto
                    {
                        QuestionAr = model.QuestionAr,
                        QuestionEn = model.QuestionEn,
                        AnswerAr = model.AnswerAr,
                        AnswerEn = model.AnswerEn,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActive,
                    };

                    await _faqService.UpdateFaqAsync(id, dto);

                    TempData["SuccessMessage"] = _localizationService.Get("admin.faq.edit.success");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        _localizationService.Get("admin.faq.edit.error") + ": " + ex.Message
                    );
                }
            }

            return View(model);
        }

        // POST: /Admin/Faq/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _faqService.DeleteFaqAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.faq.delete.success"
                    );
                }
                else
                {
                    TempData["ErrorMessage"] = _localizationService.Get("admin.faq.delete.error");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    _localizationService.Get("admin.faq.delete.errorException") + ": " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Faq/UpdateStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            try
            {
                var success = await _faqService.UpdateFaqStatusAsync(id, isActive);
                if (success)
                {
                    var statusKey = isActive
                        ? "admin.faq.status.active"
                        : "admin.faq.status.inactive";
                    TempData["SuccessMessage"] = string.Format(
                        _localizationService.Get("admin.faq.status.updated"),
                        _localizationService.Get(statusKey)
                    );
                }
                else
                {
                    TempData["ErrorMessage"] = _localizationService.Get("admin.faq.notfound");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    _localizationService.Get("admin.faq.status.errorException") + ": " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
