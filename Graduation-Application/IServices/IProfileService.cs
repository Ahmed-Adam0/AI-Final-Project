using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;
using Microsoft.AspNetCore.Http;

namespace Graduation_Application.IServices
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<UserProfileDto> UpdateProfileImageAsync(string userId, IFormFile file);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
