using System.Threading.Tasks;
using Graduation_Application.DTOs.NotificationDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.SignalR;

namespace Graduation_infrastructure.SignalR
{
    public class NotificationHubService : INotificationHub
    {
        private readonly IHubContext<InternalNotificationHub> _hubContext;

        public NotificationHubService(IHubContext<InternalNotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendAsync(InternalNotificationDto notification)
        {
            await _hubContext.Clients.Group(notification.UserId).SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                isRead = notification.IsRead,
                createdAt = notification.CreatedAt
            });
        }
    }
}
