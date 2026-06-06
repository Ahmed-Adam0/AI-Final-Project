//using Graduation_Application.DTOs.PaymentDTO;
//using Graduation_Application.IServices;
//using Graduation_domain.Entities;
//using Graduation_Domain.Enums;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Graduation_Application.Services
//{
//    public class PaymobWebhookService : IPaymobWebhookService
//    {
//        private readonly AppDbContext _context;
//        private readonly IPaymobHmacValidator _hmacValidator;
//        private readonly IWebhookLogRepository _webhookLogRepository;
//        private readonly ILogger<PaymobWebhookService> _logger;

//        public PaymobWebhookService(
//            AppDbContext context,
//            IPaymobHmacValidator hmacValidator,
//            IWebhookLogRepository webhookLogRepository,
//            ILogger<PaymobWebhookService> logger)
//        {
//            _context = context;
//            _hmacValidator = hmacValidator;
//            _webhookLogRepository = webhookLogRepository;
//            _logger = logger;
//        }

//        public async Task ProcessAsync(string rawPayload, string hmacHeader)
//        {
//            _logger.LogInformation(
//                "Paymob webhook received. Payload length: {Length}", rawPayload.Length);

//            // Log only a short prefix — never the full HMAC value (security risk)
//            var hmacPrefix = hmacHeader.Length > 8 ? hmacHeader[..8] + "…" : "(empty)";
//            _logger.LogInformation("HMAC header prefix: {HmacPrefix}", hmacPrefix);

//            try
//            {
//                // --- 1. Deserialize payload ---
//                PaymobWebhookPayload payload;
//                try
//                {
//                    payload = JsonSerializer.Deserialize<PaymobWebhookPayload>(rawPayload)
//                        ?? throw new InvalidOperationException(
//                            "Deserializer returned null for webhook payload");
//                }
//                catch (JsonException ex)
//                {
//                    _logger.LogError(ex, "Invalid webhook payload — JSON deserialization failed");
//                    await PersistLogAsync(rawPayload, success: false,
//                        error: $"JSON deserialization failed: {ex.Message}");
//                    return;
//                }

//                // --- 2. Guard: payload must contain a transaction object ---
//                if (payload.Obj == null)
//                {
//                    _logger.LogWarning("Webhook received without a transaction object (obj is null)");
//                    await PersistLogAsync(rawPayload, success: false,
//                        error: "Missing transaction object (obj)");
//                    return;
//                }

//                var paymobTransactionId = payload.Obj.Id;
//                var paymobOrderId = payload.Obj.Order?.Id;

//                // --- 3. HMAC validation (before any DB access) ---
//                if (!_hmacValidator.Validate(hmacHeader, rawPayload))
//                {
//                    _logger.LogWarning(
//                        "HMAC validation FAILED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}",
//                        paymobTransactionId, paymobOrderId);
//                    await PersistLogAsync(rawPayload, success: false,
//                        error: "HMAC signature mismatch");
//                    return;
//                }

//                _logger.LogInformation(
//                    "HMAC validation PASSED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}",
//                    paymobTransactionId, paymobOrderId);

//                // --- 4. Look up the local PaymentTransaction ---
//                var transaction = await _context.PaymentTransactions
//                    .FirstOrDefaultAsync(t => t.PaymobOrderId == paymobOrderId);

//                if (transaction == null)
//                {
//                    _logger.LogWarning(
//                        "No PaymentTransaction found for PaymobOrderId: {PaymobOrderId} (TransactionId: {TransactionId})",
//                        paymobOrderId, paymobTransactionId);
//                    await PersistLogAsync(rawPayload, success: false,
//                        error: $"No PaymentTransaction found for PaymobOrderId={paymobOrderId}");
//                    return;
//                }

//                // --- 5. Idempotency guard — skip if already successfully processed ---
//                if (transaction.Status == PaymentStatus.Paid)
//                {
//                    _logger.LogInformation(
//                        "Idempotency: Transaction {TransactionDbId} (PaymobOrderId: {PaymobOrderId}) already Paid — skipping duplicate webhook.",
//                        transaction.Id, paymobOrderId);
//                    await PersistLogAsync(rawPayload, success: true,
//                        error: "Skipped: transaction already Paid (idempotency)");
//                    return;
//                }

//                // --- 6. Load related Order (warn if missing — do not abort) ---
//                var order = await _context.Orders
//                    .FirstOrDefaultAsync(o => o.Id == transaction.LocalOrderId);

//                if (order == null)
//                {
//                    _logger.LogWarning(
//                        "Order {LocalOrderId} not found for PaymentTransaction {TransactionDbId} — continuing without order update.",
//                        transaction.LocalOrderId, transaction.Id);
//                }

//                // --- 7. Apply payment outcome ---
//                ApplyPaymentOutcome(payload.Obj, transaction, order, paymobTransactionId);

//                _logger.LogInformation(
//                    "Payment outcome applied — Success: {Success}, Voided: {Voided}, TransactionId: {TransactionId}, LocalOrderId: {LocalOrderId}",
//                    payload.Obj.Success, payload.Obj.IsVoided, paymobTransactionId, transaction.LocalOrderId);

//                // --- 8. Persist all changes in a single unit of work ---
//                var webhookLog = BuildWebhookLog(rawPayload, success: true, error: null);
//                await _webhookLogRepository.AddAsync(webhookLog);
//                await _context.SaveChangesAsync();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unhandled exception while processing Paymob webhook");

//                try
//                {
//                    await PersistLogAsync(rawPayload, success: false, error: ex.Message);
//                }
//                catch (Exception logEx)
//                {
//                    _logger.LogError(logEx, "Failed to persist webhook error log after exception");
//                }
//            }
//        }

//        private void ApplyPaymentOutcome(
//            PaymobTransactionObj obj,
//            PaymentTransaction transaction,
//            Order? order,
//            long paymobTransactionId)
//        {
//            if (obj.Success)
//            {
//                transaction.Status = PaymentStatus.Paid;
//                transaction.TransactionId = paymobTransactionId.ToString();
//                transaction.PaidAt = DateTime.UtcNow;

//                if (order != null)
//                    order.Status = OrderStatus.Confirmed.ToString();
//            }
//            else if (obj.IsVoided)
//            {
//                transaction.Status = PaymentStatus.Cancelled;
//                transaction.TransactionId = paymobTransactionId.ToString();
//                transaction.FailureReason = "Transaction voided";

//                if (order != null)
//                    order.Status = OrderStatus.Cancelled.ToString();
//            }
//            else
//            {
//                var errorMessage = obj.Data?.ContainsKey("error") == true
//                    ? obj.Data["error"]?.ToString()
//                    : "Payment failed";

//                transaction.Status = PaymentStatus.Failed;
//                transaction.TransactionId = paymobTransactionId.ToString();
//                transaction.FailureReason = errorMessage;

//                if (order != null)
//                    order.Status = OrderStatus.PaymentFailed.ToString();
//            }
//        }

//        private async Task PersistLogAsync(string rawPayload, bool success, string? error)
//        {
//            try
//            {
//                var log = BuildWebhookLog(rawPayload, success, error);
//                await _webhookLogRepository.AddAsync(log);
//                await _webhookLogRepository.SaveChangesAsync();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to persist webhook log entry");
//            }
//        }

//        private static WebhookLog BuildWebhookLog(string rawPayload, bool success, string? error) =>
//            new()
//            {
//                RawPayload = rawPayload,
//                Success = success,
//                Error = error,
//                ReceivedAt = DateTime.UtcNow
//            };
//    }
//}
