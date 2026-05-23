using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Review : BaseEntity<int>
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
