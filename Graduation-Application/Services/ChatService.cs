using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ChatDTO;
using Graduation_Application.IServices;
using Graduation_Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Graduation_Application.Services
{
    public class ChatService : IChatService
    {
        private const string ChatClientName = "N8NChatClient";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly N8NOptions _options;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IHttpClientFactory httpClientFactory,
            IOptions<N8NOptions> options,
            ILogger<ChatService> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ChatReplyDto> SendMessageAsync(
            ChatRequestDto request,
            CancellationToken cancellationToken = default
        )
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new ArgumentException("userId is required.", nameof(request.UserId));
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException("message is required.", nameof(request.Message));
            }

            if (string.IsNullOrWhiteSpace(_options.WebhookUrl))
            {
                throw new InvalidOperationException("N8N WebhookUrl is not configured.");
            }

            var client = _httpClientFactory.CreateClient(ChatClientName);
            var payload = new N8NChatRequestDto
            {
                UserId = request.UserId,
                ConversationId = request.ConversationId,
                Message = request.Message,
            };

            try
            {
                using var response = await client.PostAsJsonAsync(
                    _options.WebhookUrl,
                    payload,
                    cancellationToken
                );

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "n8n returned non-success status code {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseBody
                    );

                    throw new InvalidOperationException(
                        $"n8n call failed with status {(int)response.StatusCode}. Response: {responseBody}"
                    );
                }

                N8NChatResponseDto? n8nResponse;
                try
                {
                    n8nResponse = JsonSerializer.Deserialize<N8NChatResponseDto>(
                        responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to deserialize n8n response. Raw body: {Body}",
                        responseBody
                    );
                    throw new InvalidOperationException(
                        $"Chat service returned malformed JSON. Raw response: {responseBody}",
                        ex
                    );
                }

                if (n8nResponse == null)
                {
                    _logger.LogError("n8n response body was empty or invalid JSON. Raw body: {Body}", responseBody);
                    throw new InvalidOperationException(
                        $"Chat service returned an empty response. Raw response: {responseBody}"
                    );
                }

                if (n8nResponse.Success.HasValue && !n8nResponse.Success.Value)
                {
                    _logger.LogError("n8n returned success=false for user {UserId}.", request.UserId);
                    throw new InvalidOperationException("Chat service failed to process the request.");
                }

                var reply = string.IsNullOrWhiteSpace(n8nResponse.Reply)
                    ? n8nResponse.Output
                    : n8nResponse.Reply;

                if (string.IsNullOrWhiteSpace(reply))
                {
                    _logger.LogError("n8n response did not include a valid reply.");
                    throw new InvalidOperationException("Chat service returned an invalid reply.");
                }

                return new ChatReplyDto { Reply = reply };
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout while calling n8n webhook.");
                throw new TimeoutException("Request to chat service timed out.", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network failure while calling n8n webhook.");
                throw new HttpRequestException("Unable to reach chat service.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse n8n response JSON.");
                throw new InvalidOperationException("Chat service returned malformed data.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling n8n webhook.");
                throw;
            }
        }
    }
}
