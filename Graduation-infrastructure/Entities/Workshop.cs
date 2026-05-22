using System.Collections.Generic;

namespace Graduation_infrastructure.Entities
{
    public class Workshop : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string WorkshopNameAr { get; set; }
        public string WorkshopNameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string Address { get; set; }
        public string? LogoUrl { get; set; }
        public decimal? Rating { get; set; }
        public bool IsVerified { get; set; }

        public List<Product>? Products { get; set; }
        public List<Review>? Reviews { get; set; }
    }
}
