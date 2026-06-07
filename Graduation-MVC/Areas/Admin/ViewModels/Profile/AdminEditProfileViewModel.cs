using Microsoft.AspNetCore.Http;

namespace Graduation_MVC.Areas.Admin.ViewModels.Profile
{
    public class AdminEditProfileViewModel
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredLanguage { get; set; }
        public IFormFile ProfileImage { get; set; }
        public string CurrentProfileImage { get; set; }
    }
}
