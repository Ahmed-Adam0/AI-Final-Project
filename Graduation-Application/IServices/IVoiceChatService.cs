using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ChatDTO;
using Microsoft.AspNetCore.Http;

namespace Graduation_Application.IServices
{
    /// <summary>
    /// Service responsible for handling voice chat logic, including speech-to-text conversion
    /// and routing the message to the chat service.
    /// </summary>
    public interface IVoiceChatService
    {
        /// <summary>
        /// Processes an incoming voice message, transcribes it, and sends the resulting text to the chatbot.
        /// </summary>
        /// <param name="audioFile">The audio file containing the user's voice message.</param>
        /// <param name="userId">The ID of the user sending the message.</param>
        /// <param name="conversationId">The optional conversation ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The reply from the chatbot.</returns>
        Task<ChatReplyDto> ProcessVoiceMessageAsync(IFormFile audioFile, string userId, string? conversationId = null, string token = "", CancellationToken cancellationToken = default);
    }
}
