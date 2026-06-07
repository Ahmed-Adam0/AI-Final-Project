using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.BannerDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class BannerService : IBannerService
    {
        private readonly IGenaricRepositories<Banner> _repository;

        public BannerService(IGenaricRepositories<Banner> repository)
        {
            _repository = repository;
        }

        public async Task<List<BannerDto>> GetAllBannersAsync()
        {
            var banners = await _repository
                .GetAllAsNoTracking()
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            return banners.Adapt<List<BannerDto>>();
        }

        public async Task<BannerDto?> GetBannerByIdAsync(int id)
        {
            var banner = await _repository.GetByIdAsync(id);
            return banner?.Adapt<BannerDto>();
        }

        public async Task<BannerResponseDto> CreateBannerAsync(CreateBannerDto createBannerDto)
        {
            if (createBannerDto == null)
                throw new ArgumentNullException(nameof(createBannerDto));

            var banner = new Banner
            {
                TitleAr = createBannerDto.TitleAr,
                TitleEn = createBannerDto.TitleEn,
                DescriptionAr = createBannerDto.DescriptionAr,
                DescriptionEn = createBannerDto.DescriptionEn,
                ImageUrl = createBannerDto.ImageUrl,
                RedirectUrl = createBannerDto.RedirectUrl,
                DisplayOrder = createBannerDto.DisplayOrder,
                IsActive = createBannerDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(banner);
            await _repository.SaveChangesAsync();

            return banner.Adapt<BannerResponseDto>();
        }

        public async Task<BannerResponseDto> UpdateBannerAsync(int bannerId, UpdateBannerDto updateBannerDto)
        {
            if (updateBannerDto == null)
                throw new ArgumentNullException(nameof(updateBannerDto));

            var banner = await _repository.GetByIdAsync(bannerId);
            if (banner == null)
                throw new ArgumentException($"Banner with ID {bannerId} not found.");

            banner.TitleAr = updateBannerDto.TitleAr;
            banner.TitleEn = updateBannerDto.TitleEn;
            banner.DescriptionAr = updateBannerDto.DescriptionAr;
            banner.DescriptionEn = updateBannerDto.DescriptionEn;
            banner.ImageUrl = updateBannerDto.ImageUrl;
            banner.RedirectUrl = updateBannerDto.RedirectUrl;
            banner.DisplayOrder = updateBannerDto.DisplayOrder;
            banner.IsActive = updateBannerDto.IsActive;
            banner.UpdatedAt = DateTime.UtcNow;

            _repository.Update(banner);
            await _repository.SaveChangesAsync();

            return banner.Adapt<BannerResponseDto>();
        }

        public async Task<bool> DeleteBannerAsync(int bannerId)
        {
            var banner = await _repository.GetByIdAsync(bannerId);
            if (banner == null)
                return false;

            _repository.Delete(banner);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateBannerStatusAsync(int bannerId, bool isActive)
        {
            var banner = await _repository.GetByIdAsync(bannerId);
            if (banner == null)
                return false;

            banner.IsActive = isActive;
            banner.UpdatedAt = DateTime.UtcNow;

            _repository.Update(banner);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
