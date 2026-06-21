using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;

namespace Graduation_Application.IServices
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(
            string userId,
            CreateOrderDto request,
            ClaimsPrincipal user
        );
        Task<OrderResponseDto> GetOrderByIdAsync(int orderId);
        Task<List<OrderResponseDto>> GetMyOrdersAsync(string userId);
        Task UpdateOrderStatusAsync(int orderId, string status);
        Task CancelOrderAsync(int orderId, string userId);
        Task<List<OrderResponseDto>> GetAllOrdersAsync();
        Task<OrderResponseDto> UpdateOrderItemsAsync(int orderId, string userId, UpdateOrderItemsDto dto);
        Task ApproveVendorOrderScheduleAsync(int vendorOrderId, string userId);
        Task RejectVendorOrderScheduleAsync(int vendorOrderId, string userId);
    }
}
