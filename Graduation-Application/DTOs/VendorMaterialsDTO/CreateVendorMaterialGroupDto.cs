using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.VendorMaterialsDTO
{
    public class CreateVendorMaterialGroupDto
    {
        [Required]
        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;
    }
}
