using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class OrderItem : BaseEntity<int>
    {
        // Add explicit FK
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
