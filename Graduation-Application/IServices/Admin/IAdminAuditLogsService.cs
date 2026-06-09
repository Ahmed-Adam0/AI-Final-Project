using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AuditLogsDTO;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminAuditLogsService
    {
        Task<PaginatedResult<AuditLogListItemDto>> GetLogsAsync(AuditLogsFilterDto filter);
        Task<AuditLogDetailsDto> GetLogDetailsAsync(int id);
        Task CreateLogAsync(
            string userId,
            string userName,
            string userRole,
            string action,
            string entityType,
            string? entityId,
            string description
        );
    }
}
