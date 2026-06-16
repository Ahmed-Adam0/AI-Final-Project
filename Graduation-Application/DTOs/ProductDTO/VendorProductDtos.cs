using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.ProductDTO
{
    /// <summary>
    /// DTO for vendor product list view
    /// Used in vendor dashboard endpoints
    /// Includes status information useful for vendor management
    /// </summary>
    public class VendorProductListDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CategoryId { get; set; }
        public int ReviewCount { get; set; }
        public decimal AverageRating { get; set; }
    }

    /// <summary>
    /// DTO for vendor product update status
    /// </summary>
    public class UpdateVendorProductStatusDto
    {
        public bool IsActive { get; set; }
    }
}
