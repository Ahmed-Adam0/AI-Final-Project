using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class VendorOrderStatusHistory : BaseEntity<int>
    {
        [Required(ErrorMessage = "VendorOrder is required")]
        public int VendorOrderId { get; set; }

        [Required(ErrorMessage = "VendorOrder is required")]
        public VendorOrder VendorOrder { get; set; } = null!;

        [Required(ErrorMessage = "Old status is required")]
        public string OldStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "New status is required")]
        public string NewStatus { get; set; } = string.Empty;
    }
}
