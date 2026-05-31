using System;

namespace Graduation_Application.DTOs.VendorDTO
{
    public class VendorProfileDto
    {
        // User fields
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string PreferredLanguage { get; set; }
        public string? ProfileImage { get; set; }

        // Workshop fields
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; }
        public string WorkshopNameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string? LogoUrl { get; set; }
        public decimal? Rating { get; set; }
        public bool IsVerified { get; set; }
        public WorkshopAddressDto? WorkshopAddress { get; set; }
    }
}
