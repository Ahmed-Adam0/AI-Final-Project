using System;
using System.Collections.Generic;

namespace Graduation_infrastructure.Entities
{
    public class Cart : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public List<CartItem> Items { get; set; }
    }
}
