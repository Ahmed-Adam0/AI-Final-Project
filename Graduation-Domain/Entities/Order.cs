using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Order : BaseEntity<int>
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "TotalPrice is required")]
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid";
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public List<VendorOrder> VendorOrders { get; set; } = [];
        public List<FinalResultImage> FinalResultImages { get; set; }
        public List<OrderReviewImage> OrderReviewImages { get; set; } = [];
    }
}
