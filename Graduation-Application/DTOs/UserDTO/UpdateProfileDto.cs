using Graduation_domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.UserDTO
{
    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }


        [Required]
        public string PreferredLanguage { get; set; } = "ar";

        public string? PhoneNumber { get; set; }

        public string? UserName { get; set; }
        public List<AddressDto>? Addresses { get; set; }

    }
}
