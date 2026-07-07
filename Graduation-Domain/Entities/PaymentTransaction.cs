using Graduation_Domain.Enums;

namespace Graduation_domain.Entities
{
    public class PaymentTransaction : BaseEntity<int>
    {
        public int LocalOrderId { get; set; }
        public int? PaymobOrderId { get; set; }
        public string? PaymentToken { get; set; }
        public string? TransactionId { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;
        public decimal Amount { get; set; }
        public decimal? CommissionAmount { get; set; }   // 10% marketplace commission
        public decimal? VendorNetAmount  { get; set; }   // 90% net amount credited to vendor wallet
        public string Currency { get; set; } = "EGP";
        public DateTime? PaidAt { get; set; }
        public string? FailureReason { get; set; }
        public List<PaymentMilestone> PaymentMilestones { get; set; } = [];
    }
}
