using System;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.VendorDTO
{
    public class CreateVendorDto
    {
        // User fields
        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string PreferredLanguage { get; set; } = "ar";

        // Workshop fields
        [Required(ErrorMessage = "WorkshopNameAr is required")]
        public string WorkshopNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "WorkshopNameEn is required")]
        public string WorkshopNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "DescriptionAr is required")]
        public string DescriptionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "DescriptionEn is required")]
        public string DescriptionEn { get; set; } = string.Empty;


        public WorkshopAddressDto? WorkshopAddress { get; set; }
    }
}

