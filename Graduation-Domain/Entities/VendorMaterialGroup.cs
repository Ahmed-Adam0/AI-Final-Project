using System.Collections.Generic;

namespace Graduation_domain.Entities
{
    public class VendorMaterialGroup : BaseEntity<int>
    {
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; } = null!;

        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;

        public List<VendorMaterialOption> Options { get; set; } = [];
    }
}
