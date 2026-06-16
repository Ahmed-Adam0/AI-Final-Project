using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.ProductDTO
{
    // ─────────────────────────────────────────────────────────────────────────
    // Base Product CRUD (vendor-agnostic identity fields only)
    // Price and vendor are now managed via VendorProductListing / ProductVariant
    // ─────────────────────────────────────────────────────────────────────────

    public class CreateProductDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required]
        public string DescriptionEn { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductDto
    {
        public int CategoryId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>Lowest CurrentPrice across all active variants for this product (for display).</summary>
        public decimal? MinPrice { get; set; }

        /// <summary>Number of vendors currently listing this product.</summary>
        public int VendorCount { get; set; }
    }
}
