using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Enums;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Graduation_Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileRepository _profileRepository;
        private readonly IFileService _fileService;
        private readonly IInternalNotificationService _internalNotificationService;

        public ProfileService(UserManager<ApplicationUser> userManager,IProfileRepository profileRepository, IFileService fileService, IInternalNotificationService internalNotificationService)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
            _fileService = fileService;
            _internalNotificationService = internalNotificationService;
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

            // enforce validation and reject email changes for Google users
            bool isGoogleUser = !string.IsNullOrEmpty(user.GoogleId);
            if (isGoogleUser)
            {
                if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new System.Exception("Google-authenticated users cannot change their email address manually.");
                }
                // Keep the dto email matching the user's current email so Mapster doesn't modify it
                dto.Email = user.Email;
            }
            else
            {
                // validate email for standard users
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var exists = await _profileRepository.EmailExistsAsync(dto.Email, userId);
                    if (exists) throw new System.Exception("Email already taken.");
                }
            }

            // Mapster will update allowed properties
            dto.Adapt(user, typeof(UpdateProfileDto), typeof(ApplicationUser));

            // Update Addresses if present
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

        public async Task<UserProfileDto> UpdateProfileImageAsync(string userId, Microsoft.AspNetCore.Http.IFormFile file)
        {
            var user = await _profileRepository.GetWithAddressesAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            var newUrl = await _fileService.SaveImageAsync(file, "profiles", user.ProfileImage);
            user.ProfileImage = newUrl;

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(" ; ", identityResult.Errors.Select(e => e.Description));
                throw new System.Exception(errors);
            }

            var updatedUser = await _profileRepository.GetWithAddressesAsync(userId);
            return updatedUser!.Adapt<UserProfileDto>();
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new System.Exception("User not found");

            if (!string.IsNullOrEmpty(user.GoogleId))
            {
                throw new System.Exception("Password change is not available for Google accounts.");
            }

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
            await _internalNotificationService.CreateAsync(userId, NotificationType.PasswordReset);

        }
    }
}
