using System;
using System.Threading.Tasks;
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

        public async Task SendAsync(string userId, string title, string message)
        {
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                title,
                message,
                createdAt = DateTime.UtcNow
            });
        }
    }
}
