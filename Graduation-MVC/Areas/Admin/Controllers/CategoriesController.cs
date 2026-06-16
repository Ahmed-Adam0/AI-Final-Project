using System;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.CategoryDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ISubCategoryService _subCategoryService;
        private readonly IProductTypeService _productTypeService;
        private readonly IFileService _fileService;
        private readonly ILocalizationService _localizationService;

        public CategoriesController(
            ICategoryService categoryService,
            ISubCategoryService subCategoryService,
            IProductTypeService productTypeService,
            IFileService fileService,
            ILocalizationService localizationService
        )
        {
            _categoryService = categoryService;
            _subCategoryService = subCategoryService;
            _productTypeService = productTypeService;
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
                        _localizationService.Get("admin.categories.create.error")
                            + ": "
                            + ex.Message
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
                        _localizationService.Get("admin.categories.edit.error") + ": " + ex.Message
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
                    _localizationService.Get("admin.categories.delete.errorException")
                    + ": "
                    + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // SUB-CATEGORIES CRUD
        // ==========================================

        // GET: /Admin/Categories/{categoryId}/SubCategories
        [HttpGet]
        public async Task<IActionResult> SubCategories(int categoryId)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            var subCategories = await _subCategoryService.GetSubCategoriesByCategoryIdAsync(
                categoryId
            );

            ViewBag.Category = category;
            return View(subCategories);
        }

        // GET: /Admin/Categories/{categoryId}/SubCategories/Create
        [HttpGet]
        public async Task<IActionResult> CreateSubCategory(int categoryId)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AdminSubCategoryFormViewModel
            {
                CategoryId = categoryId,
                CategoryNameEn = category.NameEn,
            };

            return View(model);
        }

        // POST: /Admin/Categories/{categoryId}/SubCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubCategory(AdminSubCategoryFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dto = new CreateSubCategoryDto
                    {
                        NameAr = model.NameAr,
                        NameEn = model.NameEn,
                        CategoryId = model.CategoryId,
                    };

                    await _subCategoryService.CreateSubCategoryAsync(dto);
                    TempData["SuccessMessage"] = "Subcategory created successfully.";
                    return RedirectToAction(
                        nameof(SubCategories),
                        new { categoryId = model.CategoryId }
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating subcategory: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Admin/Categories/SubCategories/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> EditSubCategory(int id)
        {
            var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
            {
                TempData["ErrorMessage"] = "Subcategory not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AdminSubCategoryFormViewModel
            {
                Id = subCategory.Id,
                NameAr = subCategory.NameAr,
                NameEn = subCategory.NameEn,
                CategoryId = subCategory.CategoryId,
                CategoryNameEn = subCategory.CategoryNameEn,
            };

            return View(model);
        }

        // POST: /Admin/Categories/SubCategories/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubCategory(
            int id,
            AdminSubCategoryFormViewModel model
        )
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var dto = new UpdateSubCategoryDto
                    {
                        NameAr = model.NameAr,
                        NameEn = model.NameEn,
                        CategoryId = model.CategoryId,
                    };

                    await _subCategoryService.UpdateSubCategoryAsync(id, dto);
                    TempData["SuccessMessage"] = "Subcategory updated successfully.";
                    return RedirectToAction(
                        nameof(SubCategories),
                        new { categoryId = model.CategoryId }
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating subcategory: " + ex.Message);
                }
            }

            return View(model);
        }

        // POST: /Admin/Categories/SubCategories/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubCategory(int id, int categoryId)
        {
            try
            {
                await _subCategoryService.DeleteSubCategoryAsync(id);
                TempData["SuccessMessage"] = "Subcategory deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting subcategory: " + ex.Message;
            }

            return RedirectToAction(nameof(SubCategories), new { categoryId = categoryId });
        }

        // ==========================================
        // PRODUCT TYPES CRUD
        // ==========================================

        // GET: /Admin/Categories/SubCategories/{subCategoryId}/ProductTypes
        [HttpGet]
        public async Task<IActionResult> ProductTypes(int subCategoryId)
        {
            var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(subCategoryId);
            if (subCategory == null)
            {
                TempData["ErrorMessage"] = "Subcategory not found.";
                return RedirectToAction(nameof(Index));
            }

            var productTypes = await _productTypeService.GetProductTypesBySubCategoryIdAsync(
                subCategoryId
            );

            ViewBag.SubCategory = subCategory;
            return View(productTypes);
        }

        // GET: /Admin/Categories/SubCategories/{subCategoryId}/ProductTypes/Create
        [HttpGet]
        public async Task<IActionResult> CreateProductType(int subCategoryId)
        {
            var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(subCategoryId);
            if (subCategory == null)
            {
                TempData["ErrorMessage"] = "Subcategory not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AdminProductTypeFormViewModel
            {
                SubCategoryId = subCategoryId,
                SubCategoryNameEn = subCategory.NameEn,
            };

            return View(model);
        }

        // POST: /Admin/Categories/SubCategories/{subCategoryId}/ProductTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProductType(AdminProductTypeFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dto = new CreateProductTypeDto
                    {
                        NameAr = model.NameAr,
                        NameEn = model.NameEn,
                        SubCategoryId = model.SubCategoryId,
                    };

                    await _productTypeService.CreateProductTypeAsync(dto);
                    TempData["SuccessMessage"] = "Product Type created successfully.";
                    return RedirectToAction(
                        nameof(ProductTypes),
                        new { subCategoryId = model.SubCategoryId }
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating product type: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Admin/Categories/ProductTypes/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> EditProductType(int id)
        {
            var productType = await _productTypeService.GetProductTypeByIdAsync(id);
            if (productType == null)
            {
                TempData["ErrorMessage"] = "Product Type not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AdminProductTypeFormViewModel
            {
                Id = productType.Id,
                NameAr = productType.NameAr,
                NameEn = productType.NameEn,
                SubCategoryId = productType.SubCategoryId,
                SubCategoryNameEn = productType.SubCategoryNameEn,
            };

            return View(model);
        }

        // POST: /Admin/Categories/ProductTypes/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductType(
            int id,
            AdminProductTypeFormViewModel model
        )
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var dto = new UpdateProductTypeDto
                    {
                        NameAr = model.NameAr,
                        NameEn = model.NameEn,
                        SubCategoryId = model.SubCategoryId,
                    };

                    await _productTypeService.UpdateProductTypeAsync(id, dto);
                    TempData["SuccessMessage"] = "Product Type updated successfully.";
                    return RedirectToAction(
                        nameof(ProductTypes),
                        new { subCategoryId = model.SubCategoryId }
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating product type: " + ex.Message);
                }
            }

            return View(model);
        }

        // POST: /Admin/Categories/ProductTypes/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductType(int id, int subCategoryId)
        {
            try
            {
                await _productTypeService.DeleteProductTypeAsync(id);
                TempData["SuccessMessage"] = "Product Type deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting product type: " + ex.Message;
            }

            return RedirectToAction(nameof(ProductTypes), new { subCategoryId = subCategoryId });
        }
    }
}
