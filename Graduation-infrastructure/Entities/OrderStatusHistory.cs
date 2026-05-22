using System;

namespace Graduation_infrastructure.Entities
{
    public class OrderStatusHistory : BaseEntity<int>
    {
        public Order Order { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }
}
