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
        public int ProductTypeId { get; set; }
        public string ProductTypeNameAr { get; set; } = string.Empty;
        public string ProductTypeNameEn { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string SubCategoryNameAr { get; set; } = string.Empty;
        public string SubCategoryNameEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public bool IsHidden { get; set; }
        public decimal BasePrice { get; set; }
        
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; } = string.Empty;
        public string WorkshopNameEn { get; set; } = string.Empty;
        public string? WorkshopLogoUrl { get; set; }
        public decimal? WorkshopRating { get; set; }
        public bool WorkshopIsVerified { get; set; }

        public List<ProductImageDto> Images { get; set; } = [];

        /// <summary>All attribute dimensions defined for this product type (e.g., Material, Color).</summary>
        public List<ProductAttributeDto> Attributes { get; set; } = [];

        public List<ProductMaterialGroupResponseDto> MaterialGroups { get; set; } = [];

        public ProductDetailsDto()
        {
            Images = new List<ProductImageDto>();
            Attributes = new List<ProductAttributeDto>();
            MaterialGroups = new List<ProductMaterialGroupResponseDto>();
        }
    }

    public class ProductMaterialGroupResponseDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public List<ProductMaterialOptionDetailsDto> Options { get; set; } = [];
    }

    public class ProductMaterialOptionDetailsDto
    {
        public int Id { get; set; }
        public int VendorMaterialGroupId { get; set; }
        public string ValueAr { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;
        public decimal PriceOption { get; set; }
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
        public decimal PriceDelta { get; set; }
    }
}
