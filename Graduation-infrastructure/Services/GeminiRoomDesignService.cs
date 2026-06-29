using Graduation_Application.DTOs.RoomDesignDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Graduation_infrastructure.Services
{
    public class GeminiRoomDesignService : IGeminiRoomDesignService
    {
        private readonly IGenaricRepositories<ProductImage> _productImageRepo;
        private readonly IFileService _fileService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiRoomDesignService> _logger;

        public GeminiRoomDesignService(
            IGenaricRepositories<ProductImage> productImageRepo,
            IFileService fileService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiRoomDesignService> logger)
        {
            _productImageRepo = productImageRepo;
            _fileService = fileService;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RoomDesignResponseDto> GenerateRoomDesignAsync(RoomDesignRequestDto request)
        {
            request.ProductIds ??= new List<int>();

            if (request.ProductIds.Count > 3)
            {
                throw new ArgumentException("Max input at request input_references: must have between 0 and 3 items.");
            }

            _logger.LogInformation($"Starting GenerateRoomDesignAsync. RoomImage size: {request.RoomImage?.Length}, Products count: {request.ProductIds.Count}");

            // 1. Convert Room Image to Base64
            string roomImageBase64 = await ConvertFormFileToBase64Async(request.RoomImage);
            _logger.LogInformation("Room image converted to base64 successfully.");

            return await GenerateRoomDesignInternalAsync(roomImageBase64, request.ProductIds);
        }

        public async Task<RoomDesignResponseDto> GenerateRoomDesignFromUrlAsync(string roomImageUrl, List<int> productIds)
        {
            productIds ??= new List<int>();

            if (productIds.Count > 3)
            {
                throw new ArgumentException("Max input at request input_references: must have between 0 and 3 items.");
            }

            _logger.LogInformation($"Starting GenerateRoomDesignFromUrlAsync. Products count: {productIds.Count}");

            // 1. Download Room Image and Convert to Base64
            string roomImageBase64 = await DownloadImageAsBase64Async(roomImageUrl);
            if (string.IsNullOrEmpty(roomImageBase64))
            {
                throw new Exception("Failed to download or convert the room image URL.");
            }
            _logger.LogInformation("Room image downloaded and converted to base64 successfully.");

            return await GenerateRoomDesignInternalAsync(roomImageBase64, productIds);
        }

        private async Task<RoomDesignResponseDto> GenerateRoomDesignInternalAsync(string roomImageBase64, List<int> productIds)
        {
            var productBase64Images = new List<string>();

            // 2. Fetch product images and convert to Base64
            foreach (var productId in productIds)
            {
                var primaryImage = await _productImageRepo
                    .WhereAsNoTracking(pi => pi.ProductId == productId && pi.IsPrimary)
                    .FirstOrDefaultAsync();

                if (primaryImage == null)
                {
                    // Fallback to any image if no primary is found
                    primaryImage = await _productImageRepo
                        .WhereAsNoTracking(pi => pi.ProductId == productId)
                        .FirstOrDefaultAsync();
                }

                if (primaryImage != null && !string.IsNullOrEmpty(primaryImage.ImageUrl))
                {
                    var base64 = await DownloadImageAsBase64Async(primaryImage.ImageUrl);
                    if (!string.IsNullOrEmpty(base64))
                    {
                        productBase64Images.Add(base64);
                        _logger.LogInformation($"Fetched and converted product image for ID {productId}");
                    }
                }
            }

            // 3. Construct Payload & Call API
            _logger.LogInformation("Calling Gemini API...");
            string generatedImageBase64 = await CallGeminiApiAsync(roomImageBase64, productBase64Images);
            _logger.LogInformation("Gemini API call completed.");

            // 4. Save Image using FileService and return URL
            if (string.IsNullOrEmpty(generatedImageBase64))
            {
                throw new Exception("Failed to generate image from Gemini API.");
            }
            
            if (generatedImageBase64.StartsWith("ERROR:"))
            {
                throw new Exception($"OpenRouter API returned unknown JSON format. Raw Response: {generatedImageBase64.Substring(6)}");
            }

            // If the API returned a direct URL instead of Base64
            if (generatedImageBase64.StartsWith("URL:"))
            {
                var directUrl = generatedImageBase64.Substring(4);
                return new RoomDesignResponseDto { GeneratedImageUrl = directUrl };
            }

            // Check if it's actually base64
            Span<byte> buffer = new Span<byte>(new byte[generatedImageBase64.Length]);
            if (!Convert.TryFromBase64String(generatedImageBase64, buffer, out _))
            {
                throw new Exception($"API did not return a valid Base64 string or URL. Response was: {generatedImageBase64}");
            }

            var generatedFile = ConvertBase64ToFormFile(generatedImageBase64, "generated_room.jpg");
            var url = await _fileService.SaveImageAsync(generatedFile, "room_designs");

            return new RoomDesignResponseDto { GeneratedImageUrl = url };
        }

        private async Task<string> ConvertFormFileToBase64Async(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            var mimeType = file.ContentType ?? "image/jpeg";
            return $"data:{mimeType};base64,{base64}";
        }

        private async Task<string> DownloadImageAsBase64Async(string url)
        {
            try
            {
                if (!url.StartsWith("http"))
                {
                    // Ensure the URL starts with a slash if it doesn't already
                    if (!url.StartsWith("/")) url = "/" + url;
                    url = "https://home-ai.runasp.net" + url;
                }

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var base64 = Convert.ToBase64String(bytes);
                return $"data:{mimeType};base64,{base64}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to download image from {url}");
                return string.Empty;
            }
        }

        private async Task<string> CallGeminiApiAsync(string roomImageBase64, List<string> productImagesBase64)
        {
            var apiKey = _configuration["OpenRouter:ApiKey"] ?? _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key for OpenRouter or Gemini is not configured.");
            }

            var baseUrl = _configuration["Gemini:BaseUrl"] ?? "https://openrouter.ai/api/v1/images";

            var inputReferences = new List<object>();
            
            // First image is the room
            inputReferences.Add(new
            {
                type = "image_url",
                image_url = new { url = roomImageBase64 }
            });
            
            // Remaining images are products
            foreach (var prodImg in productImagesBase64)
            {
                inputReferences.Add(new
                {
                    type = "image_url",
                    image_url = new { url = prodImg }
                });
            }

            var prompt = @"You are an expert interior designer.
The first uploaded image is an empty room.
All remaining uploaded images are real furniture products.
Place all uploaded furniture naturally inside the room.
Preserve:
* room dimensions
* perspective
* lighting
* walls
* windows
* floor
* ceiling
* camera angle
Do NOT redesign the room.
Do NOT replace furniture.
Do NOT generate similar furniture.
Use the exact uploaded furniture.
Arrange them professionally.
Maintain realistic proportions and shadows.
Return ONLY the edited room image.";

            var payload = new
            {
                model = "google/gemini-2.5-flash-image", // Specific model for Image Generation
                prompt = prompt,
                input_references = inputReferences
            };
            
            var payloadJson = JsonSerializer.Serialize(payload);
            _logger.LogInformation($"Sending Payload to {baseUrl}");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, baseUrl);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            requestMessage.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API Error: {responseContent}");
                throw new Exception($"Gemini API request failed with status code {response.StatusCode}: {responseContent}");
            }

            // Extract the generated image Base64. 
            // Depending on the exact API response format, this will need to be parsed.
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            // Handle standard Image Generation response format: {"data": [{"b64_json": "..."}]}
            if (root.TryGetProperty("data", out var dataArr) && dataArr.GetArrayLength() > 0)
            {
                if (dataArr[0].TryGetProperty("b64_json", out var b64Prop))
                {
                    var b64 = b64Prop.GetString();
                    if (!string.IsNullOrEmpty(b64)) return b64.Trim();
                }
            }

            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                if (choices[0].TryGetProperty("message", out var messageProp) && messageProp.TryGetProperty("content", out var contentProp))
                {
                    var content = contentProp.GetString();
                    if (content == null) 
                    {
                        _logger.LogWarning("Content is null inside choices[0].message");
                        return $"ERROR: Content is null inside choices. Raw response: {responseContent}";
                    }

                // Check if the AI returned a JSON string (like OpenAI's image generation format) inside the content
                if (content.TrimStart().StartsWith("{") && content.Contains("b64_json"))
                {
                    try
                    {
                        using var innerDoc = JsonDocument.Parse(content);
                        if (innerDoc.RootElement.TryGetProperty("data", out var innerDataArr) && innerDataArr.GetArrayLength() > 0)
                        {
                            var b64 = innerDataArr[0].GetProperty("b64_json").GetString();
                            if (!string.IsNullOrEmpty(b64)) return b64.Trim();
                        }
                    }
                    catch { /* Fallback to standard processing if parsing fails */ }
                }

                // If the model returns markdown like "![image](data:image/jpeg;base64,.....)" we extract it
                if (content.Contains("base64,"))
                {
                    var base64Part = content.Substring(content.IndexOf("base64,") + 7);
                    var endIdx = base64Part.IndexOf(")");
                    if (endIdx > 0) base64Part = base64Part.Substring(0, endIdx);
                    return base64Part.Trim();
                }
                
                // If it returns a markdown URL or plain text URL
                if (content.Contains("http"))
                {
                    var urlPart = content.Substring(content.IndexOf("http"));
                    var endIdx = urlPart.IndexOf(")");
                    if (endIdx > 0) urlPart = urlPart.Substring(0, endIdx);
                    
                    var spaceIdx = urlPart.IndexOf(" ");
                    if (spaceIdx > 0) urlPart = urlPart.Substring(0, spaceIdx);

                    return $"URL:{urlPart.Trim()}";
                }
                
                // If the model returns the base64 directly
                return content.Trim();
                }
            }

            return $"ERROR:{responseContent}";
        }

        private IFormFile ConvertBase64ToFormFile(string base64, string fileName)
        {
            var bytes = Convert.FromBase64String(base64);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }
    }
}
