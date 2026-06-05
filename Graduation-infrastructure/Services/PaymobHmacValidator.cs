using System.Security.Cryptography;
using System.Text;
using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Graduation_infrastructure.Services
{
    public class PaymobHmacValidator : IPaymobHmacValidator
    {
        private readonly PaymobSettings _settings;
        private readonly ILogger<PaymobHmacValidator> _logger;

        public PaymobHmacValidator(IOptions<PaymobSettings> settings, ILogger<PaymobHmacValidator> logger)
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
                    Encoding.UTF8.GetBytes(hmacHeader));

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

        private string ComputeHmac(string payload)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(payloadBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
