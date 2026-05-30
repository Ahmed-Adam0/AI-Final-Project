using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;

namespace Graduation_Application.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<bool> VerifyOtpAsync(VerifyOtpDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task ConfirmEmailOtpAsync(ConfirmEmailOtpDto dto);
        Task ResendConfirmationEmailAsync(ResendConfirmationDto dto);
        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
    }
}
