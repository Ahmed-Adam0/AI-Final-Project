using System;

namespace Graduation_Application.DTOs.VendorDTO
{
    public class VendorResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; } = string.Empty;
        public string WorkshopNameEn { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        // Full URL to workshop logo
        public string? LogoUrl { get; set; }
    }
}
