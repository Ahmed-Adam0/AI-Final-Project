using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.OrderDTO;

namespace Graduation_Application.IServices
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(string userId);
        Task<OrderResponseDto> GetOrderByIdAsync(int orderId);
        Task<List<OrderResponseDto>> GetMyOrdersAsync(string userId);
        Task UpdateOrderStatusAsync(int orderId, string status);
        Task<List<OrderResponseDto>> GetAllOrdersAsync();
    }
}
