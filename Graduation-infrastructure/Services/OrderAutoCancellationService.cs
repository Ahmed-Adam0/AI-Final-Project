using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Graduation_infrastructure.AppDbContext;
using Graduation_Application.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Graduation_infrastructure.Services
{
    public class OrderAutoCancellationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderAutoCancellationService> _logger;
        private readonly IConfiguration _configuration;

        public OrderAutoCancellationService(
            IServiceProvider serviceProvider,
            ILogger<OrderAutoCancellationService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order Auto-Cancellation Service started.");

            var checkIntervalMinutes = _configuration.GetValue<int>("OrderSla:CheckIntervalMinutes", 5);
            var slaTimeoutHours = _configuration.GetValue<int>("OrderSla:SlaTimeoutHours", 48);

            _logger.LogInformation("Service configured with CheckIntervalMinutes={Interval} and SlaTimeoutHours={SlaTimeout}", checkIntervalMinutes, slaTimeoutHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAutoCancellationsAsync(slaTimeoutHours);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while running Order Auto-Cancellation worker.");
                }

                await Task.Delay(TimeSpan.FromMinutes(checkIntervalMinutes), stoppingToken);
            }

            _logger.LogInformation("Order Auto-Cancellation Service stopped.");
        }

        private async Task ProcessAutoCancellationsAsync(int slaTimeoutHours)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var internalNotificationService = scope.ServiceProvider.GetRequiredService<IInternalNotificationService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var thresholdTime = DateTime.UtcNow.AddHours(-slaTimeoutHours);

            _logger.LogInformation("Querying for expired vendor orders older than {ThresholdTime}", thresholdTime);

            // Fetch vendor orders that are NOT in final states (Delivered, Cancelled) and have exceeded the SLA threshold.
            var expiredVendorOrders = await dbContext.VendorOrders
                .Include(vo => vo.StatusHistory)
                .Include(vo => vo.MasterOrder)
                    .ThenInclude(mo => mo.VendorOrders)
                .Include(vo => vo.Workshop)
                    .ThenInclude(w => w.User)
                .Where(vo => vo.Status != VendorOrderStatus.Delivered 
                          && vo.Status != VendorOrderStatus.Cancelled
                          && vo.CreatedAt <= thresholdTime)
                .ToListAsync();

            if (!expiredVendorOrders.Any())
            {
                _logger.LogInformation("No expired vendor orders found.");
                return;
            }

            _logger.LogInformation("Found {Count} expired vendor orders to auto-cancel.", expiredVendorOrders.Count);

            foreach (var vendorOrder in expiredVendorOrders)
            {
                try
                {
                    _logger.LogInformation("Auto-cancelling VendorOrder ID {OrderId} (Created at {CreatedAt}) due to SLA timeout.", vendorOrder.Id, vendorOrder.CreatedAt);

                    var oldStatus = vendorOrder.Status.ToString();
                    vendorOrder.Status = VendorOrderStatus.Cancelled;
                    vendorOrder.UpdatedAt = DateTime.UtcNow;

                    vendorOrder.StatusHistory.Add(new VendorOrderStatusHistory
                    {
                        VendorOrderId = vendorOrder.Id,
                        OldStatus = oldStatus,
                        NewStatus = VendorOrderStatus.Cancelled.ToString()
                    });

                    // Recalculate MasterOrder status
                    var allVendorStatuses = vendorOrder.MasterOrder.VendorOrders
                        .Select(v => v.Id == vendorOrder.Id ? VendorOrderStatus.Cancelled : v.Status)
                        .ToList();
                    
                    var derivedStatus = CalculateMasterOrderStatus(allVendorStatuses);
                    vendorOrder.MasterOrder.Status = derivedStatus;
                    vendorOrder.MasterOrder.UpdatedAt = DateTime.UtcNow;

                    // Save database changes atomically for this order cancellation
                    await dbContext.SaveChangesAsync();

                    // Send Notifications to Customer
                    try
                    {
                        var customerUserId = vendorOrder.MasterOrder.UserId;
                        await internalNotificationService.CreateAsync(
                            customerUserId,
                            NotificationType.OrderCancelled,
                            vendorOrder.Id.ToString()
                        );

                        // Find customer email
                        var customer = await dbContext.Orders
                            .Where(o => o.Id == vendorOrder.MasterOrderId)
                            .Select(o => o.User)
                            .FirstOrDefaultAsync();

                        if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
                        {
                            await emailService.SendOrderStatusChangedEmailAsync(customer.Email, vendorOrder.Id, "Cancelled");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notifications to customer for cancelled VendorOrder {OrderId}.", vendorOrder.Id);
                    }

                    // Send Notifications to Vendor
                    try
                    {
                        var vendorUserId = vendorOrder.Workshop.UserId;
                        await internalNotificationService.CreateAsync(
                            vendorUserId,
                            NotificationType.VendorOrderCancelled,
                            vendorOrder.Id.ToString()
                        );

                        if (vendorOrder.Workshop.User != null && !string.IsNullOrWhiteSpace(vendorOrder.Workshop.User.Email))
                        {
                            await emailService.SendOrderStatusChangedEmailAsync(vendorOrder.Workshop.User.Email, vendorOrder.Id, "Cancelled");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notifications to vendor for cancelled VendorOrder {OrderId}.", vendorOrder.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing auto-cancellation for VendorOrder ID {OrderId}.", vendorOrder.Id);
                }
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
                                s == VendorOrderStatus.Confirmed || 
                                s == VendorOrderStatus.InProgress || 
                                s == VendorOrderStatus.ReadyForPickup))
                return "Processing";

            return "Pending";
        }
    }
}
