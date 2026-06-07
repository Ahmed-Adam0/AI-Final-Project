using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AuthAdmin;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminAuthService
    {
        Task<AdminAuthResultDto> LoginAsync(AdminLoginDto dto);
        Task ForgotPasswordAsync(AdminForgotPasswordDto dto);
        Task<bool> VerifyOtpAsync(AdminVerifyOtpDto dto);
        Task ResetPasswordAsync(AdminResetPasswordDto dto);
    }
}

