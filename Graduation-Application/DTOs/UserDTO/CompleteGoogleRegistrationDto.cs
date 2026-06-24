using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.UserDTO
{
    public class CompleteGoogleRegistrationDto
    {
        [Required(ErrorMessage = "Registration token is required")]
        public string RegistrationToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string PreferredLanguage { get; set; } = "en";
    }
}
