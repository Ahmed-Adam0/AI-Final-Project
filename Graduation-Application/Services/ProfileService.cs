using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Graduation_Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileRepository _profileRepository;

        public ProfileService(UserManager<ApplicationUser> userManager,IProfileRepository profileRepository)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _profileRepository.GetWithAddressesAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            return user.Adapt<UserProfileDto>();
        }

        public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _profileRepository.GetWithAddressesAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            // validate username
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                var exists = await _profileRepository.UsernameExistsAsync(dto.UserName, userId);
                if (exists) throw new System.Exception("Username already taken.");
            }

            // Mapster بيعدل بس الـ properties المحددة في الـ config
            dto.Adapt(user, typeof(UpdateProfileDto), typeof(ApplicationUser));

            // عدّل الـ Addresses لو موجودة
            List<Address>? newAddresses = null;
            if (dto.Addresses != null)
            {
                newAddresses = dto.Addresses.Adapt<List<Address>>();
                newAddresses.ForEach(a => a.UserId = userId);
            }

            await _profileRepository.UpdateProfileAsync(user, newAddresses);

            var updatedUser = await _profileRepository.GetWithAddressesAsync(userId);
            return updatedUser!.Adapt<UserProfileDto>();
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            // Check if old password equals new password
            if (dto.OldPassword == dto.NewPassword)
            {
                throw new System.Exception("New password must be different from old password");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ; ", result.Errors.Select(e => e.Description));
                throw new System.Exception(errors);
            }
        }
    }
}
