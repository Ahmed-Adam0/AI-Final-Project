using System;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminProfileDTO;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Graduation_Application.Services.Admin
{
    public class AdminProfileService : IAdminProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;

        public AdminProfileService(
            UserManager<ApplicationUser> userManager,
            IFileService fileService)
        {
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<AdminProfileDto> GetProfileAsync(string adminId)
        {
            var user = await _userManager.FindByIdAsync(adminId);
            if (user == null)
            {
                throw new Exception("Admin not found");
            }

            return user.Adapt<AdminProfileDto>();
        }

        public async Task<AdminProfileDto> UpdateProfileAsync(string adminId, AdminUpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(adminId);
            if (user == null)
            {
                throw new Exception("Admin not found");
            }

            // Update basic information
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.PreferredLanguage = dto.PreferredLanguage;

            // Update profile image if provided
            if (dto.ProfileImage != null)
            {
                user.ProfileImage = await _fileService.SaveImageAsync(dto.ProfileImage, "admins", user.ProfileImage);
            }
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userManager.UpdateAsync(user);

            return user.Adapt<AdminProfileDto>();
        }

        public async Task ChangePasswordAsync(string adminId, AdminChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(adminId);
            if (user == null)
            {
                throw new Exception("Admin not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errorMessage);
            }
        }
    }
}
