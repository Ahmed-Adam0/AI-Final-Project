using Graduation_domain.Entities;
using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.UserDTO
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        // Full URL to profile image (e.g. https://api.example.com/uploads/profiles/abc.jpg)
        public string? ProfileImage { get; set; }
        public string PreferredLanguage { get; set; } = "ar";
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public List<AddressDto>? Addresses { get; set; }

        // Backend-driven UI controls
        public bool IsGoogleUser { get; set; }
        public bool CanEditEmail { get; set; }
    }
}
