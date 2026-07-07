namespace Graduation_domain.Enums
{
    /// <summary>Lifecycle states of a vendor withdrawal request.</summary>
    public enum WithdrawalStatus
    {
        /// <summary>Request submitted, payout not yet processed.</summary>
        Pending = 0,

        /// <summary>Payout successfully sent to the vendor's wallet.</summary>
        Completed = 1,

        /// <summary>Payout failed; no balance was deducted.</summary>
        Failed = 2
    }
}
