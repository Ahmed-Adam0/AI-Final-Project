using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Graduation_Application.DTOs.NotificationDTO;
using Graduation_Application.IServices;
using Graduation_Application.Options;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Graduation_Application.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WhatsAppNotificationSettings _settings;

        public WhatsAppService(
            IHttpClientFactory httpClientFactory,
            IOptions<WhatsAppNotificationSettings> settings
        )
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public async Task SendTemplateAsync(WhatsAppNotificationRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (string.IsNullOrWhiteSpace(request.To))
                {
                    throw new ArgumentException(
                        "WhatsApp recipient number is required.",
                        nameof(request)
                    );
                }

                if (string.IsNullOrWhiteSpace(_settings.PhoneNumberId))
                {
                    throw new InvalidOperationException(
                        "WhatsApp PhoneNumberId is not configured."
                    );
                }

                if (string.IsNullOrWhiteSpace(_settings.AccessToken))
                {
                    throw new InvalidOperationException("WhatsApp AccessToken is not configured.");
                }

                var templateName = string.IsNullOrWhiteSpace(request.TemplateName)
                    ? _settings.DefaultTemplateName
                    : request.TemplateName;

                if (string.IsNullOrWhiteSpace(templateName))
                {
                    throw new InvalidOperationException("WhatsApp template name is required.");
                }

                var languageCode = string.IsNullOrWhiteSpace(request.LanguageCode)
                    ? _settings.DefaultLanguageCode
                    : request.LanguageCode;

                var recipient = NormalizePhoneNumber(request.To);
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    _settings.AccessToken
                );

                var url =
                    $"{TrimTrailingSlash(_settings.GraphApiBaseUrl)}/{_settings.ApiVersion}/{_settings.PhoneNumberId}/messages";

                var response = await PostTemplateAsync(
                    client,
                    url,
                    recipient,
                    templateName,
                    languageCode,
                    request.BodyParameters
                );

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    if (
                        response.StatusCode == System.Net.HttpStatusCode.BadRequest
                        && responseBody.Contains("132000", StringComparison.OrdinalIgnoreCase)
                        && request.BodyParameters is { Count: > 0 }
                    )
                    {
                        response.Dispose();
                        response = await PostTemplateAsync(
                            client,
                            url,
                            recipient,
                            templateName,
                            languageCode,
                            null
                        );
                        responseBody = await response.Content.ReadAsStringAsync();
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.Error.WriteLine(
                            $"WhatsApp notification failed with status {(int)response.StatusCode}: {responseBody}"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"WhatsApp notification error: {ex.Message}");
            }
        }

        private Task<HttpResponseMessage> PostTemplateAsync(
            HttpClient client,
            string url,
            string recipient,
            string templateName,
            string languageCode,
            IReadOnlyList<string>? bodyParameters
        )
        {
            var payload = new Dictionary<string, object?>
            {
                ["messaging_product"] = "whatsapp",
                ["to"] = recipient,
                ["type"] = "template",
                ["template"] = BuildTemplate(templateName, languageCode, bodyParameters),
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json"
            );

            return client.PostAsync(url, content);
        }

        private static object BuildTemplate(
            string templateName,
            string languageCode,
            IReadOnlyList<string>? bodyParameters
        )
        {
            var template = new Dictionary<string, object?>
            {
                ["name"] = templateName,
                ["language"] = new Dictionary<string, string> { ["code"] = languageCode },
            };

            if (bodyParameters is { Count: > 0 })
            {
                template["components"] = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = bodyParameters
                            .Select(value => new { type = "text", text = value })
                            .ToArray(),
                    },
                };
            }

            return template;
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }

        private static string TrimTrailingSlash(string value)
        {
            return value.TrimEnd('/');
        }
    }
}
