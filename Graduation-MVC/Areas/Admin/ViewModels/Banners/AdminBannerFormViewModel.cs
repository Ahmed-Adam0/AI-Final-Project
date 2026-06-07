using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Graduation_MVC.Areas.Admin.ViewModels.Banners
{
    public class AdminBannerFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Arabic Title is required")]
        [Display(Name = "Arabic Title")]
        [StringLength(150, ErrorMessage = "Arabic Title cannot exceed 150 characters")]
        public string TitleAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Title is required")]
        [Display(Name = "English Title")]
        [StringLength(150, ErrorMessage = "English Title cannot exceed 150 characters")]
        public string TitleEn { get; set; } = string.Empty;

        [Display(Name = "Arabic Description")]
        [StringLength(500, ErrorMessage = "Arabic Description cannot exceed 500 characters")]
        public string? DescriptionAr { get; set; }

        [Display(Name = "English Description")]
        [StringLength(500, ErrorMessage = "English Description cannot exceed 500 characters")]
        public string? DescriptionEn { get; set; }

        public string? ImageUrl { get; set; }

        [Display(Name = "Banner Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Redirect URL")]
        [Url(ErrorMessage = "Invalid Redirect URL")]
        public string? RedirectUrl { get; set; }

        [Required(ErrorMessage = "Display Order is required")]
        [Display(Name = "Display Order")]
        [Range(1, 10000, ErrorMessage = "Display Order must be greater than 0")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}
