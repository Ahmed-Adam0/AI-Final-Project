using System;
using System.ComponentModel.DataAnnotations;
using Graduation_Application.IServices.Admin;

namespace Graduation_Application.DTOs.Common
{
    public class FutureDateAttribute : ValidationAttribute
    {
        private const string DefaultMessageKey = "validation.estimatedDeliveryDateFuture";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                var targetDate = dateTime.Date;
                var todayLocal = DateTime.Today;
                var todayUtc = DateTime.UtcNow.Date;

                if (targetDate < todayLocal && targetDate < todayUtc)
                {
                    var localizationService = validationContext.GetService(typeof(ILocalizationService)) as ILocalizationService;
                    var messageKey = string.IsNullOrWhiteSpace(ErrorMessage) ? DefaultMessageKey : ErrorMessage;
                    var message = localizationService?.Get(messageKey) ?? "Estimated delivery date must be today or in the future.";
                    return new ValidationResult(message);
                }
            }

            return ValidationResult.Success;
        }
    }

    public class LocalizedRequiredAttribute : RequiredAttribute
    {
        private const string DefaultMessageKey = "validation.estimatedDeliveryDateRequired";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var isMissing = value == null || (value is string str && string.IsNullOrWhiteSpace(str));
            if (!isMissing)
            {
                return ValidationResult.Success;
            }

            var localizationService = validationContext.GetService(typeof(ILocalizationService)) as ILocalizationService;
            var messageKey = string.IsNullOrWhiteSpace(ErrorMessage) ? DefaultMessageKey : ErrorMessage;
            var message = localizationService?.Get(messageKey) ?? "Estimated delivery date is required.";
            return new ValidationResult(message);
        }
    }
}
