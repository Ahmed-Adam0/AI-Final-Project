using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task SendOrderConfirmationAsync(string userId, int orderId, decimal totalPrice);
        Task SendOrderStatusUpdateAsync(string userId, int orderId, string newStatus);
        Task SendOrderCancellationAsync(string userId, int orderId);
    }
}
