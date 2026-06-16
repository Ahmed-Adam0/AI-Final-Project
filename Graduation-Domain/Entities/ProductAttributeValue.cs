namespace Graduation_domain.Entities
{
    /// <summary>
    /// A specific option for a ProductAttribute dimension.
    /// Example: Attribute = "Material" → Values = ["Oak", "Walnut", "MDF"]
    /// </summary>
    public class ProductAttributeValue : BaseEntity<int>
    {
        public int AttributeId { get; set; }
        public ProductAttribute Attribute { get; set; } = null!;

        /// <summary>Value in Arabic — e.g. "خشب البلوط"</summary>
        public string ValueAr { get; set; } = string.Empty;

        /// <summary>Value in English — e.g. "Oak"</summary>
        public string ValueEn { get; set; } = string.Empty;

        // ── Navigations ──────────────────────────────────────────────────────────
        /// <summary>Join entries linking this value to the variants it participates in.</summary>
        public List<ProductVariantAttributeValue> VariantAttributeValues { get; set; } = [];
    }
}
