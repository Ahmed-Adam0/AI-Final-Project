using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class ReviewModerationLog : BaseEntity<int>
    {
        [Required]
        public int ReviewId { get; set; }

        public Review Review { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public string? AdminUserId { get; set; }
    }
}

