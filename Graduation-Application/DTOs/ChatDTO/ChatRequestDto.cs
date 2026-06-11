using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.ChatDTO
{
    public class ChatRequestDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? ConversationId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
