using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Graduation_domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "ar";
        public string ProfileImage { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public List<Address>? Addresses { get; set; }
        public List<Cart>? Carts { get; set; }
        public List<Favorite>? Favorites { get; set; }
        public List<Order>? Orders { get; set; }
        public List<Workshop>? Workshops { get; set; }
        public List<Review>? Reviews { get; set; }
    }
}
