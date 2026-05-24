using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.IServices;
using Graduation_Application.Mapper;
using Graduation_domain.Entities;
using Microsoft.AspNetCore.Identity;
using Mapster;
using Graduation_Application.DTOs.RolesDTO;

namespace Graduation_Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            RoleMappingConfig.RegisterMappings();
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var exists = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (exists)
            {
                throw new System.Exception($"Role '{dto.RoleName}' already exists");
            }

            var role = new IdentityRole(dto.RoleName);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var err = string.Join(';', result.Errors.Select(e => e.Description));
                throw new System.Exception(err);
            }

            return role.Adapt<RoleResponseDto>();
        }

        public async Task AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.UserEmail);
            if (user == null)
            {
                throw new System.Exception($"User with email '{dto.UserEmail}' not found");
            }

            var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (!roleExists)
            {
                throw new System.Exception($"Role '{dto.RoleName}' does not exist");
            }

            var hasRole = await _userManager.IsInRoleAsync(user, dto.RoleName);
            if (hasRole)
            {
                return;
            }

            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
            if (!result.Succeeded)
            {
                var err = string.Join(';', result.Errors.Select(e => e.Description));
                throw new System.Exception(err);
            }
        }

        public async Task<List<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = _roleManager.Roles.ToList();
            return roles.Adapt<List<RoleResponseDto>>();
        }
    }
}