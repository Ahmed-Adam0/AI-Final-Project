using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.Admin.AuthAdmin
{
    public class AdminResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OtpCode { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
