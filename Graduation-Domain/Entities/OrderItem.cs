using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    /// <summary>
    /// A single line item in a placed Order.
    ///
    /// SNAPSHOT PATTERN: All pricing and configuration data is FROZEN at the moment
    /// the user places the order. The referenced variant/listing/product can be updated
    /// or deleted later — this record's snapshot columns never change.
    ///
    /// What changed vs. the old model:
    ///   - REMOVED: ProductId FK (replaced by nullable ProductVariantId)
    ///   - RENAMED: UnitPrice → SnapshotUnitPrice
    ///   - ADDED:   SnapshotProductNameAr/En, SnapshotVendorName, SnapshotAttributesJson
    /// </summary>
    public class OrderItem : BaseEntity<int>
    {
        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // ── Live soft-reference (nullable — variant may be deleted later) ─────────
        /// <summary>
        /// Points to the variant that was purchased.
        /// SetNull on delete — the snapshot columns below guarantee data is never lost.
        /// </summary>
        public int? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        // ── Immutable historical snapshot ─────────────────────────────────────────
        // IMPORTANT: These columns are written once at checkout and MUST NEVER be updated.

        /// <summary>Price per unit at the exact moment of purchase.</summary>
        [Required]
        public decimal SnapshotUnitPrice { get; set; }

        /// <summary>Product name in Arabic at the time of purchase.</summary>
        [Required]
        [MaxLength(300)]
        public string SnapshotProductNameAr { get; set; } = string.Empty;

        /// <summary>Product name in English at the time of purchase.</summary>
        [Required]
        [MaxLength(300)]
        public string SnapshotProductNameEn { get; set; } = string.Empty;

        /// <summary>Workshop/vendor display name at the time of purchase.</summary>
        [Required]
        [MaxLength(200)]
        public string SnapshotVendorName { get; set; } = string.Empty;

        /// <summary>
        /// JSON-serialized list of the chosen attribute values at purchase time.
        /// Format: [{"nameEn":"Material","nameAr":"الخامة","valueEn":"Oak","valueAr":"بلوط"}]
        /// Stored as a raw JSON string for simplicity and immutability.
        /// </summary>
        public string SnapshotAttributesJson { get; set; } = "[]";

        [Required]
        public int Quantity { get; set; }
    }
}
