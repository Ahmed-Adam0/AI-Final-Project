using System.Threading.Tasks;
using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.Repositories
{
    public class VendorWalletRepository : IVendorWalletRepository
    {
        private readonly ApplicationDbContext _context;

        public VendorWalletRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<VendorWallet?> GetByWorkshopIdAsync(int workshopId)
        {
            return await _context.VendorWallets
                .FirstOrDefaultAsync(w => w.WorkshopId == workshopId);
        }

        /// <inheritdoc/>
        public async Task AddAsync(VendorWallet wallet)
        {
            await _context.VendorWallets.AddAsync(wallet);
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
