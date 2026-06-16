using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.OrderDTO
{
    /// <summary>
    /// Represents a single order line item in API responses.
    /// All fields come from the immutable snapshot — they reflect what was purchased,
    /// not the current state of the product or variant.
    /// </summary>
    public class OrderItemResponseDto
    {
        public int Id { get; set; }

        /// <summary>Nullable — variant may have been deleted after purchase.</summary>
        public int? ProductVariantId { get; set; }

        // ── Snapshot fields (what was true at purchase time) ──────────────────
        public string ProductNameAr { get; set; } = string.Empty;
        public string ProductNameEn { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        /// <summary>
        /// Deserialized list of attribute selections at purchase time.
        /// e.g. [{ attributeNameEn: "Material", valueEn: "Oak" }]
        /// </summary>
        public List<SnapshotAttributeDto> Attributes { get; set; } = [];
    }

    /// <summary>One attribute+value pair from the frozen order snapshot.</summary>
    public class SnapshotAttributeDto
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;
        public string ValueAr { get; set; } = string.Empty;
    }
}
