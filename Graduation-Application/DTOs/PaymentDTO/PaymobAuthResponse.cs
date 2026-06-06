using System.Text.Json.Serialization;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobAuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
