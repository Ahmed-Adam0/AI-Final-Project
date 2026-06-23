using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class VendorOrderService : IVendorOrderService
    {
        private readonly IGenaricRepositories<VendorOrder> _vendorOrderRepository;
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly INotificationService _notificationService;
        private readonly IInternalNotificationService _internalNotificationService;
        private readonly IEmailService _emailService;

        public VendorOrderService(
            IGenaricRepositories<VendorOrder> vendorOrderRepository,
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<Workshop> workshopRepository,
            INotificationService notificationService,
            IInternalNotificationService internalNotificationService,
            IEmailService emailService
        )
        {
            _vendorOrderRepository = vendorOrderRepository;
            _orderRepository = orderRepository;
            _workshopRepository = workshopRepository;
            _notificationService = notificationService;
            _internalNotificationService = internalNotificationService;
            _emailService = emailService;
        }

        // 1. Get Vendor Orders with Filtering and Pagination
        public async Task<(
            List<VendorOrderDashboardDto> Orders,
            int TotalCount
        )> GetVendorOrdersAsync(int workshopId, VendorOrdersFilterDto filter)
        {
            var query = _vendorOrderRepository
                .Where(vo => vo.WorkshopId == workshopId)
                .Include(vo => vo.MasterOrder)
                    .ThenInclude(mo => mo.User)
                .Include(vo => vo.Items)
                .Include(vo => vo.StatusHistory)
                .AsNoTracking();

            // Status Filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                if (Enum.TryParse<VendorOrderStatus>(filter.Status, true, out var statusEnum))
                {
                    query = query.Where(vo => vo.Status == statusEnum);
                }
            }

            // Date Range Filter
            if (filter.StartDate.HasValue)
            {
                query = query.Where(vo => vo.CreatedAt >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                var endDate = filter.EndDate.Value.Date.AddDays(1);
                query = query.Where(vo => vo.CreatedAt < endDate);
            }

            // Customer Name Filter
            if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            {
                query = query.Where(vo =>
                    EF.Functions.Like(vo.MasterOrder.User.FullName, $"%{filter.CustomerName}%")
                );
            }

            // Total count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            switch (filter.SortBy?.ToLower())
            {
                case "totalprice":
                    query = filter.SortDescending
                        ? query.OrderByDescending(vo => vo.TotalPrice)
                        : query.OrderBy(vo => vo.TotalPrice);
                    break;

                case "status":
                    query = filter.SortDescending
                        ? query.OrderByDescending(vo => vo.Status)
                        : query.OrderBy(vo => vo.Status);
                    break;

                case "customername":
                    query = filter.SortDescending
                        ? query.OrderByDescending(vo => vo.MasterOrder.User.FullName)
                        : query.OrderBy(vo => vo.MasterOrder.User.FullName);
                    break;

                default:
                    query = filter.SortDescending
                        ? query.OrderByDescending(vo => vo.CreatedAt)
                        : query.OrderBy(vo => vo.CreatedAt);
                    break;
            }

            // Pagination + Projection
            var orders = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(vo => new VendorOrderDashboardDto
                {
                    Id = vo.Id,
                    MasterOrderId = vo.MasterOrderId,
                    CustomerName = string.IsNullOrWhiteSpace(vo.MasterOrder.FirstName) && string.IsNullOrWhiteSpace(vo.MasterOrder.LastName)
                        ? (vo.MasterOrder.User != null ? vo.MasterOrder.User.FullName : "Customer")
                        : $"{vo.MasterOrder.FirstName} {vo.MasterOrder.LastName}".Trim(),
                    CustomerPhone = (vo.Status == VendorOrderStatus.Shipped || vo.Status == VendorOrderStatus.Delivered)
                        ? (vo.MasterOrder.PhoneNumber ?? (vo.MasterOrder.User != null ? vo.MasterOrder.User.PhoneNumber : "") ?? "")
                        : "",
                    TotalPrice = vo.Items.Sum(oi => oi.SnapshotUnitPrice * oi.Quantity),
                    Status = vo.Status.ToString(),
                    Address = vo.MasterOrder.Address ?? "",
                    Notes = vo.MasterOrder.Notes ?? "",
                    CreatedAt = vo.CreatedAt,
                    UpdatedAt = vo.UpdatedAt,
                    EstimatedDeliveryDate = vo.EstimatedDeliveryDate,
                    ItemCount = vo.Items.Sum(oi => oi.Quantity),
                    Items = vo
                        .Items.Select(oi => new VendorOrderItemDto
                        {
                            ProductId = oi.ProductId ?? 0,
                            ProductName = oi.SnapshotProductNameEn,
                            UnitPrice = oi.SnapshotUnitPrice,
                            Quantity = oi.Quantity,
                            Total = oi.SnapshotUnitPrice * oi.Quantity,
                        })
                        .ToList(),
                    StatusHistory = vo
                        .StatusHistory.Select(sh => new OrderStatusHistoryResponseDto
                        {
                            Id = sh.Id,
                            OldStatus = sh.OldStatus,
                            NewStatus = sh.NewStatus,
                            CreatedAt = sh.CreatedAt,
                        })
                        .OrderByDescending(s => s.CreatedAt)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return (orders, totalCount);
        }

        // 2. Get Vendor Order Details
        public async Task<VendorOrderDetailsDto> GetVendorOrderDetailsAsync(
            int orderId,
            int workshopId
        )
        {
            var vo = await _vendorOrderRepository
                .Where(v => v.Id == orderId && v.WorkshopId == workshopId)
                .Include(v => v.MasterOrder)
                    .ThenInclude(mo => mo.User)
                .Include(v => v.Items)
                .Include(v => v.StatusHistory)
                .FirstOrDefaultAsync();

            if (vo == null)
            {
                throw new Exception("Order not found or unauthorized");
            }

            return new VendorOrderDetailsDto
            {
                Id = vo.Id,
                MasterOrderId = vo.MasterOrderId,
                UserId = vo.MasterOrder.UserId,
                CustomerName = string.IsNullOrWhiteSpace(vo.MasterOrder.FirstName) && string.IsNullOrWhiteSpace(vo.MasterOrder.LastName)
                    ? (vo.MasterOrder.User != null ? vo.MasterOrder.User.FullName : "Customer")
                    : $"{vo.MasterOrder.FirstName} {vo.MasterOrder.LastName}".Trim(),
                CustomerPhone = (vo.Status == VendorOrderStatus.Shipped || vo.Status == VendorOrderStatus.Delivered)
                    ? (vo.MasterOrder.PhoneNumber ?? (vo.MasterOrder.User != null ? vo.MasterOrder.User.PhoneNumber : "") ?? "")
                    : "",
                TotalPrice = vo.Items.Sum(oi => oi.SnapshotUnitPrice * oi.Quantity),
                Status = vo.Status.ToString(),
                Address = vo.MasterOrder.Address ?? "",
                Notes = vo.MasterOrder.Notes ?? "",
                CreatedAt = vo.CreatedAt,
                UpdatedAt = vo.UpdatedAt,
                EstimatedDeliveryDate = vo.EstimatedDeliveryDate,
                Items = vo
                    .Items.Select(oi => new VendorOrderItemDto
                    {
                        ProductId = oi.ProductId ?? 0,
                        ProductName = oi.SnapshotProductNameEn,
                        UnitPrice = oi.SnapshotUnitPrice,
                        Quantity = oi.Quantity,
                        Total = oi.SnapshotUnitPrice * oi.Quantity,
                    })
                    .ToList(),
                StatusHistory = vo
                    .StatusHistory.Select(sh => new OrderStatusHistoryResponseDto
                    {
                        Id = sh.Id,
                        OldStatus = sh.OldStatus,
                        NewStatus = sh.NewStatus,
                        CreatedAt = sh.CreatedAt,
                    })
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefault(),
            };
        }

        // 3. Update Vendor Order Status
        public async Task UpdateVendorOrderStatusAsync(
            int orderId,
            int workshopId,
            string newStatus
        )
        {
            if (!Enum.TryParse<VendorOrderStatus>(newStatus, true, out var statusEnum))
            {
                throw new Exception("Invalid status");
            }

            var vendorOrder = await _vendorOrderRepository
                .Where(vo => vo.Id == orderId && vo.WorkshopId == workshopId)
                .Include(vo => vo.StatusHistory)
                .Include(vo => vo.MasterOrder)
                    .ThenInclude(mo => mo.VendorOrders)
                .FirstOrDefaultAsync();

            if (vendorOrder == null)
            {
                throw new Exception("Order not found or unauthorized");
            }

            // Validate state transition
            if (!IsValidStatusTransition(vendorOrder.Status, statusEnum))
            {
                throw new Exception($"Cannot transition from {vendorOrder.Status} to {newStatus}");
            }

            var oldStatusStr = vendorOrder.Status.ToString();
            vendorOrder.Status = statusEnum;
            vendorOrder.UpdatedAt = DateTime.UtcNow;

            // Add to status history
            vendorOrder.StatusHistory.Add(
                new VendorOrderStatusHistory
                {
                    VendorOrderId = orderId,
                    OldStatus = oldStatusStr,
                    NewStatus = newStatus,
                }
            );

            // Recalculate parent Master Order status
            var allVendorStatuses = vendorOrder
                .MasterOrder.VendorOrders.Select(v => v.Id == orderId ? statusEnum : v.Status)
                .ToList();

            var derivedStatus = CalculateMasterOrderStatus(allVendorStatuses);
            vendorOrder.MasterOrder.Status = derivedStatus;
            vendorOrder.MasterOrder.UpdatedAt = DateTime.UtcNow;

            await _vendorOrderRepository.SaveChangesAsync();

            // Send internal notifications based on status
            switch (newStatus)
            {
                case "InProgress":
                    await _internalNotificationService.CreateAsync(
                        vendorOrder.MasterOrder.UserId,
                        NotificationType.OrderInProgress,
                        vendorOrder.MasterOrderId.ToString()
                    );
                    break;
                case "Shipped":
                    await _internalNotificationService.CreateAsync(
                        vendorOrder.MasterOrder.UserId,
                        NotificationType.OrderReadyForPickup,
                        vendorOrder.MasterOrderId.ToString()
                    );
                    break;
                case "Delivered":
                    await _internalNotificationService.CreateAsync(
                        vendorOrder.MasterOrder.UserId,
                        NotificationType.OrderDelivered,
                        vendorOrder.MasterOrderId.ToString()
                    );
                    break;
                case "Cancelled":
                    await _internalNotificationService.CreateAsync(
                        vendorOrder.MasterOrder.UserId,
                        NotificationType.OrderCancelled,
                        vendorOrder.MasterOrderId.ToString()
                    );
                    break;
            }

            // Send notification to customer
            await _notificationService.SendOrderStatusUpdateAsync(
                vendorOrder.MasterOrder.UserId,
                vendorOrder.MasterOrderId,
                derivedStatus
            );
        }

        // 4. Get Revenue Statistics
        public async Task<VendorRevenueStatisticsDto> GetVendorRevenueStatisticsAsync(
            int workshopId,
            DateTime? startDate,
            DateTime? endDate
        )
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var weekStart = now.AddDays(-(int)now.DayOfWeek);
            var dayStart = now.Date;

            startDate ??= monthStart;
            endDate ??= now;

            var allOrdersInRangeQuery = _vendorOrderRepository
                .Where(vo =>
                    vo.WorkshopId == workshopId
                    && vo.CreatedAt >= startDate
                    && vo.CreatedAt <= endDate
                )
                .AsQueryable();

            var deliveredInRangeQuery = allOrdersInRangeQuery.Where(vo =>
                vo.Status == VendorOrderStatus.Delivered
            );

            var deliveredAllTimeQuery = _vendorOrderRepository
                .Where(vo =>
                    vo.WorkshopId == workshopId && vo.Status == VendorOrderStatus.Delivered
                )
                .AsQueryable();

            var totalRevenue = await deliveredInRangeQuery.SumAsync(vo => vo.TotalPrice);

            var monthlyRevenue = await deliveredAllTimeQuery
                .Where(vo => vo.CreatedAt >= monthStart && vo.CreatedAt <= now)
                .SumAsync(vo => vo.TotalPrice);

            var weeklyRevenue = await deliveredAllTimeQuery
                .Where(vo => vo.CreatedAt >= weekStart && vo.CreatedAt <= now)
                .SumAsync(vo => vo.TotalPrice);

            var dailyRevenue = await deliveredAllTimeQuery
                .Where(vo => vo.CreatedAt >= dayStart && vo.CreatedAt <= now)
                .SumAsync(vo => vo.TotalPrice);

            var completedOrdersCount = await deliveredInRangeQuery.CountAsync();

            var dailyBreakdown = await deliveredInRangeQuery
                .GroupBy(vo => vo.CreatedAt.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice),
                    OrdersCount = g.Count(),
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var ordersByStatus = await allOrdersInRangeQuery
                .GroupBy(vo => vo.Status)
                .Select(g => new OrdersByStatusDto { Status = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var monthlyBreakdown = await deliveredInRangeQuery
                .GroupBy(vo => new { vo.CreatedAt.Year, vo.CreatedAt.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(x => x.TotalPrice),
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return new VendorRevenueStatisticsDto
            {
                TotalRevenue = totalRevenue,
                MonthlyRevenue = monthlyRevenue,
                WeeklyRevenue = weeklyRevenue,
                DailyRevenue = dailyRevenue,
                CompletedOrdersCount = completedOrdersCount,
                DailyBreakdown = dailyBreakdown,
                OrdersByStatus = ordersByStatus,
                MonthlyBreakdown = monthlyBreakdown,
            };
        }

        // 5. Get Order Analytics
        public async Task<VendorOrderAnalyticsDto> GetVendorOrderAnalyticsAsync(
            int workshopId,
            DateTime? startDate,
            DateTime? endDate
        )
        {
            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var query = _vendorOrderRepository
                .Where(vo => vo.WorkshopId == workshopId)
                .AsQueryable();

            var ordersInRange = await query
                .Where(vo => vo.CreatedAt >= startDate && vo.CreatedAt <= endDate)
                .Include(vo => vo.StatusHistory)
                .ToListAsync();

            var totalOrders = ordersInRange.Count;
            var completedOrders = ordersInRange.Count(vo =>
                vo.Status == VendorOrderStatus.Delivered
            );
            var cancelledOrders = ordersInRange.Count(vo =>
                vo.Status == VendorOrderStatus.Cancelled
            );
            var pendingOrders = ordersInRange.Count(vo => vo.Status == VendorOrderStatus.Pending);
            var inProgressOrders = ordersInRange.Count(vo =>
                vo.Status == VendorOrderStatus.InProgress
                || vo.Status == VendorOrderStatus.Confirmed
                || vo.Status == VendorOrderStatus.Shipped
            );

            var totalRevenue = ordersInRange
                .Where(vo => vo.Status == VendorOrderStatus.Delivered)
                .Sum(vo => vo.TotalPrice);

            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var averageCompletionTime = CalculateAverageCompletionTime(ordersInRange);
            var completionRate = totalOrders > 0 ? (completedOrders * 100m) / totalOrders : 0;

            return new VendorOrderAnalyticsDto
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                PendingOrders = pendingOrders,
                InProgressOrders = inProgressOrders,
                AverageOrderValue = averageOrderValue,
                AverageCompletionTimeHours = averageCompletionTime,
                CompletionRate = completionRate,
            };
        }

        // 6. Get Dashboard Metrics
        public async Task<VendorDashboardMetricsDto> GetVendorDashboardMetricsAsync(int workshopId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);

            var query = _vendorOrderRepository.Where(vo => vo.WorkshopId == workshopId);

            var currentMonthOrders = await query
                .Where(vo => vo.CreatedAt >= monthStart && vo.CreatedAt <= now)
                .ToListAsync();

            var lastMonthOrders = await query
                .Where(vo => vo.CreatedAt >= lastMonthStart && vo.CreatedAt < monthStart)
                .ToListAsync();

            var totalOrders = await query.CountAsync();
            var activeOrders = await query
                .Where(vo =>
                    vo.Status != VendorOrderStatus.Delivered
                    && vo.Status != VendorOrderStatus.Cancelled
                )
                .CountAsync();
            var completedOrders = await query
                .Where(vo => vo.Status == VendorOrderStatus.Delivered)
                .CountAsync();

            var currentRevenue = currentMonthOrders
                .Where(vo => vo.Status == VendorOrderStatus.Delivered)
                .Sum(vo => vo.TotalPrice);

            var lastRevenue = lastMonthOrders
                .Where(vo => vo.Status == VendorOrderStatus.Delivered)
                .Sum(vo => vo.TotalPrice);

            var orderGrowth =
                lastRevenue > 0 ? ((currentRevenue - lastRevenue) / lastRevenue) * 100 : 0;

            var allOrders = await query.ToListAsync();
            var averageOrderValue =
                allOrders.Count > 0
                    ? allOrders
                        .Where(vo => vo.Status == VendorOrderStatus.Delivered)
                        .Sum(vo => vo.TotalPrice) / allOrders.Count
                    : 0;

            return new VendorDashboardMetricsDto
            {
                TotalOrders = totalOrders,
                ActiveOrders = activeOrders,
                CompletedOrders = completedOrders,
                TotalRevenue = currentRevenue,
                NewCustomersCount = await GetNewCustomersCountAsync(workshopId),
                OrderGrowthPercentage = (decimal)orderGrowth,
                AverageOrderValue = averageOrderValue,
            };
        }

        // 7. Get Activity Report
        public async Task<VendorActivityReportDto> GetVendorActivityReportAsync(
            int workshopId,
            string reportType,
            DateTime startDate,
            DateTime endDate
        )
        {
            var orders = await _vendorOrderRepository
                .Where(vo =>
                    vo.WorkshopId == workshopId
                    && vo.CreatedAt >= startDate
                    && vo.CreatedAt <= endDate.AddDays(1)
                )
                .Include(vo => vo.MasterOrder)
                    .ThenInclude(mo => mo.User)
                .ToListAsync();

            var completedOrders = orders.Count(vo => vo.Status == VendorOrderStatus.Delivered);
            var cancelledOrders = orders.Count(vo => vo.Status == VendorOrderStatus.Cancelled);
            var totalRevenue = orders
                .Where(vo => vo.Status == VendorOrderStatus.Delivered)
                .Sum(vo => vo.TotalPrice);

            return new VendorActivityReportDto
            {
                ReportDate = DateTime.UtcNow,
                ReportType = reportType,
                TotalOrders = orders.Count,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                AverageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0,
                Orders = orders
                    .Select(vo => new OrderActivityDto
                    {
                        OrderId = vo.Id,
                        CustomerName = string.IsNullOrWhiteSpace(vo.MasterOrder.FirstName) && string.IsNullOrWhiteSpace(vo.MasterOrder.LastName)
                            ? (vo.MasterOrder.User != null ? vo.MasterOrder.User.FullName : "Customer")
                            : $"{vo.MasterOrder.FirstName} {vo.MasterOrder.LastName}".Trim(),
                        Status = vo.Status.ToString(),
                        Amount = vo.TotalPrice,
                        CreatedAt = vo.CreatedAt,
                    })
                    .ToList(),
            };
        }

        // Helper methods
        private bool IsValidStatusTransition(
            VendorOrderStatus fromStatus,
            VendorOrderStatus toStatus
        )
        {
            var validTransitions = new Dictionary<VendorOrderStatus, List<VendorOrderStatus>>
            {
                {
                    VendorOrderStatus.Pending,
                    new List<VendorOrderStatus>
                    {
                        VendorOrderStatus.AwaitingCustomerApproval,
                        VendorOrderStatus.Cancelled,
                    }
                },
                {
                    VendorOrderStatus.AwaitingCustomerApproval,
                    new List<VendorOrderStatus>
                    {
                        VendorOrderStatus.Confirmed,
                        VendorOrderStatus.Pending,
                        VendorOrderStatus.Cancelled,
                    }
                },
                {
                    VendorOrderStatus.PendingPayment,
                    new List<VendorOrderStatus>
                    {
                        VendorOrderStatus.Confirmed,
                        VendorOrderStatus.Cancelled,
                    }
                },
                {
                    VendorOrderStatus.Confirmed,
                    new List<VendorOrderStatus>
                    {
                        VendorOrderStatus.InProgress,
                        VendorOrderStatus.Cancelled,
                    }
                },
                {
                    VendorOrderStatus.InProgress,
                    new List<VendorOrderStatus>
                    {
                        VendorOrderStatus.Shipped,
                        VendorOrderStatus.Cancelled,
                    }
                },
                {
                    VendorOrderStatus.Shipped,
                    new List<VendorOrderStatus> { VendorOrderStatus.Delivered }
                },
                { VendorOrderStatus.Delivered, new List<VendorOrderStatus>() },
                { VendorOrderStatus.Cancelled, new List<VendorOrderStatus>() },
            };

            return validTransitions.ContainsKey(fromStatus)
                && validTransitions[fromStatus].Contains(toStatus);
        }

        private string CalculateMasterOrderStatus(List<VendorOrderStatus> statuses)
        {
            if (!statuses.Any())
                return "Pending";

            if (statuses.All(s => s == VendorOrderStatus.Cancelled))
                return "Cancelled";

            var nonCancelled = statuses.Where(s => s != VendorOrderStatus.Cancelled).ToList();
            if (nonCancelled.All(s => s == VendorOrderStatus.Delivered))
                return "Completed";

            if (statuses.Any(s => s == VendorOrderStatus.Delivered))
                return "PartiallyDelivered";

            if (
                statuses.Any(s =>
                    s == VendorOrderStatus.AwaitingCustomerApproval
                    || s == VendorOrderStatus.Confirmed
                    || s == VendorOrderStatus.InProgress
                    || s == VendorOrderStatus.Shipped
                )
            )
                return "Processing";

            if (statuses.Any(s => s == VendorOrderStatus.Confirmed))
                return "Confirmed";

            return "Pending";
        }

        private double CalculateAverageCompletionTime(List<VendorOrder> orders)
        {
            var completedOrders = orders
                .Where(vo => vo.Status == VendorOrderStatus.Delivered && vo.StatusHistory.Any())
                .ToList();

            if (!completedOrders.Any())
                return 0;

            var totalHours = completedOrders.Sum(vo =>
            {
                var createdAt = vo.CreatedAt;
                var deliveredAt = vo
                    .StatusHistory.Where(sh =>
                        sh.NewStatus == VendorOrderStatus.Delivered.ToString()
                    )
                    .Select(sh => sh.CreatedAt)
                    .FirstOrDefault();

                if (deliveredAt == default)
                    return 0;

                return (deliveredAt - createdAt).TotalHours;
            });

            return totalHours / completedOrders.Count;
        }

        private async Task<int> GetNewCustomersCountAsync(int workshopId)
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var userIds = await _vendorOrderRepository
                .Where(vo => vo.WorkshopId == workshopId && vo.CreatedAt >= monthStart)
                .Include(vo => vo.MasterOrder)
                .Select(vo => vo.MasterOrder.UserId)
                .Distinct()
                .CountAsync();

            return userIds;
        }

        public async Task ProposeDeliveryDateAsync(
            int orderId,
            int workshopId,
            ProposeDeliveryDateRequestDto dto
        )
        {
            if (dto.EstimatedDeliveryDate < DateTime.UtcNow.AddMinutes(-5))
            {
                throw new Exception(
                    "Estimated delivery date must be in the future (current time or later). / يجب أن يكون تاريخ التوصيل المتوقع في المستقبل (الوقت الحالي أو بعده)."
                );
            }

            var vendorOrder = await _vendorOrderRepository
                .Where(vo => vo.Id == orderId && vo.WorkshopId == workshopId)
                .Include(vo => vo.StatusHistory)
                .Include(vo => vo.MasterOrder)
                    .ThenInclude(mo => mo.VendorOrders)
                .FirstOrDefaultAsync();

            if (vendorOrder == null)
            {
                throw new Exception("Order not found or unauthorized");
            }

            if (
                vendorOrder.Status != VendorOrderStatus.Pending
                && vendorOrder.Status != VendorOrderStatus.AwaitingCustomerApproval
            )
            {
                throw new Exception(
                    $"Cannot propose delivery date when status is {vendorOrder.Status}"
                );
            }

            var oldStatus = vendorOrder.Status.ToString();
            vendorOrder.Status = VendorOrderStatus.AwaitingCustomerApproval;
            vendorOrder.EstimatedDeliveryDate = dto.EstimatedDeliveryDate;
            vendorOrder.UpdatedAt = DateTime.UtcNow;

            vendorOrder.StatusHistory.Add(
                new VendorOrderStatusHistory
                {
                    VendorOrderId = orderId,
                    OldStatus = oldStatus,
                    NewStatus = VendorOrderStatus.AwaitingCustomerApproval.ToString(),
                }
            );

            // Derive MasterOrder status
            var allVendorStatuses = vendorOrder
                .MasterOrder.VendorOrders.Select(v =>
                    v.Id == orderId ? VendorOrderStatus.AwaitingCustomerApproval : v.Status
                )
                .ToList();
            var derivedStatus = CalculateMasterOrderStatus(allVendorStatuses);
            vendorOrder.MasterOrder.Status = derivedStatus;
            vendorOrder.MasterOrder.UpdatedAt = DateTime.UtcNow;

            await _vendorOrderRepository.SaveChangesAsync();

            // Send internal notifications, email, and SignalR live update to customer
            await _internalNotificationService.CreateAsync(
                vendorOrder.MasterOrder.UserId,
                NotificationType.DeliveryDateProposed,
                orderId.ToString()
            );

            // Fetch customer email
            var customer = await _orderRepository
                .Where(o => o.Id == vendorOrder.MasterOrderId)
                .Select(o => o.User)
                .FirstOrDefaultAsync();

            if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
            {
                await _emailService.SendDeliveryDateProposedEmailAsync(
                    customer.Email,
                    orderId,
                    dto.EstimatedDeliveryDate
                );
            }
        }
    }
}
