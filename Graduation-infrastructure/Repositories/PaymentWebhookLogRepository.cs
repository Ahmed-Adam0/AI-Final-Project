using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;

namespace Graduation_infrastructure.Repositories
{
    public class PaymentWebhookLogRepository : IPaymentWebhookLogRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentWebhookLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PaymentWebhookLog log)
        {
            await _context.PaymentWebhookLogs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
