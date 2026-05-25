using System.ComponentModel.DataAnnotations;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.DTOs.UserDTO
{
    public class ChangePasswordDto
    {
        [Required]
        [MinLength(8)]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [StrongPassword]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
