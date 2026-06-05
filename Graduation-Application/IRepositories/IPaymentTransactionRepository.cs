using Graduation_domain.Entities;

namespace Graduation_Application.IRepositories
{
    public interface IPaymentTransactionRepository
    {
        Task AddAsync(PaymentTransaction transaction);
        Task SaveChangesAsync();
    }
}
