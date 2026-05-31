using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Product : BaseEntity<int>
    {
        [Required(ErrorMessage = "WorkshopId is required")]
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; }

        // Vendor (User) ownership - Added for direct vendor management
        // Optional for backward compatibility; will be populated via migration
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required(ErrorMessage = "NameAr is required")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "NameEn is required")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "DescriptionAr is required")]
        public string DescriptionAr { get; set; }

        [Required(ErrorMessage = "DescriptionEn is required")]
        public string DescriptionEn { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }
        public List<ProductImage>? Images { get; set; }
    }
}
