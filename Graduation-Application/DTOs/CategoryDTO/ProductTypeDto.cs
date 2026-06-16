using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.CategoryDTO
{
    public class ProductTypeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string SubCategoryNameAr { get; set; } = string.Empty;
        public string SubCategoryNameEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
    }

    public class CreateProductTypeDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
    }

    public class UpdateProductTypeDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
    }
}
