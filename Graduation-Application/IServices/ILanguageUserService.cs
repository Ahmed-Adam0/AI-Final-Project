using Graduation_Application.DTOs.UserLanguageDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.IServices
{
    public interface ILanguageUserService
    {
        Task<string> GetUserLanguageAsync(string userId);
        Task UpdateUserLanguageAsync(UpdateUserLanguageCommand command);
    }
}
