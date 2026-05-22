using System;
using System.Collections.Generic;

namespace Graduation_infrastructure.Entities
{
    public class Product : BaseEntity<int>
    {
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }

        public decimal Price { get; set; }
        public List<ProductImage>? Images { get; set; }
    }
}
