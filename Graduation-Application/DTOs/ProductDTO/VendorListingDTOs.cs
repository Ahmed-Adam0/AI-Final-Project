using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.ProductDTO
{
    // ─────────────────────────────────────────────────────────────────────────
    // Vendor Listing DTOs
    // Used by vendors (workshops) to create/manage their product listings
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A vendor creates a listing to offer an existing base Product for sale.
    /// The vendor provides their own price and availability.
    /// </summary>
    public class CreateVendorListingDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "BasePrice must be greater than zero")]
        public decimal BasePrice { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateVendorListingDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "BasePrice must be greater than zero")]
        public decimal BasePrice { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    public class VendorListingResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductNameAr { get; set; } = string.Empty;
        public string ProductNameEn { get; set; } = string.Empty;
        public int WorkshopId { get; set; }
        public string WorkshopNameEn { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsAvailable { get; set; }
        public int VariantCount { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Product Variant DTOs
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new variant under a vendor listing.
    /// The variant is defined by a set of attribute value IDs (one per attribute dimension).
    /// FinalPrice = Listing.BasePrice + PriceDelta
    /// </summary>
    public class CreateProductVariantDto
    {
        [Required]
        public int ListingId { get; set; }

        public decimal PriceDelta { get; set; } = 0m;

        public string? VariantImageUrl { get; set; }

        /// <summary>
        /// IDs of the ProductAttributeValues that compose this variant.
        /// Must contain exactly one value per attribute dimension of the product.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one attribute value selection is required")]
        public List<int> AttributeValueIds { get; set; } = [];
    }

    public class UpdateProductVariantDto
    {
        public decimal PriceDelta { get; set; } = 0m;
        public string? VariantImageUrl { get; set; }
    }

    public class ProductVariantResponseDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public decimal PriceDelta { get; set; }
        public decimal CurrentPrice { get; set; }
        public string? VariantImageUrl { get; set; }
        public List<SelectedAttributeDto> SelectedAttributes { get; set; } = [];
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Product Attribute DTOs (Admin — defines variation dimensions per product)
    // ─────────────────────────────────────────────────────────────────────────

    public class CreateProductAttributeDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required, MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;
    }

    public class CreateAttributeValueDto
    {
        [Required]
        public int AttributeId { get; set; }

        [Required, MaxLength(200)]
        public string ValueAr { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ValueEn { get; set; } = string.Empty;
    }
}
