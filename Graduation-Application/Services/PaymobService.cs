using System.Net.Http.Json;
using System.Text.Json;
using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Graduation_Application.Services
{
    public class PaymobService : IPaymentGateway
    {
        private readonly PaymobSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly ILogger<PaymobService> _logger;
        private const string BaseUrl = "https://accept.paymob.com/api";

        public PaymobService(
            IOptions<PaymobSettings> settings,
            IHttpClientFactory httpClientFactory,
            IPaymentTransactionRepository paymentTransactionRepository,
            ILogger<PaymobService> logger
        )
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _paymentTransactionRepository = paymentTransactionRepository;
            _logger = logger;
        }

        public async Task<PaymentTransactionResult> CreatePaymentUrlAsync(
            int orderId,
            decimal amount,
            string firstName,
            string lastName,
            string email,
            string phone
        )
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                _logger.LogInformation("Starting Paymob payment flow for order {OrderId}", orderId);

                var authToken = await GetAuthTokenAsync(httpClient);
                var paymobOrderId = await CreateOrderAsync(httpClient, authToken, amount, orderId);
                var paymentToken = await GeneratePaymentKeyAsync(
                    httpClient,
                    authToken,
                    paymobOrderId,
                    amount,
                    firstName,
                    lastName,
                    email,
                    phone
                );

                var paymentTransaction = new PaymentTransaction
                {
                    LocalOrderId = orderId,
                    PaymobOrderId = paymobOrderId,
                    PaymentToken = paymentToken,
                    Status = PaymentStatus.Unpaid,
                    Amount = amount,
                    Currency = "EGP",
                };

                await _paymentTransactionRepository.AddAsync(paymentTransaction);
                await _paymentTransactionRepository.SaveChangesAsync();

                var paymentUrl =
                    $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentToken}";

                _logger.LogInformation(
                    "Payment URL generated for order {OrderId}: {PaymentUrl}",
                    orderId,
                    paymentUrl
                );

                return new PaymentTransactionResult
                {
                    PaymentUrl = paymentUrl,
                    Transaction = paymentTransaction
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment URL for order {OrderId}", orderId);
                throw;
            }
        }

        private async Task<string> GetAuthTokenAsync(HttpClient httpClient)
        {
            _logger.LogInformation("Requesting Paymob authentication token");

            var request = new PaymobAuthRequest { ApiKey = _settings.ApiKey };
            var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/auth/tokens", request);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<PaymobAuthResponse>()
                ?? throw new InvalidOperationException(
                    "Failed to deserialize Paymob auth response"
                );

            _logger.LogInformation("Paymob authentication token received");

            return result.Token;
        }

        private async Task<int> CreateOrderAsync(
            HttpClient httpClient,
            string authToken,
            decimal amount,
            int orderId
        )
        {
            _logger.LogInformation("Creating Paymob order for amount {Amount}", amount);

            var request = new PaymobOrderRequest
            {
                AuthToken = authToken,
                DeliveryNeeded = false,
                AmountCents = (long)(amount * 100),
                Currency = "EGP",
                MerchantOrderId = $"{orderId}_{DateTime.UtcNow.Ticks}",
            };

            var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/ecommerce/orders", request);
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response: {Response}", responseBody);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<PaymobOrderResponse>()
                ?? throw new InvalidOperationException(
                    "Failed to deserialize Paymob order response"
                );

            _logger.LogInformation("Paymob order created with ID {OrderId}", result.Id);

            return result.Id;
        }

        private async Task<string> GeneratePaymentKeyAsync(
            HttpClient httpClient,
            string authToken,
            int paymobOrderId,
            decimal amount,
            string firstName,
            string lastName,
            string email,
            string phone
        )
        {
            _logger.LogInformation(
                "Generating Paymob payment key for order {OrderId}",
                paymobOrderId
            );

            var request = new PaymobPaymentKeyRequest
            {
                AuthToken = authToken,
                AmountCents = (long)(amount * 100),
                Expiration = 3600,
                OrderId = paymobOrderId,
                BillingData = new PaymobBillingData
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PhoneNumber = phone,
                    City = "Cairo",
                    Country = "EG",
                    State = "Cairo",
                    Street = "NA",
                    Building = "NA",
                    Floor = "NA",
                    Apartment = "NA",
                    PostalCode = "00000",
                    ShippingMethod = "NA",
                },
                Currency = "EGP",
                IntegrationId = _settings.CardIntegrationId,
                //IntegrationId = _settings.WalletIntegrationId,
            };

            var response = await httpClient.PostAsJsonAsync(
                $"{BaseUrl}/acceptance/payment_keys",
                request
            );
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response: {Response}", responseBody);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>()
                ?? throw new InvalidOperationException(
                    "Failed to deserialize Paymob payment key response"
                );

            _logger.LogInformation("Paymob payment key generated successfully");

            return result.Token;
        }
    }
}
