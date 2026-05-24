using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Graduation_Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            return user.Adapt<UserProfileDto>();
        }

        public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            // Validate username if provided (ensure it's not taken by another user)
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                var exists = await _userManager.FindByNameAsync(dto.UserName);
                if (exists != null && exists.Id != user.Id)
                {
                    throw new System.Exception("Username already taken.");
                }
            }

            // Map fields from DTO onto existing user instance using Mapster.
            // This overwrites: FullName, PreferredLanguage, Email, ProfileImage, PhoneNumber, UserName
            dto.Adapt(user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ; ", result.Errors.Select(e => e.Description));
                throw new System.Exception(errors);
            }

            return user.Adapt<UserProfileDto>();
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ; ", result.Errors.Select(e => e.Description));
                throw new System.Exception(errors);
            }
        }
    }
}
