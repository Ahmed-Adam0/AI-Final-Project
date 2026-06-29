using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Graduation_Application.DTOs.ShowcaseDTO
{
    public class CreateShowcaseSlideRequest
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
        public IFormFile BackgroundImage { get; set; } = null!;

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public List<CreateShowcaseHotspotRequest>? Hotspots { get; set; }
        public string? HotspotsJson { get; set; }
    }

    public class UpdateShowcaseSlideRequest
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

        public IFormFile? BackgroundImage { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public List<CreateShowcaseHotspotRequest>? Hotspots { get; set; }
        public string? HotspotsJson { get; set; }
    }

    public class CreateShowcaseHotspotRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "X must be between 0 and 100.")]
        public decimal X { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "Y must be between 0 and 100.")]
        public decimal Y { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
