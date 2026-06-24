namespace Graduation_Application.DTOs.ChatDTO
{
    public class N8NChatRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
