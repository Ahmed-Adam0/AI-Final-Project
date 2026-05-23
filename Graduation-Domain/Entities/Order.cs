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
        public List<OrderItem> Items { get; set; }
        public List<OrderStatusHistory> StatusHistory { get; set; }
        public List<FinalResultImage> FinalResultImages { get; set; }
    }
}
