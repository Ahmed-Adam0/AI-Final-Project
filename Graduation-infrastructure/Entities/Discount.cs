using System;

namespace Graduation_infrastructure.Entities
{
    public class Discount : BaseEntity<int>
    {
        public string Code { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
