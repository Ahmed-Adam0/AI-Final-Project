namespace Graduation_Application.DTOs.ProductDTO
{
    public class CreateProductDto
    {
        public int WorkshopId { get; set; }
        public int CategoryId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductDto
    {
        public int CategoryId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal Price { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public int CategoryId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
