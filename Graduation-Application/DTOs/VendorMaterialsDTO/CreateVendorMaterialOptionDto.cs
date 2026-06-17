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

        [Required]
        [Range(typeof(decimal), "0.01", "100000000", ErrorMessage = "Price delta must be greater than zero.")]
        public decimal PriceDelta { get; set; }
    }
}
