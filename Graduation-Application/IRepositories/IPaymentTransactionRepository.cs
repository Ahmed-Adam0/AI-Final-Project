using System.Threading.Tasks;
using Graduation_domain.Entities;

namespace Graduation_Application.IRepositories
{
    public interface IPaymentTransactionRepository
    {
        Task AddAsync(PaymentTransaction transaction);
        Task<PaymentTransaction?> GetByPaymobOrderIdAsync(string paymobOrderId);
        Task<PaymentTransaction?> GetByLocalOrderIdAsync(int localOrderId);
        Task SaveChangesAsync();
    }
}
