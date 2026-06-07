using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.Admin.AuthAdmin
{
    public class AdminLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
