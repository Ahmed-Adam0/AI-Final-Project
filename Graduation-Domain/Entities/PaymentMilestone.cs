using Graduation_domain.Enums;
using System;

namespace Graduation_domain.Entities
{
    public class PaymentMilestone : BaseEntity<int>
    {
        public int VendorOrderId { get; set; }
        public VendorOrder VendorOrder { get; set; } = null!;
        public VendorOrderStatus MilestoneStatus { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public int? PaymentTransactionId { get; set; }
        public PaymentTransaction? PaymentTransaction { get; set; }
    }
}
