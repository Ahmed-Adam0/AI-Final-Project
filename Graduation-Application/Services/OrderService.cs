using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
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

        public OrderService(
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<OrderItem> orderItemRepository,
            IGenaricRepositories<Cart> cartRepository,
            IGenaricRepositories<CartItem> cartItemRepository,
            ICartService cartService,
            INotificationService notificationService
        )
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _cartService = cartService;
            _notificationService = notificationService;
        }

        public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository
                .Where(o => o.Status != "Cancelled")
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return orders.Select(MapToDto).ToList();
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
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync();

            if (cart == null || !cart.Items.Any())
            {
                throw new Exception("Cart is empty");
            }

            decimal totalPrice = cart.Items.Sum(ci => ci.Price * ci.Quantity);

            var phoneNumber = request.PhoneNumber ?? user.FindFirst(ClaimTypes.MobilePhone)?.Value;

            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                Status = "Pending",
                Address = request.Address,
                PhoneNumber = phoneNumber,
                Notes = request.Notes,
                // Create new OrderItem instances (fresh objects)
                Items = cart
                    .Items.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Price,
                    })
                    .ToList(),
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
            await _notificationService.SendOrderConfirmationAsync(
                userId,
                order.Id,
                order.TotalPrice
            );

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            return MapToDto(order);
        }

        public async Task<List<OrderResponseDto>> GetMyOrdersAsync(string userId)
        {
            var orders = await _orderRepository
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task CancelOrderAsync(int orderId, string userId)
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.StatusHistory)
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
            await _notificationService.SendOrderCancellationAsync(userId, order.Id);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var validStatuses = new[]
            {
                "Pending",
                "Confirmed",
                "InProgress",
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

            var oldStatus = order.Status;
            order.Status = status;
            order.StatusHistory.Add(
                new OrderStatusHistory { OldStatus = oldStatus, NewStatus = status }
            );

            //_orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
            await _notificationService.SendOrderStatusUpdateAsync(order.UserId, order.Id, status);
        }

        private OrderResponseDto MapToDto(Order order)
        {
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
                Items = order
                    .Items.Select(oi => new OrderItemResponseDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.NameEn,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
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
            };
        }
    }
}
