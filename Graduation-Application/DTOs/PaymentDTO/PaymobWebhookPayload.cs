using System.Text.Json.Serialization;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobWebhookPayload
    {
        [JsonPropertyName("obj")]
        public PaymobTransactionObject Obj { get; set; }

        [JsonPropertyName("hmac")]
        public string Hmac { get; set; }
    }

    public class PaymobTransactionObject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool Is3dSecure { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("owner")]
        public int Owner { get; set; }

        [JsonPropertyName("order")]
        public PaymobWebhookOrder Order { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobSourceData SourceData { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, object>? Data { get; set; }
    }

    public class PaymobWebhookOrder
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class PaymobSourceData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("pan")]
        public string Pan { get; set; }

        [JsonPropertyName("sub_type")]
        public string SubType { get; set; }
    }
}
