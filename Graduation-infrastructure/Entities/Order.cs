using System;
using System.Collections.Generic;

namespace Graduation_infrastructure.Entities
{
    public class Order : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public List<OrderItem> Items { get; set; }
        public List<OrderStatusHistory> StatusHistory { get; set; }
        public List<FinalResultImage> FinalResultImages { get; set; }
    }
}
