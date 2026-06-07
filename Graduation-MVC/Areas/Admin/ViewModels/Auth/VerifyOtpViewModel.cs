using System.ComponentModel.DataAnnotations;

namespace Graduation_MVC.Areas.Admin.ViewModels.Auth
{
    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OtpCode { get; set; }
    }
}
