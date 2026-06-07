using System.Threading.Tasks;
using Graduation_domain.Entities;

namespace Graduation_Application.IServices
{
    public interface IPaymentService
    {
        Task ProcessPaymentAsync(
            PaymentTransaction t,
            Order? o,
            bool success,
            string transactionId,
            string? failureReason = null
        );
    }
}
