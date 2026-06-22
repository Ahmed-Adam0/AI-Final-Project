using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Graduation_Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Graduation_Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IGenaricRepositories<VendorOrder> _vendorOrderRepository;
        private readonly IInternalNotificationService _internalNotificationService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IOrderRepository orderRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IGenaricRepositories<VendorOrder> vendorOrderRepository,
            IInternalNotificationService internalNotificationService,
            ILogger<PaymentService> logger
        )
        {
            _orderRepository = orderRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _vendorOrderRepository = vendorOrderRepository;
            _internalNotificationService = internalNotificationService;
            _logger = logger;
        }

        public async Task ProcessPaymentAsync(
            PaymentTransaction t,
            Order? o,
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
            t.TransactionId = transactionId;

            if (success)
            {
                t.Status = PaymentStatus.Paid;
                t.PaidAt = DateTime.UtcNow;
                t.FailureReason = null;

                if (o != null)
                {
                    // 1. Fetch VendorOrders under this MasterOrder that are in PendingPayment status
                    var pendingPaymentVendorOrders = await _vendorOrderRepository
                        .Where(vo => vo.MasterOrderId == o.Id && vo.Status == VendorOrderStatus.PendingPayment)
                        .Include(vo => vo.StatusHistory)
                        .ToListAsync();

                    foreach (var vo in pendingPaymentVendorOrders)
                    {
                        var oldStatus = vo.Status.ToString();
                        vo.Status = VendorOrderStatus.Confirmed;
                        vo.UpdatedAt = DateTime.UtcNow;
                        vo.StatusHistory.Add(new VendorOrderStatusHistory
                        {
                            VendorOrderId = vo.Id,
                            OldStatus = oldStatus,
                            NewStatus = VendorOrderStatus.Confirmed.ToString()
                        });
                    }

                    // 2. Derive MasterOrder status based on all child VendorOrders
                    var allVendorOrders = await _vendorOrderRepository
                        .Where(vo => vo.MasterOrderId == o.Id)
                        .ToListAsync();

                    // Map all vendor statuses (with local modifications reflected)
                    var allStatuses = allVendorOrders
                        .Select(vo => vo.MasterOrderId == o.Id && pendingPaymentVendorOrders.Any(pvo => pvo.Id == vo.Id) 
                            ? VendorOrderStatus.Confirmed 
                            : vo.Status)
                        .ToList();

                    var derivedStatus = CalculateMasterOrderStatus(allStatuses);
                    o.Status = derivedStatus;
                    o.UpdatedAt = DateTime.UtcNow;

                    // 3. Notify customer
                    await _internalNotificationService.CreateAsync(
                        o.UserId,
                        NotificationType.OrderConfirmed,
                        o.Id.ToString()
                    );
                }
            }
            else
            {
                t.Status = PaymentStatus.Failed;
                t.FailureReason = failureReason ?? "Payment failed";
            }

            await _paymentTransactionRepository.SaveChangesAsync();
        }

        private string CalculateMasterOrderStatus(List<VendorOrderStatus> statuses)
        {
            if (!statuses.Any()) return "Pending";

            if (statuses.All(s => s == VendorOrderStatus.Cancelled))
                return "Cancelled";

            var nonCancelled = statuses.Where(s => s != VendorOrderStatus.Cancelled).ToList();
            if (nonCancelled.All(s => s == VendorOrderStatus.Delivered))
                return "Completed";

            if (statuses.Any(s => s == VendorOrderStatus.Delivered))
                return "PartiallyDelivered";

            if (statuses.Any(s => s == VendorOrderStatus.AwaitingCustomerApproval || 
                                s == VendorOrderStatus.PendingPayment ||
                                s == VendorOrderStatus.Confirmed || 
                                s == VendorOrderStatus.InProgress || 
                                s == VendorOrderStatus.Shipped))
                return "Processing";

            return "Pending";
        }
    }
}
