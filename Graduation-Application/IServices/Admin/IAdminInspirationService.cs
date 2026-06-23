using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.Inspirations;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminInspirationService
    {
        Task<PaginatedResult<AdminInspirationListDto>> GetInspirationsAsync(AdminInspirationFilterDto filter);
        Task<bool> ApproveInspirationAsync(int id);
    }
}
