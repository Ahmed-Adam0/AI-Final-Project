using System.Collections.Generic;

namespace Graduation_domain.Entities
{
    public class VendorMaterialOption : BaseEntity<int>
    {
        public int VendorMaterialGroupId { get; set; }
        public VendorMaterialGroup Group { get; set; } = null!;

        public string ValueAr { get; set; } = string.Empty;
        public string ValueEn { get; set; } = string.Empty;

        public decimal PriceDelta { get; set; }

        public List<ProductMaterialOption> ProductMaterialOptions { get; set; } = [];
    }
}
