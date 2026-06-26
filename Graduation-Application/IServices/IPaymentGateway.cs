using System.Threading.Tasks;
using Graduation_domain.Entities;

namespace Graduation_Application.IServices
{
    public interface IPaymentGateway
    {
        Task<PaymentTransactionResult> CreatePaymentUrlAsync(
            int orderId,
            decimal amount,
            string firstName,
            string lastName,
            string email,
            string phone);
    }

    public class PaymentTransactionResult
    {
        public string PaymentUrl { get; set; } = null!;
        public PaymentTransaction Transaction { get; set; } = null!;
    }
}
