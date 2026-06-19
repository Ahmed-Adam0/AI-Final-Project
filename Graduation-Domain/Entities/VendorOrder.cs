using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Graduation_domain.Enums;

namespace Graduation_domain.Entities
{
    public class VendorOrder : BaseEntity<int>
    {
        [Required]
        public int MasterOrderId { get; set; }
        public Order MasterOrder { get; set; } = null!;

        [Required]
        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; } = null!;

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public VendorOrderStatus Status { get; set; } = VendorOrderStatus.Pending;

        public List<OrderItem> Items { get; set; } = [];
        public List<VendorOrderStatusHistory> StatusHistory { get; set; } = [];
    }
}
