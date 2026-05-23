using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Cart : BaseEntity<int>
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public List<CartItem> Items { get; set; }
    }
}
