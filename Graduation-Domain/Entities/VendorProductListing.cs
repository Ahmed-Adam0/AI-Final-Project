namespace Graduation_domain.Entities
{
    /// <summary>
    /// Represents a specific vendor's (Workshop's) offer to sell a base Product.
    /// This is the pricing anchor — each vendor sets their own base price per product.
    /// One Product can have many VendorProductListings (one per vendor).
    /// </summary>
    public class VendorProductListing : BaseEntity<int>
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; } = null!;

        /// <summary>
        /// The vendor's base price for this product before any variant delta is applied.
        /// FinalPrice = BasePrice + ProductVariant.PriceDelta
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Whether the vendor is currently offering this product for purchase.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        // ── Navigations ──────────────────────────────────────────────────────────
        /// <summary>All purchasable variant configurations this vendor offers for this product.</summary>
        public List<ProductVariant> Variants { get; set; } = [];
    }
}
