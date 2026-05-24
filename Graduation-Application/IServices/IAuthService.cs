using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;

namespace Graduation_Application.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
