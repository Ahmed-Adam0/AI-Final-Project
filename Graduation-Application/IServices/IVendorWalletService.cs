using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.WalletDTO;

namespace Graduation_Application.IServices
{
    /// <summary>
    /// Service responsible for vendor wallet queries and withdrawal processing.
    /// Commission calculation and wallet crediting are handled by <see cref="IPaymentService"/>.
    /// </summary>
    public interface IVendorWalletService
    {
        /// <summary>Returns the current wallet state for the given workshop.</summary>
        Task<VendorWalletDto> GetWalletAsync(int workshopId);

        /// <summary>
        /// Processes a withdrawal request: validates balance, sends payout via <see cref="IPayoutGateway"/>,
        /// and updates wallet balances accordingly.
        /// </summary>
        Task<VendorWithdrawalResultDto> RequestWithdrawalAsync(int workshopId, RequestWithdrawalDto dto);

        /// <summary>Returns all withdrawal records for the given workshop, newest first.</summary>
        Task<IEnumerable<VendorWithdrawalDto>> GetWithdrawalsAsync(int workshopId);
    }
}
