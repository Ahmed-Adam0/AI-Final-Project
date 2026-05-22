using System;

namespace Graduation_infrastructure.Entities
{
    public class FinalResultImage : BaseEntity<int>
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string BeforeImageUrl { get; set; }
        public string AfterImageUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
