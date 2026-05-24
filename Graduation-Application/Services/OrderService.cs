using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Microsoft.EntityFrameworkCore;
using Graduation_infrastructure.AppDbContext;

namespace Graduation_infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

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
                    new OrderStatusHistory
                    {
                        OldStatus = null,
                        NewStatus = "Pending"
                    }
                }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price
                };
                order.Items.Add(orderItem);
                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();
            await _cartService.ClearCartAsync(userId);

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            return MapToDto(order);
        }

        public async Task<List<OrderResponseDto>> GetMyOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "InProgress", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                throw new Exception("Invalid status");
            }

            var order = await _context.Orders
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            var oldStatus = order.Status;
            order.Status = status;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OldStatus = oldStatus,
                NewStatus = status
            });

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
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
                Items = order.Items.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.NameEn,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}
