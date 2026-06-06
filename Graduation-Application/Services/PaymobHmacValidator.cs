using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Graduation_Application.Services
{
    public class PaymobHmacValidator : IPaymobHmacValidator
    {
        private readonly PaymobSettings _settings;
        private readonly ILogger<PaymobHmacValidator> _logger;

        public PaymobHmacValidator(
            IOptions<PaymobSettings> settings,
            ILogger<PaymobHmacValidator> logger
        )
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public bool Validate(string hmacHeader, string payload)
        {
            if (string.IsNullOrWhiteSpace(hmacHeader) || string.IsNullOrWhiteSpace(payload))
            {
                _logger.LogWarning("HMAC validation failed: empty header or payload");
                return false;
            }

            try
            {
                var computedHash = ComputeHmac(payload);
                var isValid = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(computedHash),
                    Encoding.UTF8.GetBytes(hmacHeader)
                );

                _logger.LogWarning("hmac: {HmacHeader}", hmacHeader);
                _logger.LogWarning("computed: {ComputedHash}", computedHash);

                if (!isValid)
                {
                    _logger.LogWarning("HMAC validation failed: signature mismatch");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HMAC validation failed with exception");
                return false;
            }
        }

        private string ComputeHmac(string rawPayload)
        {
            var root = JsonSerializer.Deserialize<JsonElement>(rawPayload);
            var obj = root.GetProperty("obj");

            static string Bool(JsonElement el) => el.GetBoolean().ToString().ToLowerInvariant();

            var data = string.Concat(
                obj.GetProperty("amount_cents").GetInt64(),
                obj.GetProperty("created_at").GetString(),
                obj.GetProperty("currency").GetString(),
                Bool(obj.GetProperty("error_occured")),
                Bool(obj.GetProperty("has_parent_transaction")),
                obj.GetProperty("id").GetInt64(),
                obj.GetProperty("integration_id").GetInt64(),
                Bool(obj.GetProperty("is_3d_secure")),
                Bool(obj.GetProperty("is_auth")),
                Bool(obj.GetProperty("is_capture")),
                Bool(obj.GetProperty("is_refunded")),
                Bool(obj.GetProperty("is_standalone_payment")),
                Bool(obj.GetProperty("is_voided")),
                obj.GetProperty("order").GetProperty("id").GetInt64(),
                obj.GetProperty("owner").GetInt64(),
                Bool(obj.GetProperty("pending")),
                obj.GetProperty("source_data").GetProperty("pan").GetString(),
                obj.GetProperty("source_data").GetProperty("sub_type").GetString(),
                obj.GetProperty("source_data").GetProperty("type").GetString(),
                Bool(obj.GetProperty("success"))
            );

            var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
