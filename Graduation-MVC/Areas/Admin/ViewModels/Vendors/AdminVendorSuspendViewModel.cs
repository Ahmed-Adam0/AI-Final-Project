using System.ComponentModel.DataAnnotations;

namespace Graduation_MVC.Areas.Admin.ViewModels.Vendors
{
    public class AdminVendorSuspendViewModel
    {
        [Required]
        public int WorkshopId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}

