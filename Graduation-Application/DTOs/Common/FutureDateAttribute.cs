using System;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.Common
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                // Allow a small buffer of 5 minutes to account for latency between client and server
                if (dateTime < DateTime.UtcNow.AddMinutes(-1))
                {
                    return new ValidationResult(ErrorMessage ?? "Estimated delivery date must be in the future (current time or later).");
                }
            }
            return ValidationResult.Success;
        }
    }
}
