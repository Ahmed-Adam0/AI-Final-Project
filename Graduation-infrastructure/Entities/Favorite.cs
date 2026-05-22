using System;

namespace Graduation_infrastructure.Entities
{
    public class Favorite : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
