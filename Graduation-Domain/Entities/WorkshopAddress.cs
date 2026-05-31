using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class WorkshopAddress : BaseEntity<int>
    {
        [Required(ErrorMessage = "WorkshopId is required")]
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
