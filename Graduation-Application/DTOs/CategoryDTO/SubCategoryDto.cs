using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.CategoryDTO
{
    public class SubCategoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
    }

    public class CreateSubCategoryDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }

    public class UpdateSubCategoryDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
