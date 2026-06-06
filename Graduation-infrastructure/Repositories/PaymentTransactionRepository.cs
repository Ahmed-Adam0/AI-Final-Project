using System.Threading.Tasks;
using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PaymentTransaction transaction)
        {
            await _context.PaymentTransactions.AddAsync(transaction);
        }

        public async Task<PaymentTransaction?> GetByPaymobOrderIdAsync(string paymobOrderId)
        {
            if (int.TryParse(paymobOrderId, out var id))
            {
                return await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.PaymobOrderId == id);
            }
            return null;
        }

        public async Task<PaymentTransaction?> GetByLocalOrderIdAsync(int localOrderId)
        {
            return await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.LocalOrderId == localOrderId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
