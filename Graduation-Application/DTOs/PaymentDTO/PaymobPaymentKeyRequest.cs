using System.Text.Json.Serialization;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobPaymentKeyRequest
    {
        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("expiration")]
        public int Expiration { get; set; } = 3600;

        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }

        [JsonPropertyName("billing_data")]
        public PaymobBillingData BillingData { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EGP";

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }
    }
}
