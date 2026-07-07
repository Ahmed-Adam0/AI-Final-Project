using System;
using Graduation_domain.Enums;

namespace Graduation_domain.Entities
{
    /// <summary>
    /// Represents a single vendor payout request.
    /// Created when a vendor requests a withdrawal from their wallet.
    /// </summary>
    public class VendorWithdrawal : BaseEntity<int>
    {
        /// <summary>FK to the Workshop (vendor) making the withdrawal.</summary>
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; } = null!;

        /// <summary>Withdrawal amount requested.</summary>
        public decimal Amount { get; set; }

        /// <summary>Paymob mobile wallet number to send the payout to.</summary>
        public string WalletNumber { get; set; } = null!;

        /// <summary>Current status of the withdrawal request.</summary>
        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

        /// <summary>Transaction reference returned by the payout provider on success.</summary>
        public string? TransactionReference { get; set; }

        /// <summary>Timestamp when the withdrawal was requested.</summary>
        public DateTime CreatedAt { get; set; }
    }
}
