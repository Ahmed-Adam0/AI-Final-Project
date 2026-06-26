using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode, int expiryMinutes);
        Task SendEmailConfirmationOtpAsync(string toEmail, string otpCode, int expiryMinutes);
        Task SendOrderCreatedEmailAsync(string toEmail, int orderId);
        Task SendOrderStatusChangedEmailAsync(string toEmail, int orderId, string newStatus);
        Task SendNewOrderVendorEmailAsync(string toEmail, int vendorOrderId);
        Task SendDeliveryDateProposedEmailAsync(string toEmail, int vendorOrderId, System.DateTime proposedDate);
        Task SendDeliveryDateApprovedEmailAsync(string toEmail, int vendorOrderId);
        Task SendDeliveryDateRejectedEmailAsync(string toEmail, int vendorOrderId);
        Task SendMilestoneCreatedEmailAsync(string toEmail, int vendorOrderId, string milestoneName, decimal amount, string lang);
        Task SendMilestonePaymentSuccessEmailAsync(string toEmail, int vendorOrderId, string milestoneName, decimal amount, string lang);
    }
}
