namespace Graduation_Application.DTOs.FaqDTO
{
    public class FaqDto
    {
        public int Id { get; set; }
        public string QuestionAr { get; set; } = string.Empty;
        public string QuestionEn { get; set; } = string.Empty;
        public string AnswerAr { get; set; } = string.Empty;
        public string AnswerEn { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
