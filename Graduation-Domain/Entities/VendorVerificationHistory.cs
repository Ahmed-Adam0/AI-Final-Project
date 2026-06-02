using System.ComponentModel.DataAnnotations;
using Graduation_domain.Enums;

namespace Graduation_domain.Entities
{
    public class VendorVerificationHistory : BaseEntity<int>
    {
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; }

        public VendorVerificationStatus OldStatus { get; set; }
        public VendorVerificationStatus NewStatus { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public string? PerformedByAdminId { get; set; }
        public ApplicationUser? PerformedByAdmin { get; set; }
    }
}

