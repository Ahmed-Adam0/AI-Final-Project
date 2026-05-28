using System.Collections.Generic;

namespace Graduation_Application.DTOs.NotificationDTO
{
    public class WhatsAppNotificationRequest
    {
        public string To { get; set; } = string.Empty;
        public string? TemplateName { get; set; }
        public string? LanguageCode { get; set; }
        public List<string>? BodyParameters { get; set; }
    }
}
