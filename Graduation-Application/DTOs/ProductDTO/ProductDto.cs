using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.DTOs.ProductDTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsHidden { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeNameAr { get; set; } = string.Empty;
        public string ProductTypeNameEn { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string SubCategoryNameAr { get; set; } = string.Empty;
        public string SubCategoryNameEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; } = string.Empty;
        public string WorkshopNameEn { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string MainImageUrl { get; set; } = string.Empty;
    }
}
