using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminDashboardDTO;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<AdminOrdersPageDto> GetOrdersPageAsync(AdminOrdersFilterDto filter);
        Task<AdminOrderDetailsDto> GetOrderDetailsAsync(int orderId);
        Task<AdminAnalyticsDto> GetAnalyticsAsync(AdminAnalyticsFilterDto filter);
        Task<AdminReportsPageDto> GetReportsAsync(AdminReportsFilterDto filter);
        Task<AdminSettingsDto> GetSettingsAsync();
    }
}
