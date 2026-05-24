using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Graduation_Application.DTOs.UserDTO
{
    public class RegisterDto
    {
        
            [Required]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [MinLength(8)]
            public string Password { get; set; }

            [Required]
            [Compare("Password")]
            public string ConfirmPassword { get; set; }

            [Required]
           
            public string? PhoneNumber { get; set; } 

            public string PreferredLanguage { get; set; } = "ar";
        }
    }

