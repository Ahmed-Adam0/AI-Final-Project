using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.VendorMaterialsDTO
{
    public class UpdateVendorMaterialOptionDto
    {
        [Required(ErrorMessage = "Value in Arabic is required")]
        [StringLength(100, ErrorMessage = "Value cannot exceed 100 characters")]
        public string ValueAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Value in English is required")]
        [StringLength(100, ErrorMessage = "Value cannot exceed 100 characters")]
        public string ValueEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price delta is required")]
        [Range(typeof(decimal), "0.01", "100000000", ErrorMessage = "Price delta must be greater than zero.")]
        public decimal PriceDelta { get; set; }
    }
}
