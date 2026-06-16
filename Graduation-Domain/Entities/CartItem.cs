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

        /// <summary>
        /// The specific variant the customer has selected (encodes vendor + attribute combination).
        /// </summary>
        [Required]
        public int ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per unit captured when the item was added to the cart.
        /// Always re-validate this against ProductVariant.CurrentPrice before finalizing an order.
        /// </summary>
        public decimal CachedPrice { get; set; }
    }
}
