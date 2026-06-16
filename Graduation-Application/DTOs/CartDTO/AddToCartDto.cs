using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.CartDTO
{
    /// <summary>
    /// Request to add a specific product variant to the cart.
    /// The variant ID encodes the full selection: vendor + attribute combination.
    /// </summary>
    public class AddToCartDto
    {
        [Required]
        public int ProductVariantId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
