using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Graduation_domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IGenaricRepositories<OrderItem> _orderItemRepository;
        private readonly IGenaricRepositories<Cart> _cartRepository;
        private readonly IGenaricRepositories<CartItem> _cartItemRepository;
        private readonly ICartService _cartService;
        private readonly INotificationService _notificationService;
        private readonly IInternalNotificationService _internalNotificationService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderService(
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<OrderItem> orderItemRepository,
            IGenaricRepositories<Cart> cartRepository,
            IGenaricRepositories<CartItem> cartItemRepository,
            ICartService cartService,
            INotificationService notificationService,
            IInternalNotificationService internalNotificationService,
            IPaymentGateway paymentGateway,
            IPaymentTransactionRepository paymentTransactionRepository,
            UserManager<ApplicationUser> userManager
        )
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _cartService = cartService;
            _notificationService = notificationService;
            _internalNotificationService = internalNotificationService;
            _paymentGateway = paymentGateway;
            _paymentTransactionRepository = paymentTransactionRepository;
            _userManager = userManager;
        }

        public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository
                .Where(o => o.Status != "Cancelled")
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var mappedOrders = new List<OrderResponseDto>();
            foreach (var order in orders)
            {
                mappedOrders.Add(await MapToDtoAsync(order));
            }
            return mappedOrders;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(
            string userId,
            CreateOrderDto request,
            ClaimsPrincipal user
        )
        {
            // Read cart WITHOUT tracking to avoid EF tracking collisions
            var cart = await _cartRepository
                .WhereAsNoTracking(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Listing)
                            .ThenInclude(l => l.Product)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Listing)
                            .ThenInclude(l => l.Workshop)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.VariantAttributeValues)
                            .ThenInclude(vav => vav.AttributeValue)
                                .ThenInclude(av => av.Attribute)
                .FirstOrDefaultAsync();

            if (cart == null || !cart.Items.Any())
            {
                throw new Exception("Cart is empty");
            }

            decimal totalPrice = cart.Items.Sum(ci => ci.CachedPrice * ci.Quantity);

            var phoneNumber = request.PhoneNumber ?? user.FindFirst(ClaimTypes.MobilePhone)?.Value;

            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                Status = "Pending",
                Address = request.Address,
                PhoneNumber = phoneNumber,
                Notes = request.Notes,
                // Build immutable snapshot items from cart items
                Items = cart.Items.Select(ci =>
                {
                    var variant = ci.ProductVariant;
                    var listing = variant?.Listing;
                    var product = listing?.Product;
                    var workshop = listing?.Workshop;

                    // Serialize chosen attributes to JSON
                    var attrs = variant?.VariantAttributeValues
                        ?.Select(vav => new
                        {
                            nameEn = vav.AttributeValue?.Attribute?.NameEn ?? string.Empty,
                            nameAr = vav.AttributeValue?.Attribute?.NameAr ?? string.Empty,
                            valueEn = vav.AttributeValue?.ValueEn ?? string.Empty,
                            valueAr = vav.AttributeValue?.ValueAr ?? string.Empty,
                        })
                        .ToList();
                    string attrsJson = attrs != null
                        ? System.Text.Json.JsonSerializer.Serialize(attrs)
                        : "[]";

                    return new OrderItem
                    {
                        ProductVariantId = ci.ProductVariantId,
                        Quantity = ci.Quantity,
                        SnapshotUnitPrice = ci.CachedPrice,
                        SnapshotProductNameEn = product?.NameEn ?? string.Empty,
                        SnapshotProductNameAr = product?.NameAr ?? string.Empty,
                        SnapshotVendorName = workshop?.WorkshopNameEn ?? string.Empty,
                        SnapshotAttributesJson = attrsJson,
                    };
                }).ToList(),
                StatusHistory = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        OrderId = 0,
                        OldStatus = "",
                        NewStatus = "Pending",
                    },
                },
            };

            // Add only the order (EF will track the order + its new items)
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Clear the cart (this uses the cart service which will operate with its own tracked entities)
            await _cartService.ClearCartAsync(userId);

            // Send internal notifications
            await _internalNotificationService.CreateAsync(
                userId,
                NotificationType.OrderPending,
                order.Id.ToString()
            );
            // Notify vendor via the first order item's workshop
            var firstItem = order.Items.FirstOrDefault();
            // Vendor notification deferred — workshop ID would need to come from the variant's listing
            // This is a known limitation: vendor notifications require loading variant data post-save
            // TODO: load variant listing asynchronously if needed

            await _notificationService.SendOrderConfirmationAsync(
                userId,
                order.Id,
                order.TotalPrice
            );

            // Fetch user info for payment gateway
            var appUser = await _userManager.FindByIdAsync(userId);
            string firstName = "Customer";
            string lastName = "User";
            if (appUser != null && !string.IsNullOrWhiteSpace(appUser.FullName))
            {
                var nameParts = appUser
                    .FullName.Trim()
                    .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 0)
                {
                    firstName = nameParts[0];
                }
                if (nameParts.Length > 1)
                {
                    lastName = nameParts[1];
                }
            }

            string email = appUser?.Email ?? "customer@example.com";
            string phone = !string.IsNullOrWhiteSpace(phoneNumber)
                ? phoneNumber
                : (
                    !string.IsNullOrWhiteSpace(appUser?.PhoneNumber)
                        ? appUser.PhoneNumber
                        : "01000000000"
                );

            var paymentUrl = await _paymentGateway.CreatePaymentUrlAsync(
                order.Id,
                order.TotalPrice,
                firstName,
                lastName,
                email,
                phone
            );

            var responseDto = await GetOrderByIdAsync(order.Id);
            responseDto.PaymentUrl = paymentUrl;

            return responseDto;
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            return await MapToDtoAsync(order);
        }

        public async Task<List<OrderResponseDto>> GetMyOrdersAsync(string userId)
        {
            var orders = await _orderRepository
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var mappedOrders = new List<OrderResponseDto>();
            foreach (var order in orders)
            {
                mappedOrders.Add(await MapToDtoAsync(order));
            }
            return mappedOrders;
        }

        public async Task CancelOrderAsync(int orderId, string userId)
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.StatusHistory)
                .Include(o => o.Items)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found or you don't have permission to cancel it.");
            }

            if (order.Status == "Delivered" || order.Status == "Cancelled")
            {
                throw new Exception($"Cannot cancel order in '{order.Status}' status.");
            }

            var oldStatus = order.Status;
            order.Status = "Cancelled";
            order.StatusHistory.Add(
                new OrderStatusHistory { OldStatus = oldStatus, NewStatus = "Cancelled" }
            );

            //_orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            // Send internal notifications for cancellation
            await _internalNotificationService.CreateAsync(
                userId,
                NotificationType.OrderCancelled,
                orderId.ToString()
            );

            // Send notification to vendor
            // Notify vendor of cancellation — deferred as vendor id requires loading variant listing
            // TODO: load vendor via order item variant listing

            await _notificationService.SendOrderCancellationAsync(userId, order.Id);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
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
            if (!validStatuses.Contains(status))
            {
                throw new Exception("Invalid status");
            }

            var order = await _orderRepository
                .Where(o => o.Id == orderId)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            if (!IsValidStatusTransition(order.Status, status))
            {
                throw new Exception($"Cannot transition from {order.Status} to {status}");
            }

            var oldStatus = order.Status;
            order.Status = status;
            order.StatusHistory.Add(
                new OrderStatusHistory { OldStatus = oldStatus, NewStatus = status }
            );

            //_orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            // Send internal notifications based on status
            switch (status)
            {
                //case "Pending":
                //    await _internalNotificationService.CreateAsync(order.UserId, NotificationType.OrderPending, orderId.ToString());
                //    break;
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

            await _notificationService.SendOrderStatusUpdateAsync(order.UserId, order.Id, status);
        }

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

        public async Task<OrderResponseDto> UpdateOrderItemsAsync(
            int orderId,
            string userId,
            UpdateOrderItemsDto dto
        )
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found or you don't have permission to update it.");
            }

            if (order.Status != "Pending")
            {
                throw new Exception(
                    $"Cannot update order items unless the order is Pending. Current status is '{order.Status}'."
                );
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                throw new Exception("No items provided for update.");
            }

            foreach (var itemDto in dto.Items)
            {
                var orderItem = order.Items.FirstOrDefault(oi => oi.ProductVariantId == itemDto.ProductId);
                if (orderItem == null)
                {
                    throw new Exception(
                        $"Product variant with ID {itemDto.ProductId} is not part of this order."
                    );
                }

                orderItem.Quantity = itemDto.Quantity;
                if (itemDto.UnitPrice.HasValue)
                {
                    orderItem.SnapshotUnitPrice = itemDto.UnitPrice.Value;
                }
            }

            order.TotalPrice = order.Items.Sum(oi => oi.SnapshotUnitPrice * oi.Quantity);

            await _orderRepository.SaveChangesAsync();

            return await MapToDtoAsync(order);
        }

        private async Task<OrderResponseDto> MapToDtoAsync(Order order)
        {
            var paymentTransaction = await _paymentTransactionRepository.GetByLocalOrderIdAsync(
                order.Id
            );

            return new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Address = order.Address,
                PhoneNumber = order.PhoneNumber,
                Notes = order.Notes,
                Items = order.Items.Select(oi => new OrderItemResponseDto
                    {
                        Id = oi.Id,
                        ProductVariantId = oi.ProductVariantId,
                        ProductNameEn = oi.SnapshotProductNameEn,
                        ProductNameAr = oi.SnapshotProductNameAr,
                        VendorName = oi.SnapshotVendorName,
                        UnitPrice = oi.SnapshotUnitPrice,
                        Quantity = oi.Quantity,
                        Attributes = string.IsNullOrEmpty(oi.SnapshotAttributesJson)
                            ? new List<SnapshotAttributeDto>()
                            : System.Text.Json.JsonSerializer.Deserialize<List<SnapshotAttributeDto>>(oi.SnapshotAttributesJson)
                                ?? new List<SnapshotAttributeDto>(),
                    })
                    .ToList(),
                StatusHistory =
                    order.StatusHistory != null
                        ? order
                            .StatusHistory.Select(sh => new OrderStatusHistoryResponseDto
                            {
                                Id = sh.Id,
                                OldStatus = sh.OldStatus,
                                NewStatus = sh.NewStatus,
                                CreatedAt = sh.CreatedAt,
                            })
                            .ToList()
                        : new List<OrderStatusHistoryResponseDto>(),
                PaymentStatus = paymentTransaction?.Status.ToString() ?? "Unpaid",
            };
        }
    }
}
