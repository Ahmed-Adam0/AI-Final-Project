using System.Threading.Tasks;
using Graduation_Application.DTOs.VendorDTO;
using Microsoft.AspNetCore.Http;

namespace Graduation_Application.IServices
{
    public interface IVendorService
    {
        Task CreateVendorAsync(CreateVendorDto dto);
        Task UpdateVendorLogoAsync(string userId, IFormFile logo);
        Task<VendorAuthResponseDto> VendorLoginAsync(VendorLoginDto dto);
        Task<VendorProfileDto> GetVendorProfileAsync(string userId);
        Task<VendorProfileDto> UpdateVendorProfileAsync(string userId, UpdateVendorProfileDto dto);
        Task<int> GetVendorProductCountAsync(string userId);
        Task LinkVendorProductsAsync(string userId);
    }
}
