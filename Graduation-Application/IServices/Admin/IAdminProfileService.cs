using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminProfileDTO;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminProfileService
    {
        Task<AdminProfileDto> GetProfileAsync(string adminId);
        Task<AdminProfileDto> UpdateProfileAsync(string adminId, AdminUpdateProfileDto dto);
        Task ChangePasswordAsync(string adminId, AdminChangePasswordDto dto);
    }
}
