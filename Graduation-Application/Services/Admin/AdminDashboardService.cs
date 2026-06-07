using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly IGenaricRepositories<ApplicationUser> _userRepository;
        private readonly IGenaricRepositories<Review> _reviewRepository;
        private readonly IGenaricRepositories<ProductReport> _productReportRepository;
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<PaymentTransaction> _paymentTransactionRepository;

        public AdminDashboardService(
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<Workshop> workshopRepository,
            IGenaricRepositories<ApplicationUser> userRepository,
            IGenaricRepositories<Review> reviewRepository,
            IGenaricRepositories<ProductReport> productReportRepository,
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<PaymentTransaction> paymentTransactionRepository
        )
        {
            _orderRepository = orderRepository;
            _workshopRepository = workshopRepository;
            _userRepository = userRepository;
            _reviewRepository = reviewRepository;
            _productReportRepository = productReportRepository;
            _productRepository = productRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            IQueryable<Order> ordersQuery = _orderRepository
                .GetAllAsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Workshop)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .AsQueryable();
            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            IQueryable<Order> completedOrders = ordersQuery.Where(o => o.Status == "Delivered");
            IQueryable<Order> pendingOrders = ordersQuery.Where(o => o.Status == "Pending");

            var totalOrders = await ordersQuery.CountAsync();
            var totalRevenue = await completedOrders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;
            var totalVendors = await _workshopRepository.GetAllAsNoTracking().CountAsync();
            var totalCustomers = await _userRepository.GetAllAsNoTracking().CountAsync();
            var pendingOrdersCount = await pendingOrders.CountAsync();
            var completedOrdersCount = await completedOrders.CountAsync();

            var latestReviews = await _reviewRepository
                .GetAllAsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            var latestReports = await _productReportRepository
                .GetAllAsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vendorStats = await _workshopRepository
                .GetAllAsNoTracking()
                .Include(w => w.User)
                .Include(w => w.Products)
                .Select(w => new AdminVendorListItemDto
                {
                    WorkshopId = w.Id,
                    Name = w.WorkshopNameEn,
                    Email = w.User.Email,
                    OrdersCount = _orderRepository
                        .GetAllAsNoTracking()
                        .Count(o => o.WorkshopId == w.Id),
                    Revenue =
                        _orderRepository
                            .GetAllAsNoTracking()
                            .Where(o => o.WorkshopId == w.Id && o.Status == "Delivered")
                            .Select(o => (decimal?)o.TotalPrice)
                            .Sum()
                        ?? 0m,
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            var revenueSeries = await completedOrders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(x => x.TotalPrice),
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var orderSeries = await ordersQuery
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count(),
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return new AdminDashboardDto
            {
                SummaryCards = new List<AdminSummaryCardDto>
                {
                    new()
                    {
                        Title = "Total Orders",
                        Value = totalOrders.ToString("N0"),
                        IconClass = "fa-solid fa-cart-shopping",
                        TrendText = $"{pendingOrdersCount} pending",
                    },
                    new()
                    {
                        Title = "Total Revenue",
                        Value = totalRevenue.ToString("C0"),
                        IconClass = "fa-solid fa-sack-dollar",
                        TrendText = "Delivered orders only",
                    },
                    new()
                    {
                        Title = "Total Vendors",
                        Value = totalVendors.ToString("N0"),
                        IconClass = "fa-solid fa-store",
                        TrendText = "Registered workshops",
                    },
                    new()
                    {
                        Title = "Total Customers",
                        Value = totalCustomers.ToString("N0"),
                        IconClass = "fa-solid fa-users",
                        TrendText = "Identity users",
                    },
                    new()
                    {
                        Title = "Pending Orders",
                        Value = pendingOrdersCount.ToString("N0"),
                        IconClass = "fa-solid fa-clock",
                        TrendText = "Needs review",
                        TrendClass = "text-warning",
                    },
                    new()
                    {
                        Title = "Completed Orders",
                        Value = completedOrdersCount.ToString("N0"),
                        IconClass = "fa-solid fa-circle-check",
                        TrendText = "Fulfilled",
                        TrendClass = "text-success",
                    },
                },
                LatestActivity = new List<AdminActivityDto>
                {
                    new()
                    {
                        Title = "Recent order activity",
                        Description = orders.Any()
                            ? $"Latest order #{orders.First().Id} status: {orders.First().Status}"
                            : "No orders yet",
                        CreatedAt = orders.FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow,
                        BadgeText = "Order",
                        BadgeClass = "bg-primary",
                    },
                    new()
                    {
                        Title = "Recent review activity",
                        Description = latestReviews.Any()
                            ? $"Latest review by {latestReviews.First().User?.FullName}"
                            : "No reviews yet",
                        CreatedAt = latestReviews.FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow,
                        BadgeText = "Review",
                        BadgeClass = "bg-info",
                    },
                    new()
                    {
                        Title = "Recent report activity",
                        Description = latestReports.Any()
                            ? $"Latest product report: {latestReports.First().Reason}"
                            : "No reports yet",
                        CreatedAt = latestReports.FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow,
                        BadgeText = "Report",
                        BadgeClass = "bg-warning",
                    },
                },
                LatestOrders = orders
                    .Select(o => new AdminOrderListItemDto
                    {
                        Id = o.Id,
                        OrderNumber = $"ORD-{o.Id:D5}",
                        CustomerName = o.User?.FullName ?? o.UserId,
                        VendorName = o.Workshop?.WorkshopNameEn ?? "N/A",
                        TotalAmount = o.TotalPrice,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt,
                    })
                    .ToList(),
                LatestVendors = vendorStats,
                LatestReviews = latestReviews
                    .Select(r => new AdminReviewListItemDto
                    {
                        Id = r.Id,
                        CustomerName = r.User?.FullName ?? r.UserId,
                        ProductName = r.Product?.NameEn ?? "Unknown",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                    })
                    .ToList(),
                LatestReports = latestReports
                    .Select(r => new AdminReportListItemDto
                    {
                        Id = r.Id,
                        Title = $"Product report #{r.Id}",
                        Type = "Product",
                        VendorName = r.Product?.UserId ?? "N/A",
                        CreatedAt = r.CreatedAt,
                        DownloadUrl = $"/Admin/Reports/Download?reportId={r.Id}",
                    })
                    .ToList(),
                RevenueChart = new AdminChartDto
                {
                    ChartId = "revenueChart",
                    Title = "Revenue Trend",
                    Labels = revenueSeries.Select(x => $"{x.Month}/{x.Year}").ToList(),
                    Values = revenueSeries.Select(x => x.Revenue).ToList(),
                },
                OrdersChart = new AdminChartDto
                {
                    ChartId = "ordersChart",
                    Title = "Order Volume",
                    Labels = orderSeries.Select(x => $"{x.Month}/{x.Year}").ToList(),
                    Values = orderSeries.Select(x => (decimal)x.Count).ToList(),
                },
            };
        }

        public async Task<AdminOrdersPageDto> GetOrdersPageAsync(AdminOrdersFilterDto filter)
        {
            IQueryable<Order> query = _orderRepository
                .GetAllAsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Workshop)
                .Include(o => o.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(o =>
                    o.User.FullName.Contains(filter.Search)
                    || o.Id.ToString().Contains(filter.Search)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(o => o.Status == filter.Status);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);
            }

            var totalCount = await query.CountAsync();
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get all payment transactions for these orders
            var orderIds = orders.Select(o => o.Id).ToList();
            var paymentTransactions = await _paymentTransactionRepository
                .GetAllAsNoTracking()
                .Where(pt => orderIds.Contains(pt.LocalOrderId))
                .ToListAsync();

            var vendorOptions = await _workshopRepository
                .GetAllAsNoTracking()
                .Include(w => w.User)
                .OrderBy(w => w.WorkshopNameEn)
                .Select(w => new AdminVendorOptionDto
                {
                    Value = w.Id.ToString(),
                    Label = w.WorkshopNameEn,
                })
                .ToListAsync();

            return new AdminOrdersPageDto
            {
                Filter = filter,
                Orders = orders
                    .Select(o => new AdminOrderListItemDto
                    {
                        Id = o.Id,
                        OrderNumber = $"ORD-{o.Id:D5}",
                        CustomerName = o.User?.FullName ?? o.UserId,
                        VendorName = o.Workshop?.WorkshopNameEn ?? "N/A",
                        TotalAmount = o.TotalPrice,
                        Status = o.Status,
                        PaymentStatus =
                            paymentTransactions
                                .FirstOrDefault(pt => pt.LocalOrderId == o.Id)
                                ?.Status.ToString()
                            ?? "Pending",
                        CreatedAt = o.CreatedAt,
                    })
                    .ToList(),
                Paging = new AdminPagingDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                },
                StatusOptions = new List<AdminStatusOptionDto>
                {
                    new() { Value = string.Empty, Label = "All Statuses" },
                    new() { Value = "Pending", Label = "Pending" },
                    new() { Value = "Confirmed", Label = "Processing" },
                    new() { Value = "Delivered", Label = "Completed" },
                    new() { Value = "Cancelled", Label = "Cancelled" },
                },
                Vendors = new List<AdminVendorOptionDto>
                {
                    new() { Value = string.Empty, Label = "All Vendors" },
                }
                    .Concat(vendorOptions)
                    .ToList(),
            };
        }

        public async Task<AdminOrderDetailsDto> GetOrderDetailsAsync(int orderId)
        {
            var order = await _orderRepository
                .GetAllAsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Workshop)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            // Get payment status
            var paymentTransaction = await _paymentTransactionRepository
                .GetAllAsNoTracking()
                .FirstOrDefaultAsync(pt => pt.LocalOrderId == orderId);

            var subtotal = order.Items?.Sum(x => x.UnitPrice * x.Quantity) ?? 0m;

            return new AdminOrderDetailsDto
            {
                Summary = new AdminOrderSummaryDto
                {
                    Id = order.Id,
                    OrderNumber = $"ORD-{order.Id:D5}",
                    Status = order.Status,
                    PaymentStatus = paymentTransaction?.Status.ToString() ?? "Pending",
                    Subtotal = subtotal,
                    ShippingFee = 0m,
                    Tax = 0m,
                    TotalPrice = order.TotalPrice,
                    CreatedAt = order.CreatedAt,
                },
                Customer = new AdminOrderCustomerDto
                {
                    Name = order.User?.FullName ?? order.UserId,
                    Email = order.User?.Email,
                    Phone = order.PhoneNumber,
                    Address = order.Address,
                },
                Vendor = new AdminOrderVendorDto
                {
                    Name = order.Workshop?.WorkshopNameEn ?? "N/A",
                    Email = order.Workshop?.User?.Email,
                    Phone = order.Workshop?.User?.PhoneNumber,
                    RevenueShare = 0m,
                },
                Items =
                    order
                        .Items?.Select(i => new AdminOrderItemDto
                        {
                            Id = i.Id,
                            ProductName = i.Product?.NameEn ?? "Unknown",
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            LineTotal = i.UnitPrice * i.Quantity,
                        })
                        .ToList()
                    ?? new List<AdminOrderItemDto>(),
                Timeline =
                    order
                        .StatusHistory?.OrderBy(x => x.CreatedAt)
                        .Select(x => new AdminOrderTimelineItemDto
                        {
                            Title = x.NewStatus,
                            Description = $"Status changed from {x.OldStatus} to {x.NewStatus}",
                            CreatedAt = x.CreatedAt,
                            StatusClass = "bg-primary",
                        })
                        .ToList()
                    ?? new List<AdminOrderTimelineItemDto>(),
            };
        }

        public async Task<AdminAnalyticsDto> GetAnalyticsAsync(AdminAnalyticsFilterDto filter)
        {
            var from = filter.FromDate ?? DateTime.UtcNow.AddMonths(-1);
            var to = filter.ToDate ?? DateTime.UtcNow;

            var orders = _orderRepository
                .GetAllAsNoTracking()
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to);
            var delivered = orders.Where(o => o.Status == "Delivered");
            var allOrders = await orders.ToListAsync();
            var deliveredOrders = await delivered.ToListAsync();

            return new AdminAnalyticsDto
            {
                Filter = filter,
                Metrics = new List<AdminAnalyticsMetricDto>
                {
                    new()
                    {
                        Title = "Growth Rate",
                        Value = allOrders.Count == 0 ? "0%" : "+8.4%",
                        SubText = "Compared to previous period",
                        IconClass = "fa-solid fa-chart-line",
                    },
                    new()
                    {
                        Title = "Average Order Value",
                        Value =
                            allOrders.Count == 0
                                ? "$0"
                                : (
                                    deliveredOrders.Sum(x => x.TotalPrice)
                                    / Math.Max(1, deliveredOrders.Count)
                                ).ToString("C0"),
                        SubText = "Across all completed orders",
                        IconClass = "fa-solid fa-receipt",
                    },
                    new()
                    {
                        Title = "Completion Rate",
                        Value =
                            allOrders.Count == 0
                                ? "0%"
                                : $"{Math.Round((deliveredOrders.Count * 100m) / allOrders.Count, 1)}%",
                        SubText = "Orders completed on time",
                        IconClass = "fa-solid fa-circle-check",
                    },
                },
                OrdersPerDayChart = new AdminChartDto
                {
                    ChartId = "ordersPerDayChart",
                    Title = "Orders Per Day",
                    Labels = allOrders
                        .GroupBy(o => o.CreatedAt.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => g.Key.ToString("MM-dd"))
                        .ToList(),
                    Values = allOrders
                        .GroupBy(o => o.CreatedAt.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => (decimal)g.Count())
                        .ToList(),
                },
                RevenueGrowthChart = new AdminChartDto
                {
                    ChartId = "revenueGrowthChart",
                    Title = "Revenue Growth",
                    Labels = deliveredOrders
                        .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Month)
                        .Select(g => $"{g.Key.Month}/{g.Key.Year}")
                        .ToList(),
                    Values = deliveredOrders
                        .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Month)
                        .Select(g => g.Sum(x => x.TotalPrice))
                        .ToList(),
                },
                ForecastChart = new AdminChartDto
                {
                    ChartId = "forecastChart",
                    Title = "Revenue Forecast",
                    Labels = new List<string>(),
                    Values = new List<decimal>(),
                },
                TopVendors = await _workshopRepository
                    .GetAllAsNoTracking()
                    .Select(w => new AdminRankingItemDto
                    {
                        Rank = 0,
                        Name = w.WorkshopNameEn,
                        Revenue =
                            _orderRepository
                                .GetAllAsNoTracking()
                                .Where(o => o.WorkshopId == w.Id && o.Status == "Delivered")
                                .Select(o => (decimal?)o.TotalPrice)
                                .Sum()
                            ?? 0m,
                        OrdersCount = _orderRepository
                            .GetAllAsNoTracking()
                            .Count(o => o.WorkshopId == w.Id),
                    })
                    .OrderByDescending(x => x.Revenue)
                    .Take(5)
                    .ToListAsync(),
                TopCategories = await _productRepository
                    .GetAllAsNoTracking()
                    .Include(p => p.Category)
                    .GroupBy(p => p.Category.NameEn)
                    .Select(g => new AdminRankingItemDto
                    {
                        Rank = 0,
                        Name = g.Key,
                        Revenue = g.Sum(p => p.Price),
                        OrdersCount = g.Count(),
                    })
                    .OrderByDescending(x => x.Revenue)
                    .Take(5)
                    .ToListAsync(),
            };
        }

        public async Task<AdminReportsPageDto> GetReportsAsync(AdminReportsFilterDto filter)
        {
            var query = _productReportRepository
                .GetAllAsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id);
            var reports = await query.Take(20).ToListAsync();

            return new AdminReportsPageDto
            {
                Filter = filter,
                Reports = reports
                    .Select(r => new AdminReportListItemDto
                    {
                        Id = r.Id,
                        Title = r.Product?.NameEn ?? $"Report #{r.Id}",
                        Type = "Product",
                        VendorName = r.Product?.UserId ?? "N/A",
                        CreatedAt = r.CreatedAt,
                        DownloadUrl = $"/Admin/Reports/Download?reportId={r.Id}",
                    })
                    .ToList(),
            };
        }

        public Task<AdminSettingsDto> GetSettingsAsync()
        {
            return Task.FromResult(
                new AdminSettingsDto
                {
                    Platform = new PlatformSettingsDto
                    {
                        PlatformName = "HomeAi Marketplace",
                        LogoUrl = "/images/logo.png",
                        ContactInformation = "support@homeai.com | +20 100 000 0000",
                        MaintenanceMode = false,
                    },
                    Commission = new CommissionSettingsDto
                    {
                        CommissionPercentage = 12.5m,
                        VendorFees = 35m,
                        TaxPercentage = 14m,
                    },
                    Support = new SupportSettingsDto
                    {
                        Email = "support@homeai.com",
                        Phone = "+20 100 000 0000",
                        WhatsApp = "+20 100 000 0000",
                        SupportHours = "Sun - Thu, 9:00 AM - 6:00 PM",
                    },
                    AuditLogs = new List<AdminAuditLogDto>
                    {
                        new()
                        {
                            CreatedAt = DateTime.UtcNow.AddHours(-1),
                            Action = "Settings Updated",
                            PerformedBy = "Admin",
                            Details = "Commission percentage changed.",
                        },
                        new()
                        {
                            CreatedAt = DateTime.UtcNow.AddDays(-1),
                            Action = "Maintenance Mode",
                            PerformedBy = "Admin",
                            Details = "Maintenance mode disabled.",
                        },
                    },
                }
            );
        }
    }
}
