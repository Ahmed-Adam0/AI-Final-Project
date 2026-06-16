using System.Collections.Generic;
using Graduation_Application.DTOs.ProductDTO;

namespace Graduation_Application.DTOs.CartDTO
{
    public class CartItemResponseDto
    {
        public int Id { get; set; }

        // ── Variant identity ────────────────────────────────────────────────
        public int ProductVariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductNameAr { get; set; } = string.Empty;
        public string ProductNameEn { get; set; } = string.Empty;
        public string VendorNameEn { get; set; } = string.Empty;
        public string VendorNameAr { get; set; } = string.Empty;

        // ── Pricing ─────────────────────────────────────────────────────────
        /// <summary>Price per unit cached when item was added. May differ from live price.</summary>
        public decimal CachedPrice { get; set; }

        /// <summary>Live current price from ProductVariant — use this at checkout.</summary>
        public decimal LivePrice { get; set; }

        /// <summary>True if the price has changed since the item was added to cart.</summary>
        public bool IsPriceStale => LivePrice != CachedPrice;

        public int Quantity { get; set; }
        public decimal TotalPrice => LivePrice * Quantity;

        // ── Variant details ──────────────────────────────────────────────────
        public string? VariantImageUrl { get; set; }

        /// <summary>The attribute combination selected for this cart item.</summary>
        public List<SelectedAttributeDto> SelectedAttributes { get; set; } = [];

        public List<string> ProductImages { get; set; } = [];
    }
}
