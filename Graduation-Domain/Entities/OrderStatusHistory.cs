using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class OrderStatusHistory : BaseEntity<int>
    {
        [Required(ErrorMessage = "Order is required")]
        public Order Order { get; set; }

        [Required(ErrorMessage = "Old status is required")]
        public string OldStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "New status is required")]
        public string NewStatus { get; set; } = string.Empty;
    }
}
