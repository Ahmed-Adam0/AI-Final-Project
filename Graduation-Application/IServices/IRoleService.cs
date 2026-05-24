using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.RolesDTO;

namespace Graduation_Application.IServices
{
    public interface IRoleService
    {
        Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto);
        Task AssignRoleAsync(AssignRoleDto dto);
        Task<List<RoleResponseDto>> GetAllRolesAsync();
    }
}