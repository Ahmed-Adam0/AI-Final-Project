using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.ProductDTO
{
    public class CreateProductAttributeDto
    {
        [Required]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string NameEn { get; set; } = string.Empty;
    }

    public class UpdateProductAttributeDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
    }

    public class CreateProductAttributeValueDto
    {
        [Required]
        public string ValueAr { get; set; } = string.Empty;

        [Required]
        public string ValueEn { get; set; } = string.Empty;

        public decimal PriceDelta { get; set; }
    }

    public class UpdateProductAttributeValueDto
    {
        public string ValueAr { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;
        public decimal? PriceDelta { get; set; }
    }
}
