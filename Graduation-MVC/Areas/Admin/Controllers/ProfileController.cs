using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.Admin.AdminProfileDTO;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    [Route("Admin/Profile/[action]")]
    public class ProfileController : Controller
    {
        private readonly IAdminProfileService _profileService;
        private readonly ILocalizationService _localizationService;

        public ProfileController(
            IAdminProfileService profileService,
            ILocalizationService localizationService
        )
        {
            _profileService = profileService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized();
                }

                var profile = await _profileService.GetProfileAsync(adminId);

                var viewModel = new AdminProfileViewModel
                {
                    FullName = profile.FullName,
                    Email = profile.Email,
                    PhoneNumber = profile.PhoneNumber,
                    ProfileImage = profile.ProfileImage,
                    PreferredLanguage = profile.PreferredLanguage,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized();
                }

                var profile = await _profileService.GetProfileAsync(adminId);

                var viewModel = new AdminEditProfileViewModel
                {
                    FullName = profile.FullName,
                    PhoneNumber = profile.PhoneNumber,
                    PreferredLanguage = profile.PreferredLanguage,
                    CurrentProfileImage = profile.ProfileImage,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEditProfileViewModel vm)
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized();
                }

                ModelState.Remove("ProfileImage");
                ModelState.Remove("CurrentProfileImage");
                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                var dto = new AdminUpdateProfileDto
                {
                    FullName = vm.FullName,
                    PhoneNumber = vm.PhoneNumber,
                    PreferredLanguage = vm.PreferredLanguage,
                    ProfileImage = vm.ProfileImage,
                };

                await _profileService.UpdateProfileAsync(adminId, dto);

                // Sync the language cookie with the newly updated profile language preference
                await _localizationService.SetCultureAsync(vm.PreferredLanguage);

                // Use the new language explicitly — the new cookie is not yet in the request at this point
                TempData["SuccessMessage"] = _localizationService.Get(
                    "auth.profile.successUpdate",
                    vm.PreferredLanguage
                );
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new AdminChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel vm)
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                // Validate password confirmation
                if (vm.NewPassword != vm.ConfirmPassword)
                {
                    ModelState.AddModelError(
                        "ConfirmPassword",
                        _localizationService.Get("auth.profile.passwordsDoNotMatch")
                    );
                    return View(vm);
                }

                var dto = new AdminChangePasswordDto
                {
                    CurrentPassword = vm.CurrentPassword,
                    NewPassword = vm.NewPassword,
                };

                await _profileService.ChangePasswordAsync(adminId, dto);

                TempData["SuccessMessage"] = _localizationService.Get(
                    "auth.profile.successPasswordChange"
                );
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(vm);
            }
        }
    }
}
