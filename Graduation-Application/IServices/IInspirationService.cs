using System.Threading.Tasks;
using Graduation_Application.DTOs.InspirationDTOs;

namespace Graduation_Application.IServices
{
    public interface IInspirationService
    {
        Task UploadInspirationsAsync(string userId, UploadInspirationsDto dto);
        Task<InspirationResultDto> GetApprovedInspirationsAsync(int pageNumber, int pageSize);
    }
}
