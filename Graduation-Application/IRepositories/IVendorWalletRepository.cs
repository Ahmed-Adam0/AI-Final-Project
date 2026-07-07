using System.Threading.Tasks;
using Graduation_domain.Entities;

namespace Graduation_Application.IRepositories
{
    /// <summary>
    /// Custom repository for VendorWallet to support the upsert (find-or-create) pattern.
    /// All other wallet entity operations use <see cref="IGenaricRepositories{VendorWallet}"/>.
    /// </summary>
    public interface IVendorWalletRepository
    {
        /// <summary>Returns the wallet for a given workshop, or null if it doesn't exist yet.</summary>
        Task<VendorWallet?> GetByWorkshopIdAsync(int workshopId);

        /// <summary>Persists a newly created wallet record.</summary>
        Task AddAsync(VendorWallet wallet);

        Task SaveChangesAsync();
    }
}
