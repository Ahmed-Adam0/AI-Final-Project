using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Graduation_infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        public FileService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private string GetWebRootPath()
        {
            return _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        private string GetBaseUrl()
        {
            var req = _httpContextAccessor.HttpContext?.Request;
            if (req == null) return string.Empty;
            return $"{req.Scheme}://{req.Host.Value}";
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folderName, string? oldFileUrl = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided", nameof(file));

            if (file.Length > _maxFileSize)
                throw new ArgumentException("File size exceeds limit of 5MB", nameof(file));

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
                throw new ArgumentException("Invalid file type. Only jpg, jpeg and png are allowed.", nameof(file));

            var uploadsRoot = Path.Combine(GetWebRootPath(), "uploads");
            var targetFolder = Path.Combine(uploadsRoot, folderName);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(targetFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Delete old file if provided
            if (!string.IsNullOrWhiteSpace(oldFileUrl))
            {
                await DeleteAsync(oldFileUrl);
            }

            var relativeUrl = $"/uploads/{folderName}/{fileName}";
            var baseUrl = GetBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                // fallback to relative URL if no request context
                return relativeUrl;
            }

            return baseUrl.TrimEnd('/') + relativeUrl;
        }

        public Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.CompletedTask;

            string relativePath = fileUrl;

            // if a full URL is provided extract the path
            if (Uri.IsWellFormedUriString(fileUrl, UriKind.Absolute))
            {
                try
                {
                    var uri = new Uri(fileUrl);
                    relativePath = uri.AbsolutePath;
                }
                catch
                {
                    relativePath = fileUrl;
                }
            }

            // Normalize and map to physical path
            relativePath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(GetWebRootPath(), relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
