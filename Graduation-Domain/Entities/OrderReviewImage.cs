using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class OrderReviewImage : BaseEntity<int>
    {
        [Required(ErrorMessage = "OrderId is required")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required(ErrorMessage = "BeforeImageUrl is required")]
        public string BeforeImageUrl { get; set; }

        [Required(ErrorMessage = "AfterImageUrl is required")]
        public string AfterImageUrl { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}
