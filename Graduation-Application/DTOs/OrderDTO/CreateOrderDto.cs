using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
    }
}
