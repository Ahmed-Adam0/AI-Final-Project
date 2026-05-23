using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class ProductImage : BaseEntity<int>
    {
        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required(ErrorMessage = "ImageUrl is required")]
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }
}
