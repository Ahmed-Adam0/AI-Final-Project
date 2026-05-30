using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Review : BaseEntity<int>
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int? WorkshopId { get; set; }
        public Workshop Workshop { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        public int Rating { get; set; }
        public string Comment { get; set; }

        public string? VendorReply { get; set; }
        public DateTime? ReplyCreatedAt { get; set; }
        public bool IsReported { get; set; } = false;
        public string? ReportReason { get; set; }
    }
}
