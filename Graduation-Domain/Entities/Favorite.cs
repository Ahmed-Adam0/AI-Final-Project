using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Favorite : BaseEntity<int>
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
