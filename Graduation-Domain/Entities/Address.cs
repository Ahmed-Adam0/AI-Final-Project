using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Address : BaseEntity<int>
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string AreaAr { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
