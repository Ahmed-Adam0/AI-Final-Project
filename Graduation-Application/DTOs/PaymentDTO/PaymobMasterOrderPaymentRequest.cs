using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.PaymentDTO
{
    public class PaymobMasterOrderPaymentRequest
    {
        [Required]
        public int MasterOrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
    }
}
