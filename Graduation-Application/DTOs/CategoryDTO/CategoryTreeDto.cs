using System.Collections.Generic;

namespace Graduation_Application.DTOs.CategoryDTO
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<SubCategoryTreeDto> SubCategories { get; set; } = [];
    }

    public class SubCategoryTreeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public List<ProductTypeTreeDto> ProductTypes { get; set; } = [];
    }

    public class ProductTypeTreeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
    }
}
