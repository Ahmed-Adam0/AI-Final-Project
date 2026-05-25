using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenaricRepositories<Order> _orderRepository;
        private readonly IGenaricRepositories<OrderItem> _orderItemRepository;
        private readonly IGenaricRepositories<Cart> _cartRepository;
        private readonly IGenaricRepositories<CartItem> _cartItemRepository;
        private readonly ICartService _cartService;

        public OrderService(
            IGenaricRepositories<Order> orderRepository,
            IGenaricRepositories<OrderItem> orderItemRepository,
            IGenaricRepositories<Cart> cartRepository,
            IGenaricRepositories<CartItem> cartItemRepository,
            ICartService cartService
        )
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _cartService = cartService;
        }

        public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository
                .Where(o => o.Status != "Cancelled")
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponseDto> CreateOrderAsync(string userId)
        {
            var cart = await _cartRepository
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync();

            if (cart == null || !cart.Items.Any())
            {
                throw new Exception("Cart is empty");
            }

            decimal totalPrice = cart.Items.Sum(ci => ci.Price * ci.Quantity);

            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                Status = "Pending",
                Items = new List<OrderItem>(),
                StatusHistory = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory { OldStatus = null, NewStatus = "Pending" },
                },
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price,
                };
                order.Items.Add(orderItem);
                await _orderItemRepository.AddAsync(orderItem);
            }

            await _orderItemRepository.SaveChangesAsync();
            await _cartService.ClearCartAsync(userId);

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository
                .Where(o => o.Id == orderId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
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
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
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

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
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
            };
        }
    }
}
