using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Banner : BaseEntity<int>
    {
        [Required]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public string TitleEn { get; set; } = string.Empty;

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string? RedirectUrl { get; set; }

        public int DisplayOrder { get; set; }
    }
}
