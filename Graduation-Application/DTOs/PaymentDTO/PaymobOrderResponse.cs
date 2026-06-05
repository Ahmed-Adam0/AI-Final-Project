using System.Text.Json.Serialization;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobOrderResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("delivery_needed")]
        public bool DeliveryNeeded { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("is_payment_throttled")]
        public bool IsPaymentThrottled { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string? MerchantOrderId { get; set; }

        [JsonPropertyName("merchant_id")]
        public int MerchantId { get; set; }

        [JsonPropertyName("paid_amount_cents")]
        public long PaidAmountCents { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("items")]
        public List<object> Items { get; set; } = new();
    }
}
