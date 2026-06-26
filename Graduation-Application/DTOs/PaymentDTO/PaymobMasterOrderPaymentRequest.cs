using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobMasterOrderPaymentRequest
    {
        [Required]
        public int MasterOrderId { get; set; }

    }
}
