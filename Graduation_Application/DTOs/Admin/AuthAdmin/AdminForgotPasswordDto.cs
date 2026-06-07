using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.Admin.AuthAdmin
{
    public class AdminForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
