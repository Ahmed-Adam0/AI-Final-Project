using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.CartDTO;

namespace Graduation_Application.IServices
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(string userId);
        Task<CartItemResponseDto> AddToCartAsync(string userId, AddToCartDto dto);
        Task UpdateCartItemAsync(string userId, UpdateCartItemDto dto);
        Task RemoveFromCartAsync(string userId, int cartItemId);
        Task ClearCartAsync(string userId);
    }
}
