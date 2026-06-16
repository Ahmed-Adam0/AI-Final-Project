namespace Graduation_domain.Entities
{
    /// <summary>
    /// Explicit many-to-many join table between ProductVariant and ProductAttributeValue.
    /// Defines which attribute value selections constitute a specific ProductVariant.
    ///
    /// Composite PK: (VariantId, AttributeValueId) — no surrogate key needed.
    /// EF Core Fluent API: HasKey(x => new { x.VariantId, x.AttributeValueId })
    /// </summary>
    public class ProductVariantAttributeValue
    {
        public int VariantId { get; set; }
        public ProductVariant Variant { get; set; } = null!;

        public int AttributeValueId { get; set; }
        public ProductAttributeValue AttributeValue { get; set; } = null!;
    }
}
