using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class ActivityLog : BaseEntity<int>
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserRole { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? UserRoleAr { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ActionAr { get; set; }

        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? EntityTypeAr { get; set; }

        [MaxLength(100)]
        public string? EntityId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? DescriptionAr { get; set; }
    }
}
