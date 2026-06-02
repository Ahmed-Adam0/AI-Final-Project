using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.VendorManagementDTO;

namespace Graduation_Application.IRepositories.Admin
{
    public interface IAdminVendorsRepository
    {
        Task<AdminVendorStatisticsDto> GetStatisticsAsync();
        Task<AdminVendorsPageDto> GetVendorsPageAsync(AdminVendorsFilterDto filter, bool pendingOnly);
        Task<AdminVendorDetailsDto?> GetVendorDetailsAsync(int workshopId);
        Task<AdminVendorHistoryPageDto> GetHistoryAsync(int page, int pageSize);
    }
}

