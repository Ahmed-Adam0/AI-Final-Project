using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.UsersDTO;
using Graduation_Application.IServices.Admin;
using Graduation_Application.DTOs.Common;
using Graduation_MVC.Areas.Admin.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    [Route("Admin/Users/[action]")]
    public class UsersController : Controller
    {
        private readonly IAdminUsersService _adminUsersService;

        public UsersController(IAdminUsersService adminUsersService)
        {
            _adminUsersService = adminUsersService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AdminUsersFilterViewModel filter)
        {
            var dtoFilter = new AdminUsersFilterDto
            {
                Search = filter.Search,
                Status = filter.Status,
                EmailConfirmed = filter.EmailConfirmed,
                Page = filter.Page <= 0 ? 1 : filter.Page,
                PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize
            };

            var result = await _adminUsersService.GetUsersAsync(dtoFilter);

            var vm = new AdminUsersPageViewModel
            {
                Filter = filter,
                Users = result.Items.Select(u => new AdminUserListItemViewModel {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneNumber = u.PhoneNumber,
                }).ToList(),
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                Page = result.PageNumber,
                PageSize = result.PageSize
            };

            return View(vm);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var dto = await _adminUsersService.GetUserDetailsAsync(id);
            var vm = new AdminUserDetailsViewModel
            {
                Id = dto.Id,
                FullName = dto.FullName,
                ProfileImage = dto.ProfileImage,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = dto.IsActive,
                EmailConfirmed = dto.EmailConfirmed,
                CreatedAt = dto.CreatedAt,
                TotalOrders = dto.TotalOrders,
                TotalSpent = dto.TotalSpent
            };

            return View(vm);
        }

        [HttpPost("Activate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            await _adminUsersService.ActivateUserAsync(id);
            TempData["SuccessMessage"] = "User activated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Suspend/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(string id)
        {
            await _adminUsersService.SuspendUserAsync(id);
            TempData["SuccessMessage"] = "User suspended successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _adminUsersService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
