using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.UserDTO
{
    public class ResendConfirmationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
