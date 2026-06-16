using System.Collections.Generic;

namespace Graduation_domain.Entities
{
    /// <summary>
    /// The canonical, vendor-agnostic definition of a furniture product.
    ///
    /// What changed vs. the old model:
    ///   - REMOVED: Price (now lives on VendorProductListing / ProductVariant)
    ///   - REMOVED: WorkshopId / UserId (vendor ownership is now in VendorProductListing)
    ///   - ADDED:   VendorListings navigation (one product → many vendor offers)
    ///   - ADDED:   Attributes navigation (defines variant dimensions for this product type)
    ///
    /// All identity fields (NameAr/En, DescriptionAr/En, Category, Images, Status) are unchanged.
    /// </summary>
    public class Product : BaseEntity<int>
    {
        public bool IsHidden { get; set; } = false;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;

        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;

        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; } = null!;

        public decimal BasePrice { get; set; }

        // ── Navigations ──────────────────────────────────────────────────────────
        /// <summary>
        /// Attribute dimensions available for this product type (e.g., "Material", "Color").
        public List<ProductAttribute> Attributes { get; set; } = [];

        public List<ProductMaterialOption> MaterialOptions { get; set; } = [];

        public List<ProductImage>? Images { get; set; }
    }
}
