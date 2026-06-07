using Microsoft.AspNetCore.Http;

namespace Graduation_Application.DTOs.Admin.AdminProfileDTO
{
    public class AdminUpdateProfileDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredLanguage { get; set; }
        public IFormFile ProfileImage { get; set; }
    }
}
