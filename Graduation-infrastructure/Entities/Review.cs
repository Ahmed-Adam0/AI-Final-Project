using System;

namespace Graduation_infrastructure.Entities
{
    public class Review : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
