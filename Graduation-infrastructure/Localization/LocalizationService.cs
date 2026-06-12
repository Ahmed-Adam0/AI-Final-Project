using Graduation_Application.IServices.Admin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Graduation_infrastructure.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Graduation_domain.Entities;

namespace Graduation_infrastructure.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _localizationPath;
        private readonly Dictionary<string, Dictionary<string, string>> _cache;
        private const string LANGUAGE_COOKIE = "culture";
        private const string DEFAULT_CULTURE = "en";

        public LocalizationService(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _userManager = userManager;
            _localizationPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "Localization"
            );
            _cache = new Dictionary<string, Dictionary<string, string>>();
        }

        public string Get(string key)
        {
            var culture = GetCurrentCulture();
            return Get(key, culture);
        }

        public string Get(string key, string culture)
        {
            if (string.IsNullOrEmpty(culture))
                culture = DEFAULT_CULTURE;

            // Load from cache or file
            if (!_cache.ContainsKey(culture))
            {
                System.Diagnostics.Debug.WriteLine($"🔄 [LOCALIZATION] Cache miss for {culture}, loading...");
                LoadLocalization(culture);
            }

            if (_cache.ContainsKey(culture) && _cache[culture].ContainsKey(key))
            {
                var value = _cache[culture][key];
                System.Diagnostics.Debug.WriteLine($"✓ [LOCALIZATION] Found '{key}' in {culture}: {value}");
                return value;
            }

            // Fallback to English if key not found
            if (culture != DEFAULT_CULTURE)
            {
                if (!_cache.ContainsKey(DEFAULT_CULTURE))
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 [LOCALIZATION] Cache miss for {DEFAULT_CULTURE}, loading fallback...");
                    LoadLocalization(DEFAULT_CULTURE);
                }

                if (_cache.ContainsKey(DEFAULT_CULTURE) && _cache[DEFAULT_CULTURE].ContainsKey(key))
                {
                    var value = _cache[DEFAULT_CULTURE][key];
                    System.Diagnostics.Debug.WriteLine($"⚠️  [LOCALIZATION] Fallback to {DEFAULT_CULTURE} for key '{key}': {value}");
                    return value;
                }
            }

            // If still not found, return the key itself
            System.Diagnostics.Debug.WriteLine($"❌ [LOCALIZATION] Key not found: {key} in culture {culture}");
            return key;
        }

        public string GetCurrentCulture()
        {
            // Priority 1: Cookie
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Cookies.TryGetValue(LANGUAGE_COOKIE, out var cookieCulture))
            {
                if (!string.IsNullOrEmpty(cookieCulture) && (cookieCulture == "en" || cookieCulture == "ar"))
                {
                    System.Diagnostics.Debug.WriteLine($"🍪 [LOCALIZATION] Culture from cookie: {cookieCulture}");
                    return cookieCulture;
                }
            }

            // Priority 2: Database (if user is logged in)
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = _userManager.FindByIdAsync(userId).Result;
                    if (user != null && !string.IsNullOrEmpty(user.PreferredLanguage))
                    {
                        System.Diagnostics.Debug.WriteLine($"💾 [LOCALIZATION] Culture from DB: {user.PreferredLanguage}");
                        return user.PreferredLanguage;
                    }
                }
            }

            // Priority 3: Default
            System.Diagnostics.Debug.WriteLine($"⚙️  [LOCALIZATION] Using default culture: {DEFAULT_CULTURE}");
            return DEFAULT_CULTURE;
        }

        public async Task SetCultureAsync(string culture)
        {
            if (string.IsNullOrEmpty(culture) || (culture != "en" && culture != "ar"))
            {
                culture = DEFAULT_CULTURE;
            }

            System.Diagnostics.Debug.WriteLine($"🔧 [LOCALIZATION] SetCultureAsync called with: {culture}");

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Set cookie
                httpContext.Response.Cookies.Append(
                    LANGUAGE_COOKIE,
                    culture,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        HttpOnly = false,
                        IsEssential = true,
                        // Removed SameSite to allow cookie on reload
                    }
                );
                System.Diagnostics.Debug.WriteLine($"🍪 [LOCALIZATION] Cookie set in response: {culture}");
            }

            // Update database if user is logged in
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        user.PreferredLanguage = culture;
                        await _userManager.UpdateAsync(user);
                        System.Diagnostics.Debug.WriteLine($"💾 [LOCALIZATION] Updated user DB preference to: {culture}");
                    }
                }
            }
        }

        private void LoadLocalization(string culture)
        {
            var fileName = $"localize_{culture}.json";
            var filePath = Path.Combine(_localizationPath, fileName);

            if (!File.Exists(filePath))
            {
                _cache[culture] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            try
            {
                // 1. قراءة الملف بترميز UTF8 الصريح لضمان الحروف العربي
                var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

                // 2. إعدادات ذكية للـ JSON لتخطي التعليقات ودعم الفواصل الزائدة
                var options = new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip, // هيتخطى // و /* */ لوحده
                    AllowTrailingCommas = true // لو نسيت فاصلة في آخر سطر مش هيضرب
                };

                using (var doc = JsonDocument.Parse(json, options))
                {
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    FlattenJson(string.Empty, doc.RootElement, dict);
                    _cache[culture] = dict;
                    System.Diagnostics.Debug.WriteLine($"✅ [LOCALIZATION] Loaded {dict.Count} keys for culture: {culture}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL: Localization failed for {culture}. Error: {ex.Message}");
                _cache[culture] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void FlattenJson(string prefix, JsonElement element, Dictionary<string, string> dict)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var newPrefix = string.IsNullOrEmpty(prefix) 
                            ? property.Name 
                            : $"{prefix}.{property.Name}";
                        FlattenJson(newPrefix, property.Value, dict);
                    }
                    break;
                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        var newPrefix = $"{prefix}[{index}]";
                        FlattenJson(newPrefix, item, dict);
                        index++;
                    }
                    break;
                default:
                    dict[prefix] = element.ToString();
                    break;
            }
        }
    }
}
