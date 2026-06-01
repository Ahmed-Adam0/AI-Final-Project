using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class ProductReport : BaseEntity<int>
    {
        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        public bool IsResolved { get; set; } = false;
    }
}
