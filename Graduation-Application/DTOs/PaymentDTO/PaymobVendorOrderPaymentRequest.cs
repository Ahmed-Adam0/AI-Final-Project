using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobVendorOrderPaymentRequest
    {
        [Required]
        public int VendorOrderId { get; set; }
    }
}
