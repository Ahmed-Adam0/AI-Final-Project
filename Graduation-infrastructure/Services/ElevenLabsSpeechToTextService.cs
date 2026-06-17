using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.IServices;
using Graduation_Application.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Graduation_infrastructure.Services
{
    public class ElevenLabsSpeechToTextService : ISpeechToTextService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ElevenLabsOptions _options;
        private readonly ILogger<ElevenLabsSpeechToTextService> _logger;

        public ElevenLabsSpeechToTextService(
            IHttpClientFactory httpClientFactory,
            IOptions<ElevenLabsOptions> options,
            ILogger<ElevenLabsSpeechToTextService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> TranscribeAsync(IFormFile audioFile, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("ElevenLabs API Key is not configured in appsettings.json.");
            }

            _logger.LogInformation("Starting ElevenLabs Scribe transcription for file {FileName}.", audioFile.FileName);

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("xi-api-key", _options.ApiKey);

            using var content = new MultipartFormDataContent();
            
            // Read file into stream
            using var stream = audioFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(audioFile.ContentType);
            
            content.Add(fileContent, "file", audioFile.FileName);
            content.Add(new StringContent("scribe_v1"), "model_id");

            var response = await client.PostAsync("https://api.elevenlabs.io/v1/speech-to-text", content, cancellationToken);
            
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ElevenLabs API call failed with status {StatusCode}. Body: {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException($"Speech-to-Text provider failed. Status: {response.StatusCode}");
            }

            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (document.RootElement.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();
                    return text ?? string.Empty;
                }

                _logger.LogError("ElevenLabs response did not contain 'text' property. Body: {Body}", responseBody);
                throw new InvalidOperationException("Speech-to-Text provider returned an invalid response format.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse ElevenLabs response JSON. Body: {Body}", responseBody);
                throw new InvalidOperationException("Speech-to-Text provider returned malformed JSON.", ex);
            }
        }
    }
}
