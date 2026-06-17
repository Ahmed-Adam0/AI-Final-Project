using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ChatDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Graduation_Application.Services
{
    /// <summary>
    /// Implementation of the voice chat service.
    /// Orchestrates file validation, speech-to-text conversion, and message routing.
    /// </summary>
    public class VoiceChatService : IVoiceChatService
    {
        private readonly ISpeechToTextService _speechToTextService;
        private readonly IChatService _chatService;
        private readonly ILogger<VoiceChatService> _logger;

        private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB
        private readonly string[] _allowedExtensions = { ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".webm" };
        private readonly string[] _allowedMimeTypes = { "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp4", "audio/aac", "audio/webm", "video/webm" };

        public VoiceChatService(
            ISpeechToTextService speechToTextService,
            IChatService chatService,
            ILogger<VoiceChatService> logger)
        {
            _speechToTextService = speechToTextService;
            _chatService = chatService;
            _logger = logger;
        }

        public async Task<ChatReplyDto> ProcessVoiceMessageAsync(IFormFile audioFile, string userId, string? conversationId = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            if (audioFile == null || audioFile.Length == 0)
            {
                throw new ArgumentException("Audio file is required and cannot be empty.");
            }

            if (audioFile.Length > MaxFileSizeInBytes)
            {
                throw new ArgumentException($"File size exceeds the maximum allowed limit of {MaxFileSizeInBytes / (1024 * 1024)} MB.");
            }

            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Unsupported audio format. Allowed formats: {string.Join(", ", _allowedExtensions)}");
            }

            var contentType = audioFile.ContentType.ToLowerInvariant();
            if (!_allowedMimeTypes.Contains(contentType))
            {
                // Note: Sometimes browsers send different mime types, so we check both extension and a list of common mimes.
                // It's safer to just rely on the extension if mime type isn't perfectly matched, but strictly we could enforce both.
                // We'll enforce both as per strict requirements.
                throw new ArgumentException($"Unsupported MIME type. Allowed MIME types: {string.Join(", ", _allowedMimeTypes)}");
            }

            _logger.LogInformation("Starting voice processing for user {UserId}. File size: {FileSize} bytes.", userId, audioFile.Length);

            string transcribedText;
            try
            {
                transcribedText = await _speechToTextService.TranscribeAsync(audioFile, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Speech provider failure for user {UserId}.", userId);
                throw new InvalidOperationException("Failed to transcribe the audio file.", ex);
            }

            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                throw new ApplicationException("Empty transcription result."); // Will map to 422 in controller
            }

            _logger.LogInformation("Transcription successful for user {UserId}. Length: {Length} characters.", userId, transcribedText.Length);
            
            // Print the text to the console as requested by the user
            Console.WriteLine("=====================================================");
            Console.WriteLine($"Transcribed Text (User: {userId}):");
            Console.WriteLine(transcribedText);
            Console.WriteLine("=====================================================");

            var chatRequest = new ChatRequestDto
            {
                UserId = userId,
                Message = transcribedText,
                ConversationId = conversationId
            };

            ChatReplyDto chatReply;
            try
            {
                chatReply = await _chatService.SendMessageAsync(chatRequest, cancellationToken);
                _logger.LogInformation("n8n Chat service responded successfully for user {UserId}.", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "n8n Failure for user {UserId} after voice transcription.", userId);
                throw; // Bubble up to controller for 502/500 mapping
            }

            stopwatch.Stop();
            _logger.LogInformation("Voice message processing completed for user {UserId} in {ElapsedMilliseconds}ms.", userId, stopwatch.ElapsedMilliseconds);

            return chatReply;
        }
    }
}
