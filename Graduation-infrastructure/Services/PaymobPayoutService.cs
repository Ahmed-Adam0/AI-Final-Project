using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Graduation_infrastructure.Services
{
    /// <summary>
    /// Concrete implementation of <see cref="IPayoutGateway"/> using Paymob Wallet Payout API.
    ///
    /// CONFIGURATION (appsettings.json):
    /// <code>
    /// "Paymob": {
    ///   "WalletPayoutApiKey": "YOUR_PAYOUT_API_KEY",
    ///   "WalletPayoutIntegrationId": 12345
    /// }
    /// </code>
    ///
    /// TODO: Replace the placeholder body below with the actual Paymob Wallet Payout API call
    ///       once credentials and endpoint details are confirmed with the Paymob team.
    ///       Reference: https://docs.paymob.com/docs/wallet-payout
    /// </summary>
    public class PaymobPayoutService : IPayoutGateway
    {
        private readonly PaymobSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PaymobPayoutService> _logger;

        private const string PaymobBaseUrl = "https://accept.paymob.com/api";

        public PaymobPayoutService(
            IOptions<PaymobSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<PaymobPayoutService> logger)
        {
            _settings          = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger            = logger;
        }

        /// <inheritdoc/>
        public async Task<string> SendPayoutAsync(decimal amount, string walletNumber)
        {
            // ── Guard: credentials not yet configured ──────────────────────────
            if (string.IsNullOrWhiteSpace(_settings.WalletPayoutApiKey) ||
                _settings.WalletPayoutIntegrationId == null)
            {
                _logger.LogWarning(
                    "Paymob Wallet Payout credentials are not configured. " +
                    "Set Paymob:WalletPayoutApiKey and Paymob:WalletPayoutIntegrationId in appsettings.json. " +
                    "Running in SIMULATED MOCK MODE for local testing. " +
                    "Amount={Amount}, WalletNumber={WalletNumber}", amount, walletNumber);

                var mockTransactionRef = $"SIMULATED-PAYOUT-{Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}";
                return mockTransactionRef;
            }

            // ── TODO: Replace with real Paymob Wallet Payout API call ─────────
            //
            // Step 1 — Authenticate
            //   POST https://accept.paymob.com/api/auth/tokens
            //   Body: { "api_key": _settings.WalletPayoutApiKey }
            //   → returns auth_token
            //
            // Step 2 — Initiate payout
            //   POST https://accept.paymob.com/api/acceptance/payments/pay
            //   Headers: Authorization: Bearer {auth_token}
            //   Body: {
            //     "source": { "identifier": walletNumber, "subtype": "WALLET" },
            //     "payment_token": "<payment_key>",   // generated via payment keys endpoint
            //     "integration_id": _settings.WalletPayoutIntegrationId,
            //     "amount_cents": (long)(amount * 100)
            //   }
            //   → returns { "id": "txn_ref_...", "success": true }
            //
            // Throw on failure so VendorWalletService marks withdrawal as Failed.
            // ────────────────────────────────────────────────────────────────────

            _logger.LogInformation(
                "Paymob Wallet Payout initiated (placeholder): Amount={Amount}, WalletNumber={WalletNumber}",
                amount, walletNumber);

            // Placeholder: simulate a successful payout with a dummy reference.
            // REPLACE THIS BLOCK with the real HTTP calls described above.
            await Task.Delay(0); // Remove when real async call is added.
            var dummyTransactionRef = $"PAYOUT-PLACEHOLDER-{Guid.NewGuid():N}";
            _logger.LogWarning(
                "PaymobPayoutService is running in PLACEHOLDER mode. " +
                "No real payout was sent. Ref={Ref}", dummyTransactionRef);

            return dummyTransactionRef;
        }
    }
}
