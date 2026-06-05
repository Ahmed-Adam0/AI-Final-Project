using System.Text;
using System.Text.Json;
using Graduation_Application.DTOs.PaymentDTO;
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

        public PaymentsController(
            IPaymentGateway paymentGateway,
            IPaymobHmacValidator hmacValidator,
            ApplicationDbContext context,
            ILogger<PaymentsController> logger
        )
        {
            _paymentGateway = paymentGateway;
            _hmacValidator = hmacValidator;
            _context = context;
            _logger = logger;
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
            string rawPayload;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                rawPayload = await reader.ReadToEndAsync();
            }

            _logger.LogInformation("Paymob webhook received: {Payload}", rawPayload);

            try
            {
                PaymobWebhookPayload payload;
                try
                {
                    payload =
                        JsonSerializer.Deserialize<PaymobWebhookPayload>(rawPayload)
                        ?? throw new InvalidOperationException(
                            "Failed to deserialize webhook payload"
                        );
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid webhook payload format");
                    return Ok(new { Message = "Received" });
                }

                if (payload.Obj == null)
                {
                    _logger.LogWarning("Webhook received without transaction object");
                    return Ok(new { Message = "Received" });
                }

                var hmacHeader = payload.Hmac;
                if (!_hmacValidator.Validate(hmacHeader, rawPayload))
                {
                    _logger.LogWarning(
                        "HMAC validation failed for transaction {TransactionId}",
                        payload.Obj.Id
                    );
                    return Unauthorized(new { Message = "Invalid HMAC signature" });
                }

                _logger.LogInformation(
                    "HMAC validated successfully for transaction {TransactionId}",
                    payload.Obj.Id
                );

                var paymobOrderId = payload.Obj.Order?.Id;
                var transaction = await _context.PaymentTransactions.FirstOrDefaultAsync(t =>
                    t.PaymobOrderId == paymobOrderId
                );

                if (transaction == null)
                {
                    _logger.LogWarning(
                        "No payment transaction found for Paymob order {OrderId}",
                        payload.Obj.Order?.Id
                    );
                    return Ok(new { Message = "Received" });
                }

                if (payload.Obj.Success)
                {
                    transaction.Status = PaymentStatus.Paid;
                    transaction.TransactionId = payload.Obj.Id.ToString();
                    transaction.PaidAt = DateTime.UtcNow;
                    _logger.LogInformation(
                        "Payment succeeded for transaction {TransactionId}",
                        payload.Obj.Id
                    );
                }
                else if (payload.Obj.IsVoided)
                {
                    transaction.Status = PaymentStatus.Cancelled;
                    transaction.FailureReason = "Transaction voided";
                    _logger.LogInformation(
                        "Payment cancelled for transaction {TransactionId}",
                        payload.Obj.Id
                    );
                }
                else
                {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.FailureReason =
                        payload.Obj.Data?.ContainsKey("error") == true
                            ? payload.Obj.Data["error"]?.ToString()
                            : "Payment failed";
                    _logger.LogInformation(
                        "Payment failed for transaction {TransactionId}",
                        payload.Obj.Id
                    );
                }

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Paymob webhook");
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
    }
}
