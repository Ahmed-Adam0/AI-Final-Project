using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class ProductType : BaseEntity<int>
    {
        [Required(ErrorMessage = "NameAr is required")]
        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "NameEn is required")]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; } = null!;

        public List<Product> Products { get; set; } = [];
    }
}
