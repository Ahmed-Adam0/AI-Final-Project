using System.ComponentModel.DataAnnotations;

namespace Graduation_MVC.Areas.Admin.ViewModels.Categories
{
    public class AdminProductTypeFormViewModel
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

        [Required]
        public int SubCategoryId { get; set; }

        public string? SubCategoryNameEn { get; set; }
    }
}
