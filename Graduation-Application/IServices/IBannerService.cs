using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.BannerDTO;

namespace Graduation_Application.IServices
{
    public interface IBannerService
    {
        Task<List<BannerDto>> GetAllBannersAsync();
        Task<BannerDto?> GetBannerByIdAsync(int id);
        Task<BannerResponseDto> CreateBannerAsync(CreateBannerDto createBannerDto);
        Task<BannerResponseDto> UpdateBannerAsync(int bannerId, UpdateBannerDto updateBannerDto);
        Task<bool> DeleteBannerAsync(int bannerId);
        Task<bool> UpdateBannerStatusAsync(int bannerId, bool isActive);
    }
}
