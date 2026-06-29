using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class ShowcaseSlide : BaseEntity<int>
    {
        [Required]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public string TitleEn { get; set; } = string.Empty;

        public string? SubtitleAr { get; set; }
        public string? SubtitleEn { get; set; }

        public string? ButtonTextAr { get; set; }
        public string? ButtonTextEn { get; set; }

        [Required]
        public string ButtonLink { get; set; } = string.Empty;

        [Required]
        public string BackgroundImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public int WorkshopId { get; set; }
        public Workshop Workshop { get; set; } = null!;

        public List<ShowcaseHotspot> Hotspots { get; set; } = new();
    }
}
