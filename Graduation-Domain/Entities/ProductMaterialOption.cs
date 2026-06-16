namespace Graduation_domain.Entities
{
    public class ProductMaterialOption
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int VendorMaterialOptionId { get; set; }
        public VendorMaterialOption VendorMaterialOption { get; set; } = null!;

        public decimal PriceOption { get; set; }
    }
}
