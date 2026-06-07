using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Graduation_MVC.Areas.Admin.ViewModels.Categories
{
    public class AdminCategoryFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Arabic Name is required")]
        [Display(Name = "Arabic Name")]
        [StringLength(100, ErrorMessage = "Arabic Name cannot exceed 100 characters")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Name is required")]
        [Display(Name = "English Name")]
        [StringLength(100, ErrorMessage = "English Name cannot exceed 100 characters")]
        public string NameEn { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Display(Name = "Category Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
