using System.Collections.Generic;

namespace Graduation_Application.DTOs.CartDTO
{
    public class CartResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public List<CartItemResponseDto> Items { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
