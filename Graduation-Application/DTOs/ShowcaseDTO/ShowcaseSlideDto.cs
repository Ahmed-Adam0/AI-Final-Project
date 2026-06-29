using System.Collections.Generic;
using Graduation_Application.DTOs.ProductDTO;

namespace Graduation_Application.DTOs.ShowcaseDTO
{
    public class ShowcaseSlideDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;
        public string ButtonLink { get; set; } = string.Empty;
        public string BackgroundImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int WorkshopId { get; set; }

        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string? SubtitleAr { get; set; }
        public string? SubtitleEn { get; set; }
        public string? ButtonTextAr { get; set; }
        public string? ButtonTextEn { get; set; }

        public List<ShowcaseHotspotDto> Hotspots { get; set; } = new();
    }

    public class ShowcaseHotspotDto
    {
        public int Id { get; set; }
        public ProductDto Product { get; set; } = null!;
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
