using System.ComponentModel.DataAnnotations;
using Graduation_domain.Enums;

namespace Graduation_domain.Entities
{
    public class VendorAccountStatusHistory : BaseEntity<int>
    {
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; }

        public VendorAccountStatus OldStatus { get; set; }
        public VendorAccountStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public string? PerformedByAdminId { get; set; }
        public ApplicationUser? PerformedByAdmin { get; set; }
    }
}

