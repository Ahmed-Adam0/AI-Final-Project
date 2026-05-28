using System.Threading.Tasks;
using Graduation_Application.DTOs.NotificationDTO;

namespace Graduation_Application.IServices
{
    public interface IWhatsAppService
    {
        Task SendTemplateAsync(WhatsAppNotificationRequest request);
    }
}
