using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        public int? AddressId { get; set; }

        public string? SecondaryAddress { get; set; }

        public int? SecondaryAddressId { get; set; }

        public string? Notes { get; set; }
    }
}
