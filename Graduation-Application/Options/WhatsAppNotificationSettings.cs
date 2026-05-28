namespace Graduation_Application.Options
{
    public class WhatsAppNotificationSettings
    {
        public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com";
        public string ApiVersion { get; set; } = "v25.0";
        public string PhoneNumberId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string DefaultTemplateName { get; set; } = string.Empty;
        public string DefaultLanguageCode { get; set; } = "ar";
    }
}
