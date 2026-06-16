using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Category : BaseEntity<int>
    {
        [Required(ErrorMessage = "NameAr is required")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "NameEn is required")]
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }

        public List<SubCategory> SubCategories { get; set; } = [];
    }
}
