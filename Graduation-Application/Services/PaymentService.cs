using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Graduation_Domain.Enums;
using Microsoft.AspNetCore.Identity;
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
        private readonly IGenaricRepositories<PaymentMilestone> _milestoneRepository;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IOrderRepository orderRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IGenaricRepositories<VendorOrder> vendorOrderRepository,
            IInternalNotificationService internalNotificationService,
            IGenaricRepositories<PaymentMilestone> milestoneRepository,
            IEmailService emailService,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentService> logger
        )
        {
            _orderRepository = orderRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _vendorOrderRepository = vendorOrderRepository;
            _internalNotificationService = internalNotificationService;
            _milestoneRepository = milestoneRepository;
            _emailService = emailService;
            _userManager = userManager;
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

                var milestones = await _milestoneRepository
                    .Where(m => m.PaymentTransactionId == t.Id)
                    .Include(m => m.VendorOrder)
                        .ThenInclude(vo => vo.StatusHistory)
                    .Include(m => m.VendorOrder.MasterOrder)
                        .ThenInclude(mo => mo.VendorOrders)
                    .ToListAsync();

                if (milestones.Any())
                {
                    foreach (var milestone in milestones)
                    {
                        milestone.IsPaid = true;
                        milestone.PaidAt = DateTime.UtcNow;

                        var vo = milestone.VendorOrder;
                        var oldStatus = vo.Status;

                        if (milestone.MilestoneStatus == VendorOrderStatus.PendingPayment)
                        {
                            vo.Status = VendorOrderStatus.Confirmed;
                            vo.UpdatedAt = DateTime.UtcNow;
                            vo.StatusHistory.Add(new VendorOrderStatusHistory
                            {
                                VendorOrderId = vo.Id,
                                OldStatus = oldStatus.ToString(),
                                NewStatus = VendorOrderStatus.Confirmed.ToString()
                            });
                        }

                        // Save milestone changes
                        await _milestoneRepository.SaveChangesAsync();

                        // Send success notifications and emails
                        try
                        {
                            var userId = vo.MasterOrder.UserId;
                            var user = await _userManager.FindByIdAsync(userId);
                            var lang = user?.PreferredLanguage ?? "en";

                            // In-app
                            await _internalNotificationService.CreateAsync(userId, NotificationType.MilestonePaid, $"{milestone.MilestoneStatus}|{milestone.Amount}|{vo.Id}");

                            // Email
                            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                            {
                                await _emailService.SendMilestonePaymentSuccessEmailAsync(user.Email, vo.Id, milestone.MilestoneStatus.ToString(), milestone.Amount, lang);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send payment success notifications for Milestone ID {Id}.", milestone.Id);
                        }
                    }

                    // Recalculate parent MasterOrder status
                    var firstVo = milestones.First().VendorOrder;
                    var masterOrder = firstVo.MasterOrder;

                    var allStatuses = masterOrder.VendorOrders.Select(v => v.Status).ToList();
                    var derivedStatus = CalculateMasterOrderStatus(allStatuses);
                    masterOrder.Status = derivedStatus;
                    masterOrder.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Fallback to legacy flow
                    if (o != null)
                    {
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

                        var allVendorOrders = await _vendorOrderRepository
                            .Where(vo => vo.MasterOrderId == o.Id)
                            .ToListAsync();

                        var allStatuses = allVendorOrders.Select(vo => vo.Status).ToList();
                        var derivedStatus = CalculateMasterOrderStatus(allStatuses);
                        o.Status = derivedStatus;
                        o.UpdatedAt = DateTime.UtcNow;

                        await _internalNotificationService.CreateAsync(
                            o.UserId,
                            NotificationType.OrderConfirmed,
                            o.Id.ToString()
                        );
                    }
                }
            }
            else
            {
                t.Status = PaymentStatus.Failed;
                t.FailureReason = failureReason ?? "Payment failed";
            }

            await _paymentTransactionRepository.SaveChangesAsync();
        }

        public async Task<decimal> GetRemainingBalanceForMasterOrderAsync(int masterOrderId)
        {
            var masterOrder = await _orderRepository.GetByIdAsync(masterOrderId);
            if (masterOrder == null) return 0m;

            var paidMilestones = await _milestoneRepository
                .Where(m => m.VendorOrder.MasterOrderId == masterOrderId && m.IsPaid)
                .ToListAsync();

            var paidAmount = paidMilestones.Sum(m => m.Amount);
            return Math.Max(0m, masterOrder.TotalPrice - paidAmount);
        }

        public async Task<decimal> GetRemainingBalanceForVendorOrderAsync(int vendorOrderId)
        {
            var vendorOrder = await _vendorOrderRepository
                .Where(vo => vo.Id == vendorOrderId)
                .FirstOrDefaultAsync();
            if (vendorOrder == null) return 0m;

            var paidMilestones = await _milestoneRepository
                .Where(m => m.VendorOrderId == vendorOrderId && m.IsPaid)
                .ToListAsync();

            var paidAmount = paidMilestones.Sum(m => m.Amount);
            return Math.Max(0m, vendorOrder.TotalPrice - paidAmount);
        }

        public async Task<object> GetMilestoneBreakdownForMasterOrderAsync(int masterOrderId)
        {
            var milestones = await _milestoneRepository
                .Where(m => m.VendorOrder.MasterOrderId == masterOrderId)
                .Include(m => m.VendorOrder)
                    .ThenInclude(vo => vo.Workshop)
                .ToListAsync();

            var list = milestones.Select(m => new
            {
                MilestoneId = m.Id,
                VendorOrderId = m.VendorOrderId,
                VendorName = m.VendorOrder.Workshop?.WorkshopNameEn ?? "Vendor",
                MilestoneStatus = m.MilestoneStatus.ToString(),
                Amount = m.Amount,
                IsPaid = m.IsPaid,
                PaidAt = m.PaidAt
            }).ToList();

            var masterOrder = await _orderRepository.GetByIdAsync(masterOrderId);
            decimal totalPrice = masterOrder?.TotalPrice ?? 0m;
            decimal paidAmount = milestones.Where(m => m.IsPaid).Sum(m => m.Amount);
            decimal remainingBalance = Math.Max(0m, totalPrice - paidAmount);

            return new
            {
                MasterOrderId = masterOrderId,
                TotalPrice = totalPrice,
                RemainingBalance = remainingBalance,
                Milestones = list
            };
        }

        public async Task<object> GetMilestoneBreakdownForVendorOrderAsync(int vendorOrderId, int? workshopId = null)
        {
            var query = _milestoneRepository
                .Where(m => m.VendorOrderId == vendorOrderId);

            if (workshopId.HasValue)
            {
                query = query.Where(m => m.VendorOrder.WorkshopId == workshopId.Value);
            }

            var milestones = await query
                .Include(m => m.VendorOrder)
                .ToListAsync();

            var list = milestones.Select(m => new
            {
                MilestoneId = m.Id,
                MilestoneStatus = m.MilestoneStatus.ToString(),
                Amount = m.Amount,
                IsPaid = m.IsPaid,
                PaidAt = m.PaidAt
            }).ToList();

            var vendorOrder = await _vendorOrderRepository
                .Where(vo => vo.Id == vendorOrderId)
                .FirstOrDefaultAsync();

            decimal totalPrice = vendorOrder?.TotalPrice ?? 0m;
            decimal paidAmount = milestones.Where(m => m.IsPaid).Sum(m => m.Amount);
            decimal remainingBalance = Math.Max(0m, totalPrice - paidAmount);

            return new
            {
                VendorOrderId = vendorOrderId,
                WorkshopId = vendorOrder?.WorkshopId ?? 0,
                TotalPrice = totalPrice,
                RemainingBalance = remainingBalance,
                Milestones = list
            };
        }

        public async Task CreateMilestoneIfNotExistAsync(int vendorOrderId, VendorOrderStatus milestoneStatus, decimal totalAmount)
        {
            _logger.LogInformation("CreateMilestoneIfNotExistAsync: Order={Order}, Status={Status}", vendorOrderId, milestoneStatus);

            var existing = await _milestoneRepository
                .FirstOrDefaultAsync(m => m.VendorOrderId == vendorOrderId && m.MilestoneStatus == milestoneStatus);

            if (existing != null)
            {
                _logger.LogWarning("Milestone for VendorOrderId={Order} and Status={Status} already exists.", vendorOrderId, milestoneStatus);
                return;
            }

            decimal amount = 0m;
            if (milestoneStatus == VendorOrderStatus.PendingPayment)
            {
                amount = Math.Round(totalAmount * 0.30m, 2);
            }
            else if (milestoneStatus == VendorOrderStatus.Shipped)
            {
                amount = Math.Round(totalAmount * 0.40m, 2);
            }
            else if (milestoneStatus == VendorOrderStatus.Delivered)
            {
                var pendingPaymentAmount = Math.Round(totalAmount * 0.30m, 2);
                var shippedAmount = Math.Round(totalAmount * 0.40m, 2);
                amount = totalAmount - pendingPaymentAmount - shippedAmount;
            }
            else
            {
                throw new ArgumentException("Invalid status for payment milestone creation.");
            }

            var milestone = new PaymentMilestone
            {
                VendorOrderId = vendorOrderId,
                MilestoneStatus = milestoneStatus,
                Amount = amount,
                IsPaid = false,
                PaidAt = null
            };

            await _milestoneRepository.AddAsync(milestone);
            await _milestoneRepository.SaveChangesAsync();

            _logger.LogInformation("Milestone created successfully: ID={Id}, Amount={Amount}", milestone.Id, amount);

            try
            {
                var vendorOrder = await _vendorOrderRepository
                    .Where(vo => vo.Id == vendorOrderId)
                    .Include(vo => vo.MasterOrder)
                    .FirstOrDefaultAsync();

                if (vendorOrder != null)
                {
                    var userId = vendorOrder.MasterOrder.UserId;
                    var user = await _userManager.FindByIdAsync(userId);
                    var lang = user?.PreferredLanguage ?? "en";

                    await _internalNotificationService.CreateAsync(userId, NotificationType.MilestoneCreated, $"{milestoneStatus}|{amount}|{vendorOrderId}");

                    if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                    {
                        await _emailService.SendMilestoneCreatedEmailAsync(user.Email, vendorOrderId, milestoneStatus.ToString(), amount, lang);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send milestone creation notifications.");
            }
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
