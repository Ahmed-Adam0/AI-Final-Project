using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.VendorDTO
{
    public class VendorLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
