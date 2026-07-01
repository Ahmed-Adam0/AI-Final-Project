using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Graduation_infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public FileService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["CloudinarySettings:CloudName"],
                configuration["CloudinarySettings:ApiKey"],
                configuration["CloudinarySettings:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folderName, string? oldFileUrl = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Array.Exists(allowedExtensions, e => e == ext))
                throw new ArgumentException("Only jpg, jpeg, png allowed");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result == null || result.SecureUrl == null)
                throw new Exception("Upload failed");

            if (!string.IsNullOrWhiteSpace(oldFileUrl))
            {
                await DeleteAsync(oldFileUrl);
            }

            return result.SecureUrl.ToString();
        }

        public async Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            try
            {
                // استخرج الـ PublicId من الـ URL
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;
                if (segments.Length >= 2)
                {
                    var publicId = string.Join("", segments[^2..]).Replace(".jpg", "").Replace(".jpeg", "").Replace(".png", "").TrimEnd('/');

                    var deleteParams = new DeletionParams(publicId);
                    await _cloudinary.DestroyAsync(deleteParams);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting image from Cloudinary: {ex.Message}");
            }
        }

        public async Task<string> Save3DModelAsync(IFormFile file, string folderName, string? oldFileUrl = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            var allowedExtensions = new[] { ".glb", ".gltf", ".fbx", ".obj" };
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Array.Exists(allowedExtensions, e => e == ext))
                throw new ArgumentException("Only glb, gltf, fbx, obj allowed");

            if (!string.IsNullOrWhiteSpace(oldFileUrl))
            {
                await DeleteRawAsync(oldFileUrl);
            }

            await using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result == null || result.SecureUrl == null)
                throw new Exception("Upload failed");

            return result.SecureUrl.ToString();
        }

        public async Task DeleteRawAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;
                var publicId = string.Join("", segments[^2..]).TrimEnd('/');

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Raw
                };
                await _cloudinary.DestroyAsync(deleteParams);
            }
            catch
            {
                // Soft fail on delete errors
            }
        }
    }
}