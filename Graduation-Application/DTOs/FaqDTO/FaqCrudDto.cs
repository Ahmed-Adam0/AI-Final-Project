namespace Graduation_Application.DTOs.FaqDTO
{
    public class CreateFaqDto
    {
        public string QuestionAr { get; set; } = string.Empty;
        public string QuestionEn { get; set; } = string.Empty;
        public string AnswerAr { get; set; } = string.Empty;
        public string AnswerEn { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateFaqDto
    {
        public string QuestionAr { get; set; } = string.Empty;
        public string QuestionEn { get; set; } = string.Empty;
        public string AnswerAr { get; set; } = string.Empty;
        public string AnswerEn { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class FaqResponseDto
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
