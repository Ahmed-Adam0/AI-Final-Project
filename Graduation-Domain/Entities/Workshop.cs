using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public WorkshopAddress? WorkshopAddress { get; set; }
        public List<Product>? Products { get; set; }
        public List<Review>? Reviews { get; set; }
    }
}
