namespace Graduation_domain.Entities
{
    /// <summary>
    /// A specific, purchasable configuration of attribute values under a vendor listing.
    /// Example: Workshop A's Sofa listing in [Oak Material + Beige Fabric + Large Size].
    ///
    /// Pricing formula: FinalPrice = Listing.BasePrice + PriceDelta
    ///
    /// CurrentPrice is a denormalized cache of FinalPrice — updated by the application
    /// whenever BasePrice or PriceDelta changes. AI/search reads CurrentPrice directly
    /// without any joins across the listing → product chain.
    /// </summary>
    public class ProductVariant : BaseEntity<int>
    {
        public int ListingId { get; set; }
        public VendorProductListing Listing { get; set; } = null!;

        /// <summary>
        /// Additional price on top of Listing.BasePrice for this specific combination.
        /// Use 0 for the default / least expensive configuration.
        /// </summary>
        public decimal PriceDelta { get; set; } = 0m;

        /// <summary>
        /// Denormalized computed price: Listing.BasePrice + PriceDelta.
        /// ALWAYS keep this in sync when updating BasePrice or PriceDelta.
        /// This is the authoritative price for AI recommendations and catalog search.
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>Optional image that shows what this specific combination looks like.</summary>
        public string? VariantImageUrl { get; set; }

        // ── Navigations ──────────────────────────────────────────────────────────
        /// <summary>The set of attribute values that define this variant combination.</summary>
        public List<ProductVariantAttributeValue> VariantAttributeValues { get; set; } = [];

        /// <summary>Order items that captured a snapshot of this variant at purchase time.</summary>
        public List<OrderItem> OrderItems { get; set; } = [];
    }
}
