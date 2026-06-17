using Graduation_Application.DTOs.UserLanguageDTO;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.Services
{
    public class LanguageUserService : ILanguageUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LanguageUserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GetUserLanguageAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.PreferredLanguage ?? "en";
        }

        public async Task UpdateUserLanguageAsync(UpdateUserLanguageCommand command)
        {
            var user = await _userManager.FindByIdAsync(command.UserId);

            if (user == null)
                throw new Exception("User not found");

            if (command.Language != "ar" && command.Language != "en")
                throw new Exception("Invalid language");

            user.PreferredLanguage = command.Language;

            await _userManager.UpdateAsync(user);
        }
    }
}
