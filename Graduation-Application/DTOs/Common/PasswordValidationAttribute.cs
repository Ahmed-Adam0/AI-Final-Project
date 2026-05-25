using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Graduation_Application.DTOs.Common
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            var password = value.ToString();

            if (string.IsNullOrEmpty(password))
                return false;

            bool hasUppercase = Regex.IsMatch(password, @"[A-Z]");
            bool hasNumber = Regex.IsMatch(password, @"[0-9]");
            bool hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*]");

            return hasUppercase && hasNumber && hasSpecialChar;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Password must contain at least 1 uppercase letter, 1 number, and 1 special character (!@#$%^&*)";
        }
    }
}
