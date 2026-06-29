using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class ShowcaseHotspot : BaseEntity<int>
    {
        public int ShowcaseSlideId { get; set; }
        public ShowcaseSlide ShowcaseSlide { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        public decimal X { get; set; }

        [Required]
        public decimal Y { get; set; }

        public int DisplayOrder { get; set; }
    }
}
