using System.Text;
using System.Text.Json;
using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_Domain.Enums;
using Graduation_infrastructure.AppDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymobHmacValidator _hmacValidator;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IPaymentWebhookLogRepository _webhookLogRepository;

        public PaymentsController(
            IPaymentGateway paymentGateway,
            IPaymobHmacValidator hmacValidator,
            ApplicationDbContext context,
            ILogger<PaymentsController> logger,
            IPaymentWebhookLogRepository webhookLogRepository
        )
        {
            _paymentGateway = paymentGateway;
            _hmacValidator = hmacValidator;
            _context = context;
            _logger = logger;
            _webhookLogRepository = webhookLogRepository;
        }

        [HttpPost("paymob")]
        public async Task<IActionResult> InitiatePaymobPayment(
            [FromBody] PaymobPaymentRequest request
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var paymentUrl = await _paymentGateway.CreatePaymentUrlAsync(
                    request.OrderId,
                    request.Amount,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Phone
                );

                return Ok(new PaymobPaymentResponse { PaymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Payment initiation failed for order {OrderId}",
                    request.OrderId
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("paymob/webhook")]
        public async Task<IActionResult> HandlePaymobWebhook()
        {
            // --- 1. Read raw body (must be first; body can only be read once) ---
            string rawPayload;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                rawPayload = await reader.ReadToEndAsync();
            }

            _logger.LogInformation(
                "Paymob webhook received. Payload length: {Length}",
                rawPayload.Length
            );
            _logger.LogInformation("Hmac header: {HmacHeader}", Request.Query["hmac"].ToString());

            // Outer try-catch ensures we ALWAYS return 200 to Paymob.
            // A non-200 response causes Paymob to retry the webhook indefinitely.
            try
            {
                // --- 2. Deserialize payload ---
                PaymobWebhookPayload payload;
                try
                {
                    payload =
                        JsonSerializer.Deserialize<PaymobWebhookPayload>(rawPayload)
                        ?? throw new InvalidOperationException(
                            "Deserializer returned null for webhook payload"
                        );
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Invalid webhook payload format — could not deserialize JSON"
                    );
                    await WriteWebhookLogAsync(
                        rawPayload,
                        success: false,
                        error: $"JSON deserialization failed: {ex.Message}"
                    );
                    return Ok(new { Message = "Received" });
                }

                // --- 3. Guard: payload must contain a transaction object ---
                if (payload.Obj == null)
                {
                    _logger.LogWarning(
                        "Webhook received without a transaction object (obj is null)"
                    );
                    await WriteWebhookLogAsync(
                        rawPayload,
                        success: false,
                        error: "Missing transaction object (obj)"
                    );
                    return Ok(new { Message = "Received" });
                }

                var paymobTransactionId = payload.Obj.Id;
                var paymobOrderId = payload.Obj.Order?.Id;

                // --- 4. HMAC validation ---
                var hmacHeader = Request.Query["hmac"].ToString();
                //var hmacPrefix = hmacHeader.Length > 8 ? hmacHeader[..8] + "…" : "(empty)";
                //_logger.LogInformation("HMAC header prefix: {HmacPrefix}", hmacPrefix);
                //;
                if (!_hmacValidator.Validate(hmacHeader, rawPayload))
                {
                    _logger.LogWarning(
                        "HMAC validation FAILED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}",
                        paymobTransactionId,
                        paymobOrderId
                    );
                    // Return 200 intentionally: returning non-200 causes Paymob to retry forever.
                    await WriteWebhookLogAsync(
                        rawPayload,
                        success: false,
                        error: "HMAC signature mismatch"
                    );
                    return Ok(new { Message = "Received" });
                }

                _logger.LogInformation(
                    "HMAC validation PASSED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}",
                    paymobTransactionId,
                    paymobOrderId
                );

                // --- 5. Look up the local PaymentTransaction ---
                var transaction = await _context.PaymentTransactions.FirstOrDefaultAsync(t =>
                    t.PaymobOrderId == paymobOrderId
                );

                if (transaction == null)
                {
                    _logger.LogWarning(
                        "No PaymentTransaction found for PaymobOrderId: {PaymobOrderId} (TransactionId: {TransactionId})",
                        paymobOrderId,
                        paymobTransactionId
                    );
                    await WriteWebhookLogAsync(
                        rawPayload,
                        success: false,
                        error: $"No PaymentTransaction found for PaymobOrderId={paymobOrderId}"
                    );
                    return Ok(new { Message = "Received" });
                }

                // --- 6. Idempotency guard ---
                // If this transaction was already successfully processed, skip silently.
                if (transaction.Status == PaymentStatus.Paid)
                {
                    _logger.LogInformation(
                        "Idempotency check: PaymentTransaction {TransactionDbId} (PaymobOrderId: {PaymobOrderId}) "
                            + "is already Paid. Skipping duplicate webhook.",
                        transaction.Id,
                        paymobOrderId
                    );
                    await WriteWebhookLogAsync(
                        rawPayload,
                        success: true,
                        error: "Skipped: transaction already in Paid state (idempotency)"
                    );
                    return Ok(new { Message = "Received" });
                }

                // --- 7. Load the related Order in the same round-trip scope ---
                var order = await _context.Orders.FirstOrDefaultAsync(o =>
                    o.Id == transaction.LocalOrderId
                );

                if (order == null)
                {
                    _logger.LogWarning(
                        "Order {LocalOrderId} not found for PaymentTransaction {TransactionDbId}",
                        transaction.LocalOrderId,
                        transaction.Id
                    );
                    // Continue processing the payment transaction even if the order is not found.
                    // This prevents losing payment state due to an unexpected data inconsistency.
                }

                // --- 8. Apply payment outcome ---
                if (payload.Obj.Success)
                {
                    await ProcessPaymentAsync(
                        transaction,
                        order,
                        true,
                        paymobTransactionId.ToString()
                    );

                    _logger.LogInformation(
                        "Payment SUCCEEDED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}, LocalOrderId: {LocalOrderId}",
                        paymobTransactionId,
                        paymobOrderId,
                        transaction.LocalOrderId
                    );
                }
                else if (payload.Obj.IsVoided)
                {
                    transaction.Status = PaymentStatus.Cancelled;
                    transaction.TransactionId = paymobTransactionId.ToString();
                    transaction.FailureReason = "Transaction voided";

                    if (order != null)
                    {
                        order.Status = OrderStatus.Cancelled.ToString();
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Payment VOIDED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}, LocalOrderId: {LocalOrderId}",
                        paymobTransactionId,
                        paymobOrderId,
                        transaction.LocalOrderId
                    );
                }
                else
                {
                    await ProcessPaymentAsync(
                        transaction,
                        order,
                        false,
                        paymobTransactionId.ToString(),
                        payload.Obj.Data?.ContainsKey("error") == true
                            ? payload.Obj.Data["error"]?.ToString()
                            : "Payment failed"
                    );

                    _logger.LogInformation(
                        "Payment FAILED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}, LocalOrderId: {LocalOrderId}",
                        paymobTransactionId,
                        paymobOrderId,
                        transaction.LocalOrderId
                    );
                }

                // --- 9. Persist all changes in a single SaveChanges call ---
                // PaymentTransaction + Order (if loaded) are already tracked by the DbContext.
                // We add the webhook log to the same unit of work and save once.
                var webhookLog = BuildWebhookLog(rawPayload, success: true, error: null);
                await _webhookLogRepository.AddAsync(webhookLog);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing Paymob webhook");

                // Attempt to persist an error audit log.
                // Use a separate try-catch so a DB failure here does not prevent the 200 response.
                try
                {
                    await WriteWebhookLogAsync(rawPayload, success: false, error: ex.Message);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to persist webhook error log after exception");
                }

                // Always return 200 to Paymob — even on internal errors.
                return Ok(new { Message = "Received" });
            }
        }

        [HttpGet("paymob/callback")]
        public async Task<IActionResult> Callback()
        {
            try
            {
                _logger.LogInformation("Callback hit");

                var success = bool.Parse(Request.Query["success"]);

                var localOrderId = int.Parse(Request.Query["merchant_order_id"]);

                _logger.LogInformation(
                    "OrderId={OrderId}, Success={Success}",
                    localOrderId,
                    success
                );

                var transaction = await _context.PaymentTransactions.FirstOrDefaultAsync(x =>
                    x.LocalOrderId == localOrderId
                );

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction not found for order {OrderId}", localOrderId);

                    return Ok(new { Message = "Transaction not found" });
                }

                transaction.Status = success ? PaymentStatus.Paid : PaymentStatus.Failed;

                transaction.TransactionId = Request.Query["id"];

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback Error");

                return StatusCode(
                    500,
                    new { Error = ex.Message, InnerError = ex.InnerException?.Message }
                );
            }
        }

        private async Task ProcessPaymentAsync(
            PaymentTransaction transaction,
            Order? order,
            bool success,
            string transactionId,
            string? failureReason = null
        )
        {
            _logger.LogInformation(
                "ProcessPaymentAsync called => Success={Success}, TransactionId={TransactionId}",
                success,
                transactionId
            );
            transaction.TransactionId = transactionId;

            if (success)
            {
                transaction.Status = PaymentStatus.Paid;
                transaction.PaidAt = DateTime.UtcNow;
                transaction.FailureReason = null;

                if (order != null)
                {
                    order.Status = OrderStatus.Confirmed.ToString();
                }
            }
            else
            {
                transaction.Status = PaymentStatus.Failed;
                transaction.FailureReason = failureReason ?? "Payment failed";

                if (order != null)
                {
                    order.Status = OrderStatus.Cancelled.ToString();
                }
            }

            await _context.SaveChangesAsync();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Builds a <see cref="PaymentWebhookLog"/> record and persists it immediately
        /// via its own repository + SaveChanges. Use this helper for early-exit paths
        /// where the main SaveChanges has not been called yet.
        /// </summary>
        private async Task WriteWebhookLogAsync(string rawPayload, bool success, string? error)
        {
            var log = BuildWebhookLog(rawPayload, success, error);
            await _webhookLogRepository.AddAsync(log);
            await _webhookLogRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Creates an unsaved <see cref="PaymentWebhookLog"/> instance.
        /// Use this when you want to include the log in the same SaveChanges call
        /// as other entities (e.g. the happy-path at the end of the webhook handler).
        /// </summary>
        private static PaymentWebhookLog BuildWebhookLog(
            string rawPayload,
            bool success,
            string? error
        )
        {
            return new PaymentWebhookLog
            {
                Provider = "Paymob",
                Payload = rawPayload,
                ReceivedAt = DateTime.UtcNow,
                ProcessedSuccessfully = success,
                ErrorMessage = error,
            };
        }
    }
}
