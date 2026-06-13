using System;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.CategoryDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IFileService _fileService;
        private readonly ILocalizationService _localizationService;

        public CategoriesController(
            ICategoryService categoryService,
            IFileService fileService,
            ILocalizationService localizationService
        )
        {
            _categoryService = categoryService;
            _fileService = fileService;
            _localizationService = localizationService;
        }

        // GET: /Admin/Categories
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? search)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                categories = categories
                    .Where(c =>
                        c.NameEn.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || c.NameAr.Contains(search, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            var viewModel = new AdminCategoriesPageViewModel
            {
                Categories = categories,
                Search = search,
            };

            return View(viewModel);
        }

        // GET: /Admin/Categories/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AdminCategoryFormViewModel();
            return View(model);
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCategoryFormViewModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Category image is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Save image to Cloudinary in a folder named "categories"
                    string imageUrl = await _fileService.SaveImageAsync(
                        model.ImageFile!,
                        "categories"
                    );

                    var dto = new CreateCategoryDto
                    {
                        NameAr = model.NameAr,
                        NameEn = model.NameEn,
                        ImageUrl = imageUrl,
                    };

                    await _categoryService.CreateCategoryAsync(dto);

                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.categories.create.success"
                    );
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        $"An error occurred while creating the category: {ex.Message}"
                    );
                }
            }

            return View(model);
        }

        // GET: /Admin/Categories/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = _localizationService.Get("admin.categories.notfound");
                return RedirectToAction(nameof(Index));
            }

            var model = new AdminCategoryFormViewModel
            {
                Id = category.Id,
                NameAr = category.NameAr,
                NameEn = category.NameEn,
                ImageUrl = category.ImageUrl,
            };

            return View(model);
        }

        // POST: /Admin/Categories/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminCategoryFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string imageUrl = model.ImageUrl ?? "";

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Save the new image and pass the old image URL for potential deletion/overwrite
                        imageUrl = await _fileService.SaveImageAsync(
                            model.ImageFile,
                            "categories",
                            model.ImageUrl
                        );
                    }

                    var dto = new UpdateCategoryDto
                    {
                        NameAr = model.NameAr,
                        NameEn = model.NameEn,
                        ImageUrl = imageUrl,
                    };

                    await _categoryService.UpdateCategoryAsync(id, dto);

                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.categories.edit.success"
                    );
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        $"An error occurred while updating the category: {ex.Message}"
                    );
                }
            }

            return View(model);
        }

        // POST: /Admin/Categories/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.categories.delete.success"
                    );
                }
                else
                {
                    TempData["ErrorMessage"] = _localizationService.Get(
                        "admin.categories.delete.error"
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"An error occurred while deleting the category: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
