using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);
        Task SendOrderConfirmationAsync(string userId, int orderId, decimal totalPrice);
        Task SendOrderStatusUpdateAsync(string userId, int orderId, string newStatus);
        Task SendOrderCancellationAsync(string userId, int orderId);
        Task SendVendorNewOrderAsync(string vendorUserId, int vendorOrderId, string customerName, decimal totalPrice);
        Task SendCustomerFirstPaymentAsync(string userId, int orderId, decimal amountPaid);
        Task SendVendorAfterPaymentAsync(string vendorUserId, int vendorOrderId, decimal amountPaid);
    }
}
