namespace Graduation_domain.Entities
{
    public class ProductAttribute : BaseEntity<int>
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        /// <summary>Display name in Arabic — e.g. "الخامة"</summary>
        public string NameAr { get; set; } = string.Empty;

        /// <summary>Display name in English — e.g. "Material"</summary>
        public string NameEn { get; set; } = string.Empty;

        // ── Navigations ──────────────────────────────────────────────────────────
        /// <summary>All concrete options available for this attribute dimension.</summary>
        public List<ProductAttributeValue> Values { get; set; } = [];
    }
}
