namespace Graduation_Application.DTOs.CategoryDTO
{
    public class CreateCategoryDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
    }
}
