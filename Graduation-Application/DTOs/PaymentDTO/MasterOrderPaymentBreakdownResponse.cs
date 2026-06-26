namespace Graduation_Application.DTOs.PaymentDTO
{
    public class MasterOrderPaymentBreakdownResponse
    {
        public int MasterOrderId { get; set; }
        public decimal TotalEligibleAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
    }
}
