using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.WalletDTO;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminWithdrawalsService
    {
        /// <summary>
        /// Retrieves all withdrawal requests in the system, sorted by newest first.
        /// </summary>
        Task<IEnumerable<AdminWithdrawalDto>> GetAllWithdrawalsAsync();

        /// <summary>
        /// Approves and processes a pending withdrawal request.
        /// </summary>
        Task<bool> CompleteWithdrawalAsync(int withdrawalId);

        /// <summary>
        /// Rejects a pending withdrawal request.
        /// </summary>
        Task<bool> RejectWithdrawalAsync(int withdrawalId);
    }
}
