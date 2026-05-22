namespace Graduation_infrastructure.Entities
{
    public class ProductImage : BaseEntity<int>
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }
}
