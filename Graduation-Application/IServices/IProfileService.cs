using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;

namespace Graduation_Application.IServices
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
