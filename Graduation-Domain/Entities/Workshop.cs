using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Graduation_domain.Enums;

namespace Graduation_domain.Entities
{
    public class Workshop : BaseEntity<int>
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "WorkshopNameAr is required")]
        public string WorkshopNameAr { get; set; }

        [Required(ErrorMessage = "WorkshopNameEn is required")]
        public string WorkshopNameEn { get; set; }

        [Required(ErrorMessage = "DescriptionAr is required")]
        public string DescriptionAr { get; set; }

        [Required(ErrorMessage = "DescriptionEn is required")]
        public string DescriptionEn { get; set; }

        public string? LogoUrl { get; set; }
        public decimal? Rating { get; set; }
        public bool IsVerified { get; set; }

        // =========================
        // Vendor Management (Admin)
        // =========================
        public VendorVerificationStatus VerificationStatus { get; set; } =
            VendorVerificationStatus.inActive;
        public DateTime? VerificationDate { get; set; }
        public string? VerifiedByAdminId { get; set; }
        public ApplicationUser? VerifiedByAdmin { get; set; }
        public string? VerificationNotes { get; set; }
        public string? RejectionReason { get; set; }

        public VendorAccountStatus AccountStatus { get; set; } = VendorAccountStatus.Pending;
        public DateTime? AccountStatusChangedAt { get; set; }
        public string? AccountStatusChangedByAdminId { get; set; }
        public ApplicationUser? AccountStatusChangedByAdmin { get; set; }

        public WorkshopAddress? WorkshopAddress { get; set; }

        /// <summary>
        /// All vendor listings this workshop has created across different products.
        /// Replaces the old direct Products collection — vendor's catalog is now expressed
        /// through VendorProductListings rather than direct Product ownership.
        /// </summary>
        public List<VendorProductListing>? VendorListings { get; set; }

        public List<Review>? Reviews { get; set; }

        public List<VendorVerificationHistory>? VerificationHistory { get; set; }
        public List<VendorAccountStatusHistory>? AccountStatusHistory { get; set; }
    }
}
