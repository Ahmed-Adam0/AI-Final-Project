using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.VendorMaterialsDTO
{
    public class CreateVendorMaterialOptionDto
    {
        [Required]
        [MaxLength(200)]
        public string ValueAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ValueEn { get; set; } = string.Empty;

        public decimal PriceDelta { get; set; }
    }
}
