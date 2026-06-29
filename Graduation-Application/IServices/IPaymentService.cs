using System.Threading.Tasks;
using Graduation_domain.Entities;

namespace Graduation_Application.IServices
{
    public interface IPaymentService
    {
        Task ProcessPaymentAsync(
            PaymentTransaction t,
            Order? o,
            Graduation_Domain.Enums.PaymentStatus targetStatus,
            string transactionId,
            string? failureReason = null
        );
        Task<decimal> GetRemainingBalanceForMasterOrderAsync(int masterOrderId);
        Task<decimal> GetRemainingBalanceForVendorOrderAsync(int vendorOrderId);
        Task<object> GetMilestoneBreakdownForMasterOrderAsync(int masterOrderId);
        Task<object> GetMilestoneBreakdownForVendorOrderAsync(int vendorOrderId, int? workshopId = null);
        Task CreateMilestoneIfNotExistAsync(int vendorOrderId, Graduation_domain.Enums.VendorOrderStatus milestoneStatus, decimal totalAmount);
    }
}
