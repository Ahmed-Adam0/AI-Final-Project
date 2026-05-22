using System;

namespace Graduation_infrastructure.Entities
{
    public class Notification : BaseEntity<int>
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
