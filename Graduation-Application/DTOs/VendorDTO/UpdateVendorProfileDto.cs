using System;

namespace Graduation_Application.DTOs.VendorDTO
{
    public class UpdateVendorProfileDto
    {
        // User fields — all optional
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string PreferredLanguage { get; set; }

        // Workshop fields — all optional
        public string? WorkshopNameAr { get; set; }
        public string? WorkshopNameEn { get; set; }
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public WorkshopAddressDto? WorkshopAddress { get; set; }
    }
}
