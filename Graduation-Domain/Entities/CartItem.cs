using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class CartItem : BaseEntity<int>
    {
        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
