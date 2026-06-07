using System;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.FaqDTO;
using Graduation_Application.IServices;
using Graduation_MVC.Areas.Admin.ViewModels.FAQ;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FaqController : Controller
    {
        private readonly IFaqService _faqService;

        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        // GET: /Admin/Faq
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? search)
        {
            var faqs = await _faqService.GetAllFaqsAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                faqs = faqs
                    .Where(f => f.QuestionEn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                f.QuestionAr.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                f.AnswerEn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                f.AnswerAr.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var viewModel = new AdminFaqPageViewModel
            {
                Faqs = faqs,
                Search = search
            };

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
                        IsActive = model.IsActive
                    };

                    await _faqService.CreateFaqAsync(dto);

                    TempData["SuccessMessage"] = "FAQ created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while creating the FAQ: {ex.Message}");
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
                TempData["ErrorMessage"] = "FAQ not found.";
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
                IsActive = faq.IsActive
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
                        IsActive = model.IsActive
                    };

                    await _faqService.UpdateFaqAsync(id, dto);

                    TempData["SuccessMessage"] = "FAQ updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while updating the FAQ: {ex.Message}");
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
                    TempData["SuccessMessage"] = "FAQ deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "FAQ not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the FAQ: {ex.Message}";
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
                    TempData["SuccessMessage"] = $"FAQ status updated to {(isActive ? "Active" : "Inactive")}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "FAQ not found or could not be updated.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
