using System;

namespace Graduation_domain.Entities
{
    public class InternalNotification : BaseEntity<int>
    {
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string? TitleAr { get; set; }
        public string? TitleEn { get; set; }
        public string? MessageAr { get; set; }
        public string? MessageEn { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
