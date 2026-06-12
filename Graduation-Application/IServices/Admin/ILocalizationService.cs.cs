using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.IServices.Admin
{
    public interface ILocalizationService
    {
        /// <summary>
        /// Get localized string by key for the current culture
        /// </summary>
        string Get(string key);

        /// <summary>
        /// Get localized string by key for a specific culture
        /// </summary>
        string Get(string key, string culture);

        /// <summary>
        /// Get current active culture (Cookie > DB > default "en")
        /// </summary>
        string GetCurrentCulture();

        /// <summary>
        /// Set culture in cookie and database
        /// </summary>
        Task SetCultureAsync(string culture);
    }
}
