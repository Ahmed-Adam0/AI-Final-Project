using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class SubCategory : BaseEntity<int>
    {
        [Required(ErrorMessage = "NameAr is required")]
        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "NameEn is required")]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public List<ProductType> ProductTypes { get; set; } = [];
    }
}
