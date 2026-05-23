using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Notification : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
