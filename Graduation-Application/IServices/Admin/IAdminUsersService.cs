using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.UsersDTO;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminUsersService
    {
        Task<PaginatedResult<AdminUserListItemDto>> GetUsersAsync(AdminUsersFilterDto filter);
        Task<AdminUserDetailsDto> GetUserDetailsAsync(string userId);
        Task ActivateUserAsync(string userId);
        Task SuspendUserAsync(string userId);
        Task DeleteUserAsync(string userId);
    }
}
