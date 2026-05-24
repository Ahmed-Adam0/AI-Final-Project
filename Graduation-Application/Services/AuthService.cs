using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.IServices;
using Graduation_Application.Mapper.UsersMapping;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Graduation_Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);

            if (userExists != null)
            {
                throw new Exception($"User with email '{dto.Email}' already exists");
            }

            // Mapping RegisterDto → ApplicationUser
            var user = dto.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ; ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            // Create role if not exists
            var roleExists = await _roleManager.RoleExistsAsync("Customer");

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // Add role to user
            await _userManager.AddToRoleAsync(user, "Customer");

            // Generate token
            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            // Mapping → AuthResponseDto
            return (user, token, roles).Adapt<AuthResponseDto>();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception($"InValid Email Or Password");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordValid)
            {
                throw new Exception("InValid Email Or Password");
            }

            if (!user.IsActive)
            {
                throw new Exception("User account is inactive");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            // Mapping → AuthResponseDto
            return (user, token, roles).Adapt<AuthResponseDto>();
        }
    }
}
