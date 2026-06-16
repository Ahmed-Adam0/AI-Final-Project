using System;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.BannerDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Banners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class BannersController : Controller
    {
        private readonly IBannerService _bannerService;
        private readonly IFileService _fileService;
        private readonly ILocalizationService _localizationService;

        public BannersController(
            IBannerService bannerService,
            IFileService fileService,
            ILocalizationService localizationService
        )
        {
            _bannerService = bannerService;
            _fileService = fileService;
            _localizationService = localizationService;
        }

        // GET: /Admin/Banners
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? search)
        {
            var banners = await _bannerService.GetAllBannersAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                banners = banners
                    .Where(b =>
                        b.TitleEn.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || b.TitleAr.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || (
                            b.DescriptionEn != null
                            && b.DescriptionEn.Contains(search, StringComparison.OrdinalIgnoreCase)
                        )
                        || (
                            b.DescriptionAr != null
                            && b.DescriptionAr.Contains(search, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .ToList();
            }

            var viewModel = new AdminBannersPageViewModel { Banners = banners, Search = search };

            return View(viewModel);
        }

        // GET: /Admin/Banners/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AdminBannerFormViewModel();
            return View(model);
        }

        // POST: /Admin/Banners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminBannerFormViewModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    _localizationService.Get("admin.banners.create.imageRequired")
                );
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string imageUrl = await _fileService.SaveImageAsync(
                        model.ImageFile!,
                        "banners"
                    );

                    var dto = new CreateBannerDto
                    {
                        TitleAr = model.TitleAr,
                        TitleEn = model.TitleEn,
                        DescriptionAr = model.DescriptionAr,
                        DescriptionEn = model.DescriptionEn,
                        ImageUrl = imageUrl,
                        RedirectUrl = model.RedirectUrl,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActive,
                    };

                    await _bannerService.CreateBannerAsync(dto);

                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.banners.create.success"
                    );
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        $"An error occurred while creating the banner: {ex.Message}"
                    );
                }
            }

            return View(model);
        }

        // GET: /Admin/Banners/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null)
            {
                TempData["ErrorMessage"] = _localizationService.Get("admin.banners.notfound");
                return RedirectToAction(nameof(Index));
            }

            var model = new AdminBannerFormViewModel
            {
                Id = banner.Id,
                TitleAr = banner.TitleAr,
                TitleEn = banner.TitleEn,
                DescriptionAr = banner.DescriptionAr,
                DescriptionEn = banner.DescriptionEn,
                ImageUrl = banner.ImageUrl,
                RedirectUrl = banner.RedirectUrl,
                DisplayOrder = banner.DisplayOrder,
                IsActive = banner.IsActive,
            };

            return View(model);
        }

        // POST: /Admin/Banners/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminBannerFormViewModel model)
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
                        imageUrl = await _fileService.SaveImageAsync(
                            model.ImageFile,
                            "banners",
                            model.ImageUrl
                        );
                    }

                    var dto = new UpdateBannerDto
                    {
                        TitleAr = model.TitleAr,
                        TitleEn = model.TitleEn,
                        DescriptionAr = model.DescriptionAr,
                        DescriptionEn = model.DescriptionEn,
                        ImageUrl = imageUrl,
                        RedirectUrl = model.RedirectUrl,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActive,
                    };

                    await _bannerService.UpdateBannerAsync(id, dto);

                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.banners.edit.success"
                    );
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        $"An error occurred while updating the banner: {ex.Message}"
                    );
                }
            }

            return View(model);
        }

        // POST: /Admin/Banners/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _bannerService.DeleteBannerAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = _localizationService.Get(
                        "admin.banners.delete.success"
                    );
                }
                else
                {
                    TempData["ErrorMessage"] = _localizationService.Get(
                        "admin.banners.delete.error"
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"An error occurred while deleting the banner: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Banners/UpdateStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            try
            {
                var success = await _bannerService.UpdateBannerStatusAsync(id, isActive);
                if (success)
                {
                    var statusKey = isActive
                        ? "admin.banners.status.active"
                        : "admin.banners.status.inactive";
                    TempData["SuccessMessage"] = string.Format(
                        _localizationService.Get("admin.banners.status.updated"),
                        _localizationService.Get(statusKey)
                    );
                }
                else
                {
                    TempData["ErrorMessage"] = _localizationService.Get("admin.banners.notfound");
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
