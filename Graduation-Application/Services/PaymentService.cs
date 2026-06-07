using System;
using System.Threading.Tasks;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Graduation_Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IOrderRepository orderRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            ILogger<PaymentService> logger
        )
        {
            _orderRepository = orderRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
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
            }
            else
            {
                t.Status = PaymentStatus.Failed;
                t.FailureReason = failureReason ?? "Payment failed";
            }

            await _paymentTransactionRepository.SaveChangesAsync();
        }
    }
}
