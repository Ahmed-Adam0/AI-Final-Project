using System.Text.Json.Serialization;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobOrderRequest
    {
        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; }

        [JsonPropertyName("delivery_needed")]
        public bool DeliveryNeeded { get; set; } = false;

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EGP";

        [JsonPropertyName("merchant_order_id")]
        public string? MerchantOrderId { get; set; }
    }
}
