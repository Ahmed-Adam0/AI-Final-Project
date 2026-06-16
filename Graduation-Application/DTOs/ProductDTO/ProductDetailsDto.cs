using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.ProductDTO
{
    /// <summary>
    /// Full product detail response including all vendor listings and variant options.
    /// </summary>
    public class ProductDetailsDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public List<ProductImageDto> Images { get; set; } = [];

        /// <summary>All attribute dimensions defined for this product type (e.g., Material, Color).</summary>
        public List<ProductAttributeDto> Attributes { get; set; } = [];

        /// <summary>All vendor listings available for this product.</summary>
        public List<VendorListingSummaryDto> VendorListings { get; set; } = [];

        public ProductDetailsDto()
        {
            Images = new List<ProductImageDto>();
            Attributes = new List<ProductAttributeDto>();
            VendorListings = new List<VendorListingSummaryDto>();
        }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    /// <summary>A single attribute dimension (e.g., "Material") with its selectable values.</summary>
    public class ProductAttributeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public List<AttributeValueDto> Values { get; set; } = [];
    }

    public class AttributeValueDto
    {
        public int Id { get; set; }
        public string ValueAr { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;
    }

    /// <summary>Summary of a vendor's listing for display on the product page.</summary>
    public class VendorListingSummaryDto
    {
        public int ListingId { get; set; }
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; } = string.Empty;
        public string WorkshopNameEn { get; set; } = string.Empty;
        public string? WorkshopLogoUrl { get; set; }
        public decimal? WorkshopRating { get; set; }
        public bool WorkshopIsVerified { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsAvailable { get; set; }

        /// <summary>All purchasable variant combinations this vendor offers.</summary>
        public List<VariantSummaryDto> Variants { get; set; } = [];
    }

    /// <summary>A single purchasable variant within a vendor listing.</summary>
    public class VariantSummaryDto
    {
        public int VariantId { get; set; }
        public decimal CurrentPrice { get; set; }
        public string? VariantImageUrl { get; set; }

        /// <summary>The selected attribute values that define this variant combination.</summary>
        public List<SelectedAttributeDto> SelectedAttributes { get; set; } = [];
    }

    public class SelectedAttributeDto
    {
        public string AttributeNameEn { get; set; } = string.Empty;
        public string AttributeNameAr { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;
        public string ValueAr { get; set; } = string.Empty;
    }
}
