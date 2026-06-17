using System;
using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Graduation_infrastructure.Services
{
    /// <summary>
    /// A mock implementation of the Speech-to-Text service for development and testing.
    /// In production, this would be replaced with a concrete provider like OpenAI Whisper or Azure Speech.
    /// </summary>
    public class MockSpeechToTextService : ISpeechToTextService
    {
        private readonly ILogger<MockSpeechToTextService> _logger;

        public MockSpeechToTextService(ILogger<MockSpeechToTextService> logger)
        {
            _logger = logger;
        }

        public async Task<string> TranscribeAsync(IFormFile audioFile, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock transcription started for file {FileName} with size {FileSize} bytes.", audioFile.FileName, audioFile.Length);

            // Simulate processing delay
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            _logger.LogInformation("Mock transcription completed.");

            // Return a dummy transcribed text
            return "This is a simulated transcription of the provided voice message.";
        }
    }
}
