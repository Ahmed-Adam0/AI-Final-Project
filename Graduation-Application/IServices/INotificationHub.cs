using System.Threading.Tasks;
using Graduation_Application.DTOs.NotificationDTO;

namespace Graduation_Application.IServices
{
    public interface INotificationHub
    {
        Task SendAsync(InternalNotificationDto notification);
    }
}
