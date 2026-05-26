using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.UserDTO
{
    public class AddressDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
