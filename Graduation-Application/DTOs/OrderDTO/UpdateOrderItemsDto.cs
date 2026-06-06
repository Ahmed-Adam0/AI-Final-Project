using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class UpdateOrderItemsDto
    {
        [Required(ErrorMessage = "Items list is required")]
        public List<UpdateOrderItemDto> Items { get; set; } = new List<UpdateOrderItemDto>();
    }

    public class UpdateOrderItemDto
    {
        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public decimal? UnitPrice { get; set; }
    }
}
