using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Graduation_Application.DTOs.PaymentDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Graduation_Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymobHmacValidator _hmacValidator;
        private readonly IPaymentWebhookLogRepository _webhookLogRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IGenaricRepositories<VendorOrder> _vendorOrderRepository;
        private readonly IPaymentService _paymentService;
        private readonly IGenaricRepositories<PaymentMilestone> _milestoneRepository;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentGateway paymentGateway,
            IPaymobHmacValidator hmacValidator,
            IPaymentWebhookLogRepository webhookLogRepository,
            IOrderRepository orderRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IGenaricRepositories<VendorOrder> vendorOrderRepository,
            IPaymentService paymentService,
            IGenaricRepositories<PaymentMilestone> milestoneRepository,
            IGenaricRepositories<Workshop> workshopRepository,
            ILocalizationService localizationService,
            ILogger<PaymentsController> logger
        )
        {
            _paymentGateway = paymentGateway;
            _hmacValidator = hmacValidator;
            _webhookLogRepository = webhookLogRepository;
            _orderRepository = orderRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _vendorOrderRepository = vendorOrderRepository;
            _paymentService = paymentService;
            _milestoneRepository = milestoneRepository;
            _workshopRepository = workshopRepository;
            _localizationService = localizationService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("paymob/initiate-masterorder")]
        public async Task<IActionResult> InitiateMasterOrderPayment(
            [FromBody] PaymobMasterOrderPaymentRequest request
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();

                var order = await _orderRepository.GetByIdAsync(request.MasterOrderId);
                if (order == null)
                    return NotFound(new { Message = "Order not found" });

                if (order.UserId != userId)
                    return Forbid();

                var eligibleStatuses = new[]
                {
                    VendorOrderStatus.PendingPayment,
                    VendorOrderStatus.Confirmed,
                    VendorOrderStatus.InProgress,
                    VendorOrderStatus.Shipped,
                    VendorOrderStatus.Delivered
                };

                // Get all eligible vendor orders
                var eligibleVendorOrders = await _vendorOrderRepository
                    .Where(vo => vo.MasterOrderId == request.MasterOrderId && eligibleStatuses.Contains(vo.Status))
                    .ToListAsync();

                if (!eligibleVendorOrders.Any())
                {
                    return BadRequest(new { Message = "No eligible vendor orders found for payment allocation. Orders must be in Confirmed, InProgress, Shipped, or Delivered status." });
                }

                // Ensure all 3 milestones exist for all eligible vendor orders and collect unpaid milestones
                var unpaidMilestones = new List<PaymentMilestone>();
                foreach (var vo in eligibleVendorOrders)
                {
                    await EnsureAllMilestonesCreatedAsync(vo.Id, vo.TotalPrice);
                    
                    var voUnpaid = await _milestoneRepository
                        .Where(m => m.VendorOrderId == vo.Id && !m.IsPaid)
                        .ToListAsync();
                    unpaidMilestones.AddRange(voUnpaid);
                }

                // Sort unpaid milestones: Shipped first, then Delivered, then PendingPayment (if any somehow left unpaid)
                unpaidMilestones = unpaidMilestones
                    .OrderBy(m => m.MilestoneStatus == VendorOrderStatus.Shipped ? 0 :
                                  m.MilestoneStatus == VendorOrderStatus.Delivered ? 1 : 2)
                    .ThenBy(m => m.VendorOrderId)
                    .ThenBy(m => m.Id)
                    .ToList();

                decimal amountToPay = Math.Round(unpaidMilestones.Sum(m => m.Amount), 2);
                if (amountToPay <= 0)
                {
                    return BadRequest(new { Message = "No active unpaid payment milestones found for this master order." });
                }

                // Create a single Paymob transaction for the full remaining balance
                var paymentResult = await _paymentGateway.CreatePaymentUrlAsync(
                    request.MasterOrderId,
                    amountToPay,
                    order.FirstName ?? string.Empty,
                    order.LastName ?? string.Empty,
                    order.Email ?? string.Empty,
                    order.PhoneNumber ?? string.Empty
                );

                foreach (var milestone in unpaidMilestones)
                {
                    milestone.PaymentTransactionId = paymentResult.Transaction.Id;
                }

                await _milestoneRepository.SaveChangesAsync();

                return Ok(new PaymobPaymentResponse { PaymentUrl = paymentResult.PaymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Payment initiation failed for master order {OrderId}",
                    request.MasterOrderId
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("paymob/initiate-vendororder")]
        public async Task<IActionResult> InitiateVendorOrderPayment(
            [FromBody] PaymobVendorOrderPaymentRequest request
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();

                var vendorOrder = await _vendorOrderRepository
                    .Where(vo => vo.Id == request.VendorOrderId)
                    .Include(vo => vo.MasterOrder)
                    .FirstOrDefaultAsync();

                if (vendorOrder == null)
                    return NotFound(new { Message = "Vendor order not found" });

                if (vendorOrder.MasterOrder.UserId != userId)
                    return Forbid();

                // Fetch active unpaid milestone matching the vendor order status
                var activeMilestone = await _milestoneRepository
                    .Where(m => m.VendorOrderId == request.VendorOrderId 
                             && m.MilestoneStatus == vendorOrder.Status 
                             && !m.IsPaid)
                    .FirstOrDefaultAsync();

                if (activeMilestone == null)
                {
                    return BadRequest(new { Message = "No active unpaid payment milestone found matching the current status of the order." });
                }

                var masterOrder = vendorOrder.MasterOrder;

                var paymentResult = await _paymentGateway.CreatePaymentUrlAsync(
                    masterOrder.Id,
                    activeMilestone.Amount,
                    masterOrder.FirstName ?? string.Empty,
                    masterOrder.LastName ?? string.Empty,
                    masterOrder.Email ?? string.Empty,
                    masterOrder.PhoneNumber ?? string.Empty
                );

                // Link the active milestone to this transaction
                activeMilestone.PaymentTransactionId = paymentResult.Transaction.Id;
                await _milestoneRepository.SaveChangesAsync();

                return Ok(new PaymobPaymentResponse { PaymentUrl = paymentResult.PaymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Payment initiation failed for vendor order {VendorOrderId}",
                    request.VendorOrderId
                );
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("masterorder/{masterOrderId}/remaining-balance")]
        public async Task<IActionResult> GetMasterOrderRemainingBalance(int masterOrderId)
        {
            try
            {
                var userId = GetUserId();
                var masterOrder = await _orderRepository.GetByIdAsync(masterOrderId);
                if (masterOrder == null)
                    return NotFound(new { Message = "Master order not found" });

                if (masterOrder.UserId != userId)
                    return Forbid();

                var breakdown = await _paymentService.GetMilestoneBreakdownForMasterOrderAsync(masterOrderId);
                return Ok(breakdown);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get remaining balance for master order {MasterOrderId}", masterOrderId);
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("vendororder/{vendorOrderId}/remaining-balance")]
        public async Task<IActionResult> GetVendorOrderRemainingBalance(int vendorOrderId)
        {
            try
            {
                var userId = GetUserId();
                var isVendor = User.IsInRole("Vendor");
                
                int? workshopId = null;
                if (isVendor)
                {
                    workshopId = await GetVendorWorkshopIdAsync();
                }

                var vendorOrder = await _vendorOrderRepository
                    .Where(vo => vo.Id == vendorOrderId)
                    .Include(vo => vo.MasterOrder)
                    .FirstOrDefaultAsync();

                if (vendorOrder == null)
                    return NotFound(new { Message = "Vendor order not found" });

                if (isVendor)
                {
                    if (vendorOrder.WorkshopId != workshopId.Value)
                        return Forbid();
                }
                else
                {
                    if (vendorOrder.MasterOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                        return Forbid();
                }

                var breakdown = await _paymentService.GetMilestoneBreakdownForVendorOrderAsync(vendorOrderId, workshopId);
                return Ok(breakdown);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get remaining balance for vendor order {VendorOrderId}", vendorOrderId);
                return BadRequest(new { Message = ex.Message });
            }
        }

        private async Task<int> GetVendorWorkshopIdAsync()
        {
            var workshopIdClaim = User.FindFirstValue("WorkshopId") ?? User.FindFirst("WorkshopId")?.Value;
            if (int.TryParse(workshopIdClaim, out var workshopId))
                return workshopId;

            var userId = GetUserId();
            var workshop = await _workshopRepository.FirstOrDefaultAsync(w => w.UserId == userId);

            if (workshop == null)
                throw new KeyNotFoundException("Workshop not found for the authenticated user");

            return workshop.Id;
        }

        private string GetUserId()
        {
            var userId =
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID not found");

            return userId;
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
                var paymentResult = await _paymentGateway.CreatePaymentUrlAsync(
                    request.OrderId,
                    request.Amount,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Phone
                );

                return Ok(new PaymobPaymentResponse { PaymentUrl = paymentResult.PaymentUrl });
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
                var transaction = await _paymentTransactionRepository.GetByPaymobOrderIdAsync(paymobOrderId?.ToString() ?? "");

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
                var order = await _orderRepository.GetByIdAsync(transaction.LocalOrderId);

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
                    await _paymentService.ProcessPaymentAsync(
                        transaction,
                        order,
                        PaymentStatus.Paid,
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
                    await _paymentService.ProcessPaymentAsync(
                        transaction,
                        order,
                        PaymentStatus.Cancelled,
                        paymobTransactionId.ToString(),
                        "Transaction voided"
                    );

                    _logger.LogInformation(
                        "Payment VOIDED — TransactionId: {TransactionId}, PaymobOrderId: {PaymobOrderId}, LocalOrderId: {LocalOrderId}",
                        paymobTransactionId,
                        paymobOrderId,
                        transaction.LocalOrderId
                    );
                }
                else
                {
                    await _paymentService.ProcessPaymentAsync(
                        transaction,
                        order,
                        PaymentStatus.Failed,
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
                await _webhookLogRepository.SaveChangesAsync();

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

                var merchantOrderIdStr = Request.Query["merchant_order_id"].ToString();
                var localOrderId = int.Parse(merchantOrderIdStr.Split('_')[0]);

                _logger.LogInformation(
                    "OrderId={OrderId}, Success={Success}",
                    localOrderId,
                    success
                );

                var paymobOrderId = Request.Query["order"].ToString();
                PaymentTransaction? transaction = null;
                if (!string.IsNullOrEmpty(paymobOrderId))
                {
                    transaction = await _paymentTransactionRepository.GetByPaymobOrderIdAsync(paymobOrderId);
                }

                if (transaction == null)
                {
                    transaction = await _paymentTransactionRepository.GetByLocalOrderIdAsync(localOrderId);
                }

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction not found for order {OrderId}", localOrderId);

                    return Ok(new { Message = "Transaction not found" });
                }

                var order = await _orderRepository.GetByIdAsync(localOrderId);
                await _paymentService.ProcessPaymentAsync(
                    transaction,
                    order,
                    success ? PaymentStatus.Paid : PaymentStatus.Failed,
                    Request.Query["id"].ToString()
                );

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

        // ── Private helpers ───────────────────────────────────────────────────
 
        private async Task EnsureAllMilestonesCreatedAsync(int vendorOrderId, decimal totalPrice)
        {
            var existingPending = await _milestoneRepository
                .FirstOrDefaultAsync(m => m.VendorOrderId == vendorOrderId && m.MilestoneStatus == VendorOrderStatus.PendingPayment);
            if (existingPending == null)
            {
                var amount = Math.Round(totalPrice * 0.30m, 2);
                var milestone = new PaymentMilestone
                {
                    VendorOrderId = vendorOrderId,
                    MilestoneStatus = VendorOrderStatus.PendingPayment,
                    Amount = amount,
                    IsPaid = true,
                    PaidAt = DateTime.UtcNow
                };
                await _milestoneRepository.AddAsync(milestone);
            }

            var existingShipped = await _milestoneRepository
                .FirstOrDefaultAsync(m => m.VendorOrderId == vendorOrderId && m.MilestoneStatus == VendorOrderStatus.Shipped);
            if (existingShipped == null)
            {
                var amount = Math.Round(totalPrice * 0.40m, 2);
                var milestone = new PaymentMilestone
                {
                    VendorOrderId = vendorOrderId,
                    MilestoneStatus = VendorOrderStatus.Shipped,
                    Amount = amount,
                    IsPaid = false
                };
                await _milestoneRepository.AddAsync(milestone);
            }

            var existingDelivered = await _milestoneRepository
                .FirstOrDefaultAsync(m => m.VendorOrderId == vendorOrderId && m.MilestoneStatus == VendorOrderStatus.Delivered);
            if (existingDelivered == null)
            {
                var pendingPaymentAmount = Math.Round(totalPrice * 0.30m, 2);
                var shippedAmount = Math.Round(totalPrice * 0.40m, 2);
                var amount = totalPrice - pendingPaymentAmount - shippedAmount;
                var milestone = new PaymentMilestone
                {
                    VendorOrderId = vendorOrderId,
                    MilestoneStatus = VendorOrderStatus.Delivered,
                    Amount = amount,
                    IsPaid = false
                };
                await _milestoneRepository.AddAsync(milestone);
            }

            await _milestoneRepository.SaveChangesAsync();
        }

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
