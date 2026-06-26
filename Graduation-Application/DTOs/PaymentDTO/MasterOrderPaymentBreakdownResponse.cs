using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class MasterOrderPaymentBreakdownResponse
    {
        public decimal TotalPrice { get; set; }
        public decimal RemainingBalance { get; set; }
        public List<MasterOrderPaymentMilestoneDto> Milestones { get; set; } = [];
    }

    public class MasterOrderPaymentMilestoneDto
    {
        public int MilestoneId { get; set; }
        public int VendorOrderId { get; set; }
        public string MilestoneStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
