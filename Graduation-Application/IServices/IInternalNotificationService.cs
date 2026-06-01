using System.Threading.Tasks;
using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.NotificationDTO;
using Graduation_domain.Entities;

namespace Graduation_Application.IServices
{
    public interface IInternalNotificationService
    {
        Task CreateAsync(string userId, NotificationType type, string? messageParams = null);
        Task<PaginatedResult<InternalNotificationDto>> GetNotificationsAsync(string userId, string lang, int page, int pageSize);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(string userId, int notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}
