using System.ComponentModel.DataAnnotations;

namespace Graduation_MVC.Areas.Admin.ViewModels.FAQ
{
    public class AdminFaqFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Arabic Question is required")]
        [Display(Name = "Arabic Question")]
        public string QuestionAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Question is required")]
        [Display(Name = "English Question")]
        public string QuestionEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Answer is required")]
        [Display(Name = "Arabic Answer")]
        public string AnswerAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Answer is required")]
        [Display(Name = "English Answer")]
        public string AnswerEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display Order is required")]
        [Display(Name = "Display Order")]
        [Range(1, 10000, ErrorMessage = "Display Order must be greater than 0")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}
