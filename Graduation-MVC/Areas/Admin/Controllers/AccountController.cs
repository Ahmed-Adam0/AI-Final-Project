using System.Security.Claims;
using Graduation_Application.DTOs.Admin.AuthAdmin;
using Graduation_Application.IServices.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Graduation_MVC.Areas.Admin.ViewModels.Auth;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Account/[action]")]
    public class AccountController : Controller
    {
        private readonly IAdminAuthService _adminAuthService;
        private readonly UserManager<Graduation_domain.Entities.ApplicationUser> _userManager;
        private readonly SignInManager<Graduation_domain.Entities.ApplicationUser> _signInManager;

        public AccountController(
            IAdminAuthService adminAuthService,
            UserManager<Graduation_domain.Entities.ApplicationUser> userManager,
            SignInManager<Graduation_domain.Entities.ApplicationUser> signInManager)
        {
            _adminAuthService = adminAuthService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid) return View(vm);

                var result = await _adminAuthService.LoginAsync(new AdminLoginDto { Email = vm.Email, Password = vm.Password });

                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, result.Id),
                                    new Claim(ClaimTypes.Name, result.FullName ?? string.Empty),
                                    new Claim(ClaimTypes.Email, result.Email ?? string.Empty),
                                    new Claim(ClaimTypes.Role, result.Role ?? "SuperAdmin"),
                                    new Claim("lang", result.PreferredLanguage ?? "ar")
                                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    principal,
    //new AuthenticationProperties
    //{
    //    IsPersistent = vm.RememberMe,  // ← ده Remember Me
    //    ExpiresUtc = vm.RememberMe 
    //        ? DateTimeOffset.UtcNow.AddDays(1)  // ← 30 يوم
    //        : DateTimeOffset.UtcNow.AddHours(1)  // ← 8 ساعات
    //}
    new AuthenticationProperties
    {
        IsPersistent = false  // ← مش هيتحفظ لما يقفل المتصفح
    }

);

                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid) return View(vm);

                await _adminAuthService.ForgotPasswordAsync(new AdminForgotPasswordDto { Email = vm.Email });
                TempData["SuccessMessage"] = "OTP sent to your email";
                TempData["OtpEmail"] = vm.Email;
                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var vm = new VerifyOtpViewModel();
            if (TempData.ContainsKey("OtpEmail")) vm.Email = TempData["OtpEmail"].ToString();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid) return View(vm);

                var valid = await _adminAuthService.VerifyOtpAsync(new AdminVerifyOtpDto { Email = vm.Email, OtpCode = vm.OtpCode });
                if (!valid)
                {
                    TempData["ErrorMessage"] = "Invalid OTP code";
                    return View(vm);
                }

                TempData["OtpEmail"] = vm.Email;
                TempData["OtpCode"] = vm.OtpCode;
                return RedirectToAction("ResetPassword");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(vm);
            }
        }

        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword()
        {
            var vm = new ResetPasswordViewModel();
            if (TempData.ContainsKey("OtpEmail")) vm.Email = TempData["OtpEmail"].ToString();
            if (TempData.ContainsKey("OtpCode")) vm.OtpCode = TempData["OtpCode"].ToString();
            return View(vm);
        }

        [HttpPost("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid) return View(vm);

                await _adminAuthService.ResetPasswordAsync(new AdminResetPasswordDto { Email = vm.Email, OtpCode = vm.OtpCode, NewPassword = vm.NewPassword });
                TempData["SuccessMessage"] = "Password reset successfully";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(vm);
            }
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
