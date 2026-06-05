using System.Text.Json.Serialization;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobAuthRequest
    {
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }
    }
}
