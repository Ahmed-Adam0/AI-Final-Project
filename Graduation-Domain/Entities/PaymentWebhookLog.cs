namespace Graduation_domain.Entities
{
    /// <summary>
    /// Audit record for every inbound Paymob webhook request.
    /// Stored regardless of processing outcome so failures can be diagnosed.
    /// </summary>
    public class PaymentWebhookLog : BaseEntity<int>
    {
        /// <summary>Payment provider name (e.g. "Paymob").</summary>
        public string Provider { get; set; } = "Paymob";

        /// <summary>Raw JSON payload received from the provider.</summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>UTC timestamp when the webhook was received by this server.</summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>True when the webhook was fully processed without errors.</summary>
        public bool ProcessedSuccessfully { get; set; }

        /// <summary>Error message if processing failed; null on success.</summary>
        public string? ErrorMessage { get; set; }
    }
}
