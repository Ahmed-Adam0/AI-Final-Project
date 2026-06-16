using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    /// <summary>
    /// A single line item in a user's shopping Cart.
    ///
    /// What changed vs. the old model:
    ///   - REMOVED: ProductId (cart now references a specific variant — which includes vendor + attributes)
    ///   - RENAMED: Price → CachedPrice (documents that this is a snapshot cached at add-to-cart time)
    ///   - ADDED:   ProductVariantId FK
    ///
    /// CachedPrice is re-validated against the live CurrentPrice at checkout time.
    /// </summary>
    public class CartItem : BaseEntity<int>
    {
        [Required]
        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>
        /// JSON array of selected ProductAttributeValue IDs. E.g., "[10, 15]".
        /// </summary>
        public string SelectedOptionsJson { get; set; } = "[]";

        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per unit captured when the item was added to the cart.
        /// Always re-validate this against the product's BasePrice + Option Deltas before finalizing an order.
        /// </summary>
        public decimal CachedPrice { get; set; }
    }
}
