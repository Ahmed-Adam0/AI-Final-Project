namespace Graduation_Application.Constants
{
    /// <summary>
    /// Domain-level constants for supported user language preferences.
    /// All language validation across the application must reference this class.
    /// To add a new language, add it here and it will be picked up everywhere.
    /// </summary>
    public static class LanguageConstants
    {
        public const string Arabic = "ar";
        public const string English = "en";
        public const string Default = English;

        public static readonly IReadOnlySet<string> AllowedLanguages =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Arabic,
                English
            };

        public static bool IsAllowed(string language) =>
            !string.IsNullOrWhiteSpace(language) && AllowedLanguages.Contains(language);
    }
}
