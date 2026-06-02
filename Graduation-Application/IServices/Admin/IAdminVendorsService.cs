using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.VendorManagementDTO;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminVendorsService
    {
        Task<AdminVendorsPageDto> GetVendorsPageAsync(
            AdminVendorsFilterDto filter,
            bool pendingOnly = false
        );
        Task<AdminVendorDetailsDto?> GetVendorDetailsAsync(int workshopId);

        //Task ApproveVendorAsync(int workshopId, string performedByAdminId, string? notes = null);
        Task ApproveVendorAsync(int workshopId, string? notes = null);
        Task RejectVendorAsync(
            int workshopId,
            string performedByAdminId,
            string rejectionReason,
            string? notes = null
        );
        Task ActivateVendorAsync(int workshopId, string performedByAdminId, string? notes = null);
        Task SuspendVendorAsync(
            int workshopId,
            string performedByAdminId,
            string reason,
            string? notes = null
        );
        Task<AdminVendorHistoryPageDto> GetHistoryAsync(int page, int pageSize);
    }
}
