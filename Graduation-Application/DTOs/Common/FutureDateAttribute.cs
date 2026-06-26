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
                if (dateTime.Date < DateTime.UtcNow.Date)
                {
                    return new ValidationResult(ErrorMessage ?? "Estimated delivery date must be today or in the future.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
