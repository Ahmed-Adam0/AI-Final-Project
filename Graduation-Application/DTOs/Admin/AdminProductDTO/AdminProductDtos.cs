using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.Admin.AdminProductDTO
{
    /// <summary>
    /// DTO for admin product list items.
    /// </summary>
    public class AdminProductListDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string CategoryName { get; set; }
        public string VendorName { get; set; }
        public decimal Price { get; set; }
        public bool IsHidden { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MainImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for admin product details view.
    /// </summary>
    public class AdminProductDetailsDto
    {
        // Product Info
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal Price { get; set; }
        public bool IsHidden { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Category Info
        // Category Info
        public int ProductTypeId { get; set; }
        public string ProductTypeNameAr { get; set; } = string.Empty;
        public string ProductTypeNameEn { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string SubCategoryNameAr { get; set; } = string.Empty;
        public string SubCategoryNameEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;

        // Vendor Info (from ApplicationUser)
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }

        // Workshop Info
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; }
        public string WorkshopNameEn { get; set; }

        // Images
        public List<AdminProductImageDto> Images { get; set; } = new List<AdminProductImageDto>();

        // Reviews
        public int ReviewsCount { get; set; }
        public double AverageRating { get; set; }
    }

    /// <summary>
    /// DTO for product images in admin views.
    /// </summary>
    public class AdminProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// Filter DTO for admin product listing with search, category, vendor, and status filters.
    /// </summary>
    public class AdminProductFilterDto
    {
        public string Search { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? ProductTypeId { get; set; }
        public string VendorId { get; set; }
        public bool? IsHidden { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO for reporting a product (user-facing).
    /// </summary>
    public class ProductReportDto
    {
        public string Reason { get; set; }
    }

    /// <summary>
    /// DTO for displaying reported products in admin views.
    /// </summary>
    public class ReportedProductDto
    {
        public int ReportId { get; set; }
        public int ProductId { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameAr { get; set; }
        public string Reason { get; set; }
        public string ReportedByName { get; set; }
        public string ReportedByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }

    /// <summary>
    /// DTO for populating category dropdown in admin filters.
    /// </summary>
    public class CategoryDropdownDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
    }

    /// <summary>
    /// DTO for populating vendor dropdown in admin filters.
    /// </summary>
    public class VendorDropdownDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
