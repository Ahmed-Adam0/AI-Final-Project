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

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "BasePrice must be strictly greater than 0.")]
        public decimal BasePrice { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int>? VendorMaterialOptionIds { get; set; }
    }

    public class UpdateProductDto
    {
        public int CategoryId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public decimal? BasePrice { get; set; }
        public bool? IsActive { get; set; }
        public List<int>? VendorMaterialOptionIds { get; set; }
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

        public decimal BasePrice { get; set; }
        public int WorkshopId { get; set; }
        public bool IsHidden { get; set; }

        public List<int> VendorMaterialOptionIds { get; set; } = new List<int>();
    }
}
