using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Enums;
using Graduation_domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class VendorOrderService : IVendorOrderService
    {
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly INotificationService _notificationService;
        private readonly IInternalNotificationService _internalNotificationService;

        public VendorOrderService(
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<Workshop> workshopRepository,
            INotificationService notificationService,
            IInternalNotificationService internalNotificationService
        )
        {
            _orderRepository = orderRepository;
            _workshopRepository = workshopRepository;
            _notificationService = notificationService;
            _internalNotificationService = internalNotificationService;
        }

        // 1. Get Vendor Orders with Filtering and Pagination
        public async Task<(
            List<VendorOrderDashboardDto> Orders,
            int TotalCount
        )> GetVendorOrdersAsync(int workshopId, VendorOrdersFilterDto filter)
        {
            var query = _orderRepository.Where(o => o.WorkshopId == workshopId).AsNoTracking();

            // Status Filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(o => o.Status == filter.Status);
            }

            // Date Range Filter
            if (filter.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                var endDate = filter.EndDate.Value.Date.AddDays(1);

                query = query.Where(o => o.CreatedAt < endDate);
            }

            // Customer Name Filter
            if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            {
                query = query.Where(o =>
                    EF.Functions.Like(o.User.FullName, $"%{filter.CustomerName}%")
                );
            }

            // Total count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            switch (filter.SortBy?.ToLower())
            {
                case "totalprice":
                    query = filter.SortDescending
                        ? query.OrderByDescending(o => o.TotalPrice)
                        : query.OrderBy(o => o.TotalPrice);
                    break;

                case "status":
                    query = filter.SortDescending
                        ? query.OrderByDescending(o => o.Status)
                        : query.OrderBy(o => o.Status);
                    break;

                case "customername":
                    query = filter.SortDescending
                        ? query.OrderByDescending(o => o.User.FullName)
                        : query.OrderBy(o => o.User.FullName);
                    break;

                default:
                    query = filter.SortDescending
                        ? query.OrderByDescending(o => o.CreatedAt)
                        : query.OrderBy(o => o.CreatedAt);
                    break;
            }

            // Pagination + Projection
            var orders = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new VendorOrderDashboardDto
                {
                    Id = o.Id,
                    CustomerName = o.User.FullName,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    ItemCount = o.Items.Count(),
                    CustomerPhone = o.PhoneNumber,
                    Address = o.Address,
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
            var order = await _orderRepository
                .Where(o => o.Id == orderId && o.WorkshopId == workshopId)
                .Include(o => o.User)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found or unauthorized");
            }

            return new VendorOrderDetailsDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.User.FullName,
                CustomerPhone = order.PhoneNumber,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                Address = order.Address,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order
                    .Items.Select(oi => new VendorOrderItemDto
                    {
                        ProductId = oi.ProductId ?? 0,
                        ProductName = oi.SnapshotProductNameEn,
                        UnitPrice = oi.SnapshotUnitPrice,
                        Quantity = oi.Quantity,
                        Total = oi.SnapshotUnitPrice * oi.Quantity,
                    })
                    .ToList(),
                StatusHistory = order
                    .StatusHistory.Select(sh => new OrderStatusHistoryResponseDto
                    {
                        Id = sh.Id,
                        OldStatus = sh.OldStatus,
                        NewStatus = sh.NewStatus,
                        CreatedAt = sh.CreatedAt,
                    })
                    .OrderByDescending(s => s.CreatedAt)
                    .ToList(),
            };
        }

        // 3. Update Vendor Order Status
        public async Task UpdateVendorOrderStatusAsync(
            int orderId,
            int workshopId,
            string newStatus
        )
        {
            var validStatuses = new[]
            {
                "Pending",
                "Confirmed",
                "In Progress",
                "Ready for Pickup",
                "Delivered",
                "Cancelled",
            };

            if (!validStatuses.Contains(newStatus))
            {
                throw new Exception("Invalid status");
            }

            var order = await _orderRepository
                .Where(o => o.Id == orderId && o.WorkshopId == workshopId)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found or unauthorized");
            }

            // Validate status transition
            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                throw new Exception($"Cannot transition from {order.Status} to {newStatus}");
            }

            var oldStatus = order.Status;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // Add to status history
            order.StatusHistory.Add(
                new OrderStatusHistory
                {
                    OrderId = orderId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                }
            );

            order.UpdatedAt = DateTime.UtcNow;
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            // Send internal notifications based on status
            switch (newStatus)
            {
                case "Confirmed":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderConfirmed,
                        orderId.ToString()
                    );
                    break;
                case "In Progress":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderInProgress,
                        orderId.ToString()
                    );
                    break;
                case "Ready for Pickup":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderReadyForPickup,
                        orderId.ToString()
                    );
                    break;
                case "Delivered":
                    await _internalNotificationService.CreateAsync(
                        order.UserId,
                        NotificationType.OrderDelivered,
                        orderId.ToString()
                    );
                    break;
            }

            // Send notification to customer
            await _notificationService.SendOrderStatusUpdateAsync(order.UserId, orderId, newStatus);
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

            var allOrdersInRangeQuery = _orderRepository
                .Where(o =>
                    o.WorkshopId == workshopId && o.CreatedAt >= startDate && o.CreatedAt <= endDate
                )
                .AsQueryable();

            var deliveredInRangeQuery = allOrdersInRangeQuery.Where(o => o.Status == "Delivered");

            var deliveredAllTimeQuery = _orderRepository
                .Where(o => o.WorkshopId == workshopId && o.Status == "Delivered")
                .AsQueryable();

            var totalRevenue = await deliveredInRangeQuery.SumAsync(o => o.TotalPrice);

            var monthlyRevenue = await deliveredAllTimeQuery
                .Where(o => o.CreatedAt >= monthStart && o.CreatedAt <= now)
                .SumAsync(o => o.TotalPrice);

            var weeklyRevenue = await deliveredAllTimeQuery
                .Where(o => o.CreatedAt >= weekStart && o.CreatedAt <= now)
                .SumAsync(o => o.TotalPrice);

            var dailyRevenue = await deliveredAllTimeQuery
                .Where(o => o.CreatedAt >= dayStart && o.CreatedAt <= now)
                .SumAsync(o => o.TotalPrice);

            var completedOrdersCount = await deliveredInRangeQuery.CountAsync();

            var dailyBreakdown = await deliveredInRangeQuery
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice),
                    OrdersCount = g.Count(),
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var ordersByStatus = await allOrdersInRangeQuery
                .GroupBy(o => o.Status)
                .Select(g => new OrdersByStatusDto { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var monthlyBreakdown = await deliveredInRangeQuery
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
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

            var query = _orderRepository.Where(o => o.WorkshopId == workshopId).AsQueryable();

            var ordersInRange = await query
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .Include(o => o.StatusHistory)
                .ToListAsync();

            var totalOrders = ordersInRange.Count;
            var completedOrders = ordersInRange.Count(o => o.Status == "Delivered");
            var cancelledOrders = ordersInRange.Count(o => o.Status == "Cancelled");
            var pendingOrders = ordersInRange.Count(o => o.Status == "Pending");
            var inProgressOrders = ordersInRange.Count(o =>
                o.Status == "Confirmed"
                || o.Status == "In Progress"
                || o.Status == "Ready for Pickup"
            );

            var totalRevenue = ordersInRange
                .Where(o => o.Status == "Delivered")
                .Sum(o => o.TotalPrice);

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

            var query = _orderRepository.Where(o => o.WorkshopId == workshopId);

            var currentMonthOrders = await query
                .Where(o => o.CreatedAt >= monthStart && o.CreatedAt <= now)
                .ToListAsync();

            var lastMonthOrders = await query
                .Where(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < monthStart)
                .ToListAsync();

            var totalOrders = await query.CountAsync();
            var activeOrders = await query
                .Where(o => o.Status != "Delivered" && o.Status != "Cancelled")
                .CountAsync();
            var completedOrders = await query.Where(o => o.Status == "Delivered").CountAsync();

            var currentRevenue = currentMonthOrders
                .Where(o => o.Status == "Delivered")
                .Sum(o => o.TotalPrice);

            var lastRevenue = lastMonthOrders
                .Where(o => o.Status == "Delivered")
                .Sum(o => o.TotalPrice);

            var orderGrowth =
                lastRevenue > 0 ? ((currentRevenue - lastRevenue) / lastRevenue) * 100 : 0;

            var allOrders = await query.ToListAsync();
            var averageOrderValue =
                allOrders.Count > 0
                    ? allOrders.Where(o => o.Status == "Delivered").Sum(o => o.TotalPrice)
                        / allOrders.Count
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
            var orders = await _orderRepository
                .Where(o =>
                    o.WorkshopId == workshopId
                    && o.CreatedAt >= startDate
                    && o.CreatedAt <= endDate.AddDays(1)
                )
                .Include(o => o.User)
                .ToListAsync();

            var completedOrders = orders.Count(o => o.Status == "Delivered");
            var cancelledOrders = orders.Count(o => o.Status == "Cancelled");
            var totalRevenue = orders.Where(o => o.Status == "Delivered").Sum(o => o.TotalPrice);

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
                    .Select(o => new OrderActivityDto
                    {
                        OrderId = o.Id,
                        CustomerName = o.User.FullName,
                        Status = o.Status,
                        Amount = o.TotalPrice,
                        CreatedAt = o.CreatedAt,
                    })
                    .ToList(),
            };
        }

        // Helper methods
        private bool IsValidStatusTransition(string fromStatus, string toStatus)
        {
            var validTransitions = new Dictionary<string, List<string>>
            {
                {
                    "Pending",
                    new List<string> { "Confirmed", "Cancelled" }
                },
                {
                    "Confirmed",
                    new List<string> { "In Progress", "Cancelled" }
                },
                {
                    "In Progress",
                    new List<string> { "Ready for Pickup", "Cancelled" }
                },
                {
                    "Ready for Pickup",
                    new List<string> { "Delivered", "Cancelled" }
                },
                { "Delivered", new List<string>() },
                { "Cancelled", new List<string>() },
            };

            return validTransitions.ContainsKey(fromStatus)
                && validTransitions[fromStatus].Contains(toStatus);
        }

        private double CalculateAverageCompletionTime(List<Order> orders)
        {
            var completedOrders = orders
                .Where(o => o.Status == "Delivered" && o.StatusHistory.Any())
                .ToList();

            if (!completedOrders.Any())
                return 0;

            var totalHours = completedOrders.Sum(o =>
            {
                var createdAt = o.CreatedAt;
                var deliveredAt = o
                    .StatusHistory.Where(sh => sh.NewStatus == "Delivered")
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
            var userIds = await _orderRepository
                .Where(o => o.WorkshopId == workshopId && o.CreatedAt >= monthStart)
                .Select(o => o.UserId)
                .Distinct()
                .CountAsync();

            return userIds;
        }
    }
}
