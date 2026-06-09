using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.Admin.UsersDTO;
using Graduation_Application.DTOs.Common;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Graduation_Application.Services.Admin
{
    public class AdminUsersService : IAdminUsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenaricRepositories<Order> _orderRepository;

        public AdminUsersService(UserManager<ApplicationUser> userManager, IGenaricRepositories<Order> orderRepository)
        {
            _userManager = userManager;
            _orderRepository = orderRepository;
        }

        public async Task<PaginatedResult<AdminUserListItemDto>> GetUsersAsync(AdminUsersFilterDto filter)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(Roles.Customer);
            var query = usersInRole.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(u => (u.FullName ?? string.Empty).ToLower().Contains(s) || (u.Email ?? string.Empty).ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                if (filter.Status == "Active") query = query.Where(u => u.IsActive);
                else if (filter.Status == "Suspended") query = query.Where(u => !u.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(filter.EmailConfirmed))
            {
                if (filter.EmailConfirmed == "Confirmed") query = query.Where(u => u.EmailConfirmed);
                else if (filter.EmailConfirmed == "NotConfirmed") query = query.Where(u => !u.EmailConfirmed);
            }

            var totalCount = query.Count();
            var pageNumber = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Adapt<List<AdminUserListItemDto>>();

            return new PaginatedResult<AdminUserListItemDto>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<AdminUserDetailsDto> GetUserDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var dto = user.Adapt<AdminUserDetailsDto>();

            var totalOrders = _orderRepository.GetAllAsNoTracking().Count(o => o.UserId == userId);
            var totalSpent = _orderRepository.GetAllAsNoTracking().Where(o => o.UserId == userId && o.Status == "Delivered").Sum(o => o.TotalPrice);

            dto.TotalOrders = totalOrders;
            dto.TotalSpent = totalSpent;

            return dto;
        }

        public async Task ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
        }

        public async Task SuspendUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            await _userManager.DeleteAsync(user);
        }
    }
}
