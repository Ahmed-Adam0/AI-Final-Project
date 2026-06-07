using System.ComponentModel.DataAnnotations;

namespace Graduation_domain.Entities
{
    public class Faq : BaseEntity<int>
    {
        [Required]
        public string QuestionAr { get; set; } = string.Empty;

        [Required]
        public string QuestionEn { get; set; } = string.Empty;

        [Required]
        public string AnswerAr { get; set; } = string.Empty;

        [Required]
        public string AnswerEn { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
