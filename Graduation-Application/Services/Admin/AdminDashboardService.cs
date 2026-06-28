using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminDashboardDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IGenaricRepositories<VendorOrder> _vendorOrderRepository;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly IGenaricRepositories<ApplicationUser> _userRepository;
        private readonly IGenaricRepositories<Review> _reviewRepository;
        private readonly IGenaricRepositories<ProductReport> _productReportRepository;
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<PaymentTransaction> _paymentTransactionRepository;

        public AdminDashboardService(
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<VendorOrder> vendorOrderRepository,
            IGenaricRepositories<Workshop> workshopRepository,
            IGenaricRepositories<ApplicationUser> userRepository,
            IGenaricRepositories<Review> reviewRepository,
            IGenaricRepositories<ProductReport> productReportRepository,
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<PaymentTransaction> paymentTransactionRepository
        )
        {
            _orderRepository = orderRepository;
            _vendorOrderRepository = vendorOrderRepository;
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
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Workshop)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.StatusHistory)
                .AsQueryable();
            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
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

            var vendorStats = await _workshopRepository
                .GetAllAsNoTracking()
                .Include(w => w.User)
                .Include(w => w.Products)
                .Select(w => new AdminVendorListItemDto
                {
                    WorkshopId = w.Id,
                    Name = w.WorkshopNameEn,
                    NameAr = w.WorkshopNameAr,
                    Email = w.User.Email,
                    OrdersCount = _vendorOrderRepository
                        .GetAllAsNoTracking()
                        .Count(vo => vo.WorkshopId == w.Id),
                    Revenue =
                        _vendorOrderRepository
                            .GetAllAsNoTracking()
                            .Where(vo =>
                                vo.WorkshopId == w.Id && vo.Status == VendorOrderStatus.Delivered
                            )
                            .Select(vo => (decimal?)vo.TotalPrice)
                            .Sum()
                        ?? 0m,
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            var last6Months = Enumerable
                .Range(0, 6)
                .Select(i => DateTime.UtcNow.AddMonths(-5 + i)) // generate last 6 months in chronological order
                .Select(d => new { d.Year, d.Month })
                .ToList();
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            var revenueSeriesDb = await completedOrders
                .Where(o => o.CreatedAt >= sixMonthsAgo)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(x => x.TotalPrice),
                })
                .ToListAsync();

            var finalRevenueSeries = last6Months
                .Select(m => new
                {
                    m.Year,
                    m.Month,
                    Revenue = revenueSeriesDb
                        .FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)
                        ?.Revenue
                        ?? 0m,
                })
                .ToList();

            var orderSeriesDb = await ordersQuery
                .Where(o => o.CreatedAt >= sixMonthsAgo)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count(),
                })
                .ToListAsync();

            var finalOrderSeries = last6Months
                .Select(m => new
                {
                    m.Year,
                    m.Month,
                    Count = orderSeriesDb
                        .FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)
                        ?.Count
                        ?? 0,
                })
                .ToList();

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
                },
                LatestOrders = orders
                    .Select(o => new AdminOrderListItemDto
                    {
                        Id = o.Id,
                        OrderNumber = $"ORD-{o.Id:D5}",
                        CustomerName = o.User?.FullName ?? o.UserId,
                        VendorName =
                            o.VendorOrders != null
                                ? string.Join(
                                    ", ",
                                    o.VendorOrders.Select(vo => vo.Workshop?.WorkshopNameEn)
                                        .Where(name => !string.IsNullOrEmpty(name))
                                )
                                : "N/A",
                        VendorNameAr =
                            o.VendorOrders != null
                                ? string.Join(
                                    "، ",
                                    o.VendorOrders.Select(vo => vo.Workshop?.WorkshopNameAr)
                                        .Where(name => !string.IsNullOrEmpty(name))
                                )
                                : "غير متاح",
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
                RevenueChart = new AdminChartDto
                {
                    ChartId = "revenueChart",
                    Title = "Revenue Trend",
                    Labels = finalRevenueSeries.Select(x => $"{x.Month}/{x.Year}").ToList(),
                    Values = finalRevenueSeries.Select(x => x.Revenue).ToList(),
                },
                OrdersChart = new AdminChartDto
                {
                    ChartId = "ordersChart",
                    Title = "Order Volume",
                    Labels = finalOrderSeries.Select(x => $"{x.Month}/{x.Year}").ToList(),
                    Values = finalOrderSeries.Select(x => (decimal)x.Count).ToList(),
                },
            };
        }

        public async Task<AdminOrdersPageDto> GetOrdersPageAsync(AdminOrdersFilterDto filter)
        {
            IQueryable<Order> query = _orderRepository
                .GetAllAsNoTracking()
                .Include(o => o.User)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Workshop)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
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

            var vendorOptions = await _workshopRepository
                .GetAllAsNoTracking()
                .Include(w => w.User)
                .OrderBy(w => w.WorkshopNameEn)
                .Select(w => new AdminVendorOptionDto
                {
                    Value = w.Id.ToString(),
                    Label = w.WorkshopNameEn,
                    NameAr = w.WorkshopNameAr,
                    NameEn = w.WorkshopNameEn,
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
                        VendorName =
                            o.VendorOrders != null
                                ? string.Join(
                                    ", ",
                                    o.VendorOrders.Select(vo => vo.Workshop?.WorkshopNameEn)
                                        .Where(name => !string.IsNullOrEmpty(name))
                                )
                                : "N/A",
                        VendorNameAr =
                            o.VendorOrders != null
                                ? string.Join(
                                    "، ",
                                    o.VendorOrders.Select(vo => vo.Workshop?.WorkshopNameAr)
                                        .Where(name => !string.IsNullOrEmpty(name))
                                )
                                : "غير متاح",
                        TotalAmount = o.TotalPrice,
                        Status = o.Status,
                        PaymentStatus = o.PaymentStatus,
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
                    new() { Value = "Confirmed", Label = "Confirmed" },
                    new() { Value = "Processing", Label = "Processing" },
                    new() { Value = "PartiallyDelivered", Label = "Partially Delivered" },
                    new() { Value = "Ready for Pickup", Label = "Ready for Pickup" },
                    new() { Value = "Delivered", Label = "Delivered" },
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
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Workshop)
                        .ThenInclude(w => w.User)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.Items)
                .Include(o => o.VendorOrders)
                    .ThenInclude(vo => vo.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }


            var subtotal =
                order
                    .VendorOrders?.SelectMany(vo => vo.Items ?? new List<OrderItem>())
                    .Sum(x => x.SnapshotUnitPrice * x.Quantity)
                ?? 0m;

            return new AdminOrderDetailsDto
            {
                Summary = new AdminOrderSummaryDto
                {
                    Id = order.Id,
                    OrderNumber = $"ORD-{order.Id:D5}",
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus,
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
                    Name =
                        order.VendorOrders != null
                            ? string.Join(
                                ", ",
                                order
                                    .VendorOrders.Select(vo => vo.Workshop?.WorkshopNameEn)
                                    .Where(name => !string.IsNullOrEmpty(name))
                            )
                            : "N/A",
                    NameAr =
                        order.VendorOrders != null
                            ? string.Join(
                                "، ",
                                order
                                    .VendorOrders.Select(vo => vo.Workshop?.WorkshopNameAr)
                                    .Where(name => !string.IsNullOrEmpty(name))
                            )
                            : "غير متاح",
                    Email =
                        order.VendorOrders != null
                            ? string.Join(
                                ", ",
                                order
                                    .VendorOrders.Select(vo => vo.Workshop?.User?.Email)
                                    .Where(email => !string.IsNullOrEmpty(email))
                            )
                            : "",
                    Phone =
                        order.VendorOrders != null
                            ? string.Join(
                                ", ",
                                order
                                    .VendorOrders.Select(vo => vo.Workshop?.User?.PhoneNumber)
                                    .Where(phone => !string.IsNullOrEmpty(phone))
                            )
                            : "",
                    RevenueShare = 0m,
                },
                Items =
                    order.VendorOrders != null
                        ? order
                            .VendorOrders.SelectMany(vo => vo.Items ?? new List<OrderItem>())
                            .Select(i => new AdminOrderItemDto
                            {
                                Id = i.Id,
                                ProductName = i.SnapshotProductNameEn ?? "Unknown",
                                Quantity = i.Quantity,
                                UnitPrice = i.SnapshotUnitPrice,
                                LineTotal = i.SnapshotUnitPrice * i.Quantity,
                            })
                            .ToList()
                        : new List<AdminOrderItemDto>(),
                Timeline =
                    order.VendorOrders != null
                        ? order
                            .VendorOrders.SelectMany(vo =>
                                vo.StatusHistory ?? new List<VendorOrderStatusHistory>()
                            )
                            .OrderBy(x => x.CreatedAt)
                            .Select(x => new AdminOrderTimelineItemDto
                            {
                                Title = x.NewStatus,
                                Description = $"Status changed from {x.OldStatus} to {x.NewStatus}",
                                CreatedAt = x.CreatedAt,
                                StatusClass = "bg-primary",
                            })
                            .ToList()
                        : new List<AdminOrderTimelineItemDto>(),
            };
        }

        public async Task<AdminAnalyticsDto> GetAnalyticsAsync(AdminAnalyticsFilterDto filter)
        {
            var from = filter.FromDate ?? DateTime.UtcNow.AddMonths(-4);
            var to = filter.ToDate ?? DateTime.UtcNow;

            var orders = _orderRepository
                .GetAllAsNoTracking()
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to);
            var delivered = orders.Where(o => o.Status == "Delivered");
            var allOrders = await orders.ToListAsync();
            var deliveredOrders = await delivered.ToListAsync();

            var duration = to - from;
            var previousFrom = from - duration;
            var previousTo = from;

            var previousOrdersCount = await _orderRepository
                .GetAllAsNoTracking()
                .CountAsync(o => o.CreatedAt >= previousFrom && o.CreatedAt < previousTo);

            var currentOrdersCount = allOrders.Count;
            
            string growthRateValue = "0%";
            string trendClass = "text-muted";
            if (previousOrdersCount == 0 && currentOrdersCount > 0)
            {
                growthRateValue = "+100%";
                trendClass = "text-success";
            }
            else if (previousOrdersCount > 0)
            {
                var growth = ((currentOrdersCount - previousOrdersCount) * 100m) / previousOrdersCount;
                growthRateValue = growth > 0 ? $"+{Math.Round(growth, 1)}%" : $"{Math.Round(growth, 1)}%";
                trendClass = growth > 0 ? "text-success" : (growth < 0 ? "text-danger" : "text-muted");
            }

            var monthsCount = ((to.Year - from.Year) * 12) + to.Month - from.Month + 1;
            if (monthsCount <= 0) monthsCount = 1;

            var monthRange = Enumerable.Range(0, monthsCount)
                .Select(i => to.AddMonths(-i))
                .Select(d => new { d.Year, d.Month })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            var revenueLabels = monthRange.Select(m => $"{m.Month}/{m.Year}").ToList();
            var revenueValues = monthRange.Select(m =>
                deliveredOrders.Where(o => o.CreatedAt.Year == m.Year && o.CreatedAt.Month == m.Month).Sum(x => x.TotalPrice)
            ).ToList();

            return new AdminAnalyticsDto
            {
                Filter = filter,
                Metrics = new List<AdminAnalyticsMetricDto>
                {
                    new()
                    {
                        Title = "Growth Rate",
                        Value = growthRateValue,
                        SubText = "Compared to previous period",
                        IconClass = "fa-solid fa-chart-line",
                        TrendClass = trendClass,
                    },
                    new()
                    {
                        Title = "Average Order Value",
                        Value =
                            allOrders.Count == 0
                                ? "0"
                                : (
                                    deliveredOrders.Sum(x => x.TotalPrice)
                                    / Math.Max(1, deliveredOrders.Count)
                                ).ToString("N0"),
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
                    Labels = revenueLabels,
                    Values = revenueValues,
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
                            _vendorOrderRepository
                                .GetAllAsNoTracking()
                                .Where(vo =>
                                    vo.WorkshopId == w.Id
                                    && vo.Status == VendorOrderStatus.Delivered
                                )
                                .Select(vo => (decimal?)vo.TotalPrice)
                                .Sum()
                            ?? 0m,
                        OrdersCount = _vendorOrderRepository
                            .GetAllAsNoTracking()
                            .Count(vo => vo.WorkshopId == w.Id),
                    })
                    .OrderByDescending(x => x.Revenue)
                    .Take(5)
                    .ToListAsync(),
                TopCategories = await _productRepository
                    .GetAllAsNoTracking()
                    .Include(p => p.ProductType)
                        .ThenInclude(pt => pt.SubCategory)
                            .ThenInclude(sc => sc.Category)
                    .GroupBy(p =>
                        p.ProductType != null
                        && p.ProductType.SubCategory != null
                        && p.ProductType.SubCategory.Category != null
                            ? p.ProductType.SubCategory.Category.NameEn
                            : "Unknown"
                    )
                    .Select(g => new AdminRankingItemDto
                    {
                        Rank = 0,
                        Name = g.Key,
                        // Revenue from products not directly calculable — use 0 placeholder
                        // TODO: join with OrderItem.SnapshotUnitPrice grouped by category
                        Revenue = 0m,
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
                        VendorName = r.Product?.Workshop?.WorkshopNameEn ?? "N/A",
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
