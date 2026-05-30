using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.UserDTO
{
    public class ConfirmEmailOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OtpCodeEmail { get; set; } = string.Empty;
    }
}
