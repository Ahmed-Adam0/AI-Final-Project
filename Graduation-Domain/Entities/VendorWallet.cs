using System;

namespace Graduation_domain.Entities
{
    /// <summary>
    /// Represents a vendor's earnings wallet. One wallet per Workshop.
    /// Created automatically when the first successful payment is processed for that workshop.
    /// </summary>
    public class VendorWallet : BaseEntity<int>
    {
        /// <summary>FK to the Workshop that owns this wallet.</summary>
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; } = null!;

        /// <summary>Amount currently available for withdrawal (net of commission, minus previous withdrawals).</summary>
        public decimal AvailableBalance { get; set; } = 0m;

        /// <summary>Cumulative total of all completed withdrawals.</summary>
        public decimal TotalWithdrawn { get; set; } = 0m;

        /// <summary>Timestamp of the last balance update.</summary>
        public DateTime UpdatedAt { get; set; }
    }
}
