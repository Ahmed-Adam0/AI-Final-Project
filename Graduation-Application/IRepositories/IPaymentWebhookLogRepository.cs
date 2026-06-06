using Graduation_domain.Entities;

namespace Graduation_Application.IRepositories
{
    public interface IPaymentWebhookLogRepository
    {
        Task AddAsync(PaymentWebhookLog log);
        Task SaveChangesAsync();
    }
}
