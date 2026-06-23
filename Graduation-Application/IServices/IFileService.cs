using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IFileService
    {
        /// <summary>
        /// Saves an image under wwwroot/uploads/{folderName} and returns the full public URL.
        /// If oldFileUrl is provided the old file will be deleted.
        /// </summary>
        Task<string> SaveImageAsync(IFormFile file, string folderName, string? oldFileUrl = null);

        /// <summary>
        /// Deletes a file identified by a full URL or a relative path.
        /// </summary>
        Task DeleteAsync(string fileUrl);

        /// <summary>
        /// Deletes a raw file (like 3D models) identified by a full URL.
        /// </summary>
        Task DeleteRawAsync(string fileUrl);

        /// <summary>
        /// Saves a 3D model file under Cloudinary and returns the full secure URL.
        /// If oldFileUrl is provided, the old file will be deleted.
        /// </summary>
        Task<string> Save3DModelAsync(IFormFile file, string folderName, string? oldFileUrl = null);
    }
}
