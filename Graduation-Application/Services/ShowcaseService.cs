using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.DTOs.ShowcaseDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class ShowcaseService : IShowcaseService
    {
        private readonly IGenaricRepositories<ShowcaseSlide> _slideRepository;
        private readonly IGenaricRepositories<Review> _reviewRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<ShowcaseHotspot> _hotspotRepository;
        private readonly IFileService _fileService;

        public ShowcaseService(
            IGenaricRepositories<ShowcaseSlide> slideRepository,
            IGenaricRepositories<Review> reviewRepository,
            ILocalizationService localizationService,
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<ShowcaseHotspot> hotspotRepository,
            IFileService fileService)
        {
            _slideRepository = slideRepository;
            _reviewRepository = reviewRepository;
            _localizationService = localizationService;
            _productRepository = productRepository;
            _hotspotRepository = hotspotRepository;
            _fileService = fileService;
        }

        public async Task<List<ShowcaseSlideDto>> GetActiveShowcaseAsync()
        {
            var culture = _localizationService.GetCurrentCulture();
            var isArabic = "ar".Equals(culture, StringComparison.OrdinalIgnoreCase);

            // Fetch slides and only active hotspots ordered by DisplayOrder
            var slides = await _slideRepository.GetAllAsNoTracking()
                .Include(s => s.Hotspots.Where(h => h.IsActive).OrderBy(h => h.DisplayOrder))
                    .ThenInclude(h => h.Product)
                        .ThenInclude(p => p.Images)
                .Include(s => s.Hotspots.Where(h => h.IsActive).OrderBy(h => h.DisplayOrder))
                    .ThenInclude(h => h.Product)
                        .ThenInclude(p => p.Workshop)
                .Include(s => s.Hotspots.Where(h => h.IsActive).OrderBy(h => h.DisplayOrder))
                    .ThenInclude(h => h.Product)
                        .ThenInclude(p => p.ProductType)
                            .ThenInclude(pt => pt.SubCategory)
                                .ThenInclude(sc => sc.Category)
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            var productIds = slides
                .SelectMany(s => s.Hotspots)
                .Select(h => h.ProductId)
                .Distinct()
                .ToList();

            Dictionary<int, decimal> ratingDict = new();
            if (productIds.Any())
            {
                ratingDict = await _reviewRepository.GetAllAsNoTracking()
                    .Where(r => productIds.Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, AvgRating = g.Average(r => r.Rating) })
                    .ToDictionaryAsync(x => x.ProductId, x => Math.Round((decimal)x.AvgRating, 2));
            }

            var slideDtos = new List<ShowcaseSlideDto>();
            foreach (var slide in slides)
            {
                var slideDto = new ShowcaseSlideDto
                {
                    Id = slide.Id,
                    TitleAr = slide.TitleAr,
                    TitleEn = slide.TitleEn,
                    SubtitleAr = slide.SubtitleAr,
                    SubtitleEn = slide.SubtitleEn,
                    ButtonTextAr = slide.ButtonTextAr,
                    ButtonTextEn = slide.ButtonTextEn,
                    ButtonLink = slide.ButtonLink,
                    BackgroundImageUrl = slide.BackgroundImageUrl,
                    DisplayOrder = slide.DisplayOrder,
                    IsActive = slide.IsActive,
                    WorkshopId = slide.WorkshopId,
                    Title = isArabic ? slide.TitleAr : slide.TitleEn,
                    Subtitle = (isArabic ? slide.SubtitleAr : slide.SubtitleEn) ?? string.Empty,
                    ButtonText = (isArabic ? slide.ButtonTextAr : slide.ButtonTextEn) ?? string.Empty
                };

                foreach (var hotspot in slide.Hotspots)
                {
                    var productDto = hotspot.Product.Adapt<ProductDto>();
                    
                    productDto.AverageRating = ratingDict.TryGetValue(hotspot.ProductId, out var rating) ? rating : 0m;
                    productDto.Discount = null; // Left null as discounts are not implemented

                    slideDto.Hotspots.Add(new ShowcaseHotspotDto
                    {
                        Id = hotspot.Id,
                        Product = productDto,
                        X = hotspot.X,
                        Y = hotspot.Y,
                        DisplayOrder = hotspot.DisplayOrder,
                        IsActive = hotspot.IsActive
                    });
                }

                slideDtos.Add(slideDto);
            }

            return slideDtos;
        }

        public async Task<ShowcaseSlideDto> CreateShowcaseSlideAsync(int workshopId, CreateShowcaseSlideRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var hotspots = ParseHotspots(request.Hotspots, request.HotspotsJson);

            // Validate hotspots and product ownership
            foreach (var h in hotspots)
            {
                if (h.X < 0 || h.X > 100 || h.Y < 0 || h.Y > 100)
                {
                    throw new ArgumentException("X and Y coordinates must be between 0 and 100.");
                }

                var product = await _productRepository.GetByIdAsNoTrackingAsync(h.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {h.ProductId} does not exist.");
                }

                if (product.WorkshopId != workshopId)
                {
                    throw new UnauthorizedAccessException($"Product with ID {h.ProductId} does not belong to your workshop.");
                }
            }

            // Save background image to Cloudinary
            string imageUrl;
            try
            {
                imageUrl = await _fileService.SaveImageAsync(request.BackgroundImage, "showcase");
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Failed to upload background image to Cloudinary: " + ex.Message);
            }

            // Create slide entity with nested hotspots to save atomically in a single transaction
            var slide = new ShowcaseSlide
            {
                TitleAr = request.TitleAr,
                TitleEn = request.TitleEn,
                SubtitleAr = request.SubtitleAr,
                SubtitleEn = request.SubtitleEn,
                ButtonTextAr = request.ButtonTextAr,
                ButtonTextEn = request.ButtonTextEn,
                ButtonLink = request.ButtonLink,
                BackgroundImageUrl = imageUrl,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                WorkshopId = workshopId,
                CreatedAt = DateTime.UtcNow,
                Hotspots = hotspots.Select(h => new ShowcaseHotspot
                {
                    ProductId = h.ProductId,
                    X = h.X,
                    Y = h.Y,
                    DisplayOrder = h.DisplayOrder,
                    IsActive = h.IsActive,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };

            await _slideRepository.AddAsync(slide);
            await _slideRepository.SaveChangesAsync();

            return await MapToDtoAsync(slide.Id);
        }

        public async Task<ShowcaseSlideDto> UpdateShowcaseSlideAsync(int id, int workshopId, UpdateShowcaseSlideRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Fetch existing slide with hotspots
            var slide = await _slideRepository.GetAll()
                .Include(s => s.Hotspots)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (slide == null)
            {
                throw new ArgumentException($"Showcase slide with ID {id} not found.");
            }

            // Verify ownership
            if (slide.WorkshopId != workshopId)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this showcase slide.");
            }

            var hotspots = ParseHotspots(request.Hotspots, request.HotspotsJson);

            // Validate hotspots and product ownership
            foreach (var h in hotspots)
            {
                if (h.X < 0 || h.X > 100 || h.Y < 0 || h.Y > 100)
                {
                    throw new ArgumentException("X and Y coordinates must be between 0 and 100.");
                }

                var product = await _productRepository.GetByIdAsNoTrackingAsync(h.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {h.ProductId} does not exist.");
                }

                if (product.WorkshopId != workshopId)
                {
                    throw new UnauthorizedAccessException($"Product with ID {h.ProductId} does not belong to your workshop.");
                }
            }

            // Handle image upload if a new background image is provided
            string imageUrl = slide.BackgroundImageUrl;
            if (request.BackgroundImage != null && request.BackgroundImage.Length > 0)
            {
                try
                {
                    imageUrl = await _fileService.SaveImageAsync(request.BackgroundImage, "showcase", slide.BackgroundImageUrl);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Failed to upload new background image: " + ex.Message);
                }
            }

            // Update slide properties
            slide.TitleAr = request.TitleAr;
            slide.TitleEn = request.TitleEn;
            slide.SubtitleAr = request.SubtitleAr;
            slide.SubtitleEn = request.SubtitleEn;
            slide.ButtonTextAr = request.ButtonTextAr;
            slide.ButtonTextEn = request.ButtonTextEn;
            slide.ButtonLink = request.ButtonLink;
            slide.BackgroundImageUrl = imageUrl;
            slide.DisplayOrder = request.DisplayOrder;
            slide.IsActive = request.IsActive;
            slide.UpdatedAt = DateTime.UtcNow;

            // Remove old hotspots and attach new ones atomically
            _hotspotRepository.DeleteRange(slide.Hotspots);
            
            slide.Hotspots = hotspots.Select(h => new ShowcaseHotspot
            {
                ShowcaseSlideId = slide.Id,
                ProductId = h.ProductId,
                X = h.X,
                Y = h.Y,
                DisplayOrder = h.DisplayOrder,
                IsActive = h.IsActive,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _slideRepository.Update(slide);
            await _slideRepository.SaveChangesAsync();

            return await MapToDtoAsync(slide.Id);
        }

        public async Task<bool> DeleteShowcaseSlideAsync(int id, int workshopId)
        {
            var slide = await _slideRepository.GetAll()
                .Include(s => s.Hotspots)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (slide == null)
            {
                return false;
            }

            // Verify ownership
            if (slide.WorkshopId != workshopId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this showcase slide.");
            }

            // Delete background image from Cloudinary
            if (!string.IsNullOrEmpty(slide.BackgroundImageUrl))
            {
                try
                {
                    await _fileService.DeleteAsync(slide.BackgroundImageUrl);
                }
                catch { /* non-blocking */ }
            }

            // Delete slide entity along with nested hotspots
            _hotspotRepository.DeleteRange(slide.Hotspots);
            _slideRepository.Delete(slide);
            await _slideRepository.SaveChangesAsync();

            return true;
        }

        private List<CreateShowcaseHotspotRequest> ParseHotspots(List<CreateShowcaseHotspotRequest>? hotspots, string? hotspotsJson)
        {
            var finalHotspots = hotspots ?? new List<CreateShowcaseHotspotRequest>();
            if (!string.IsNullOrEmpty(hotspotsJson))
            {
                try
                {
                    var parsed = System.Text.Json.JsonSerializer.Deserialize<List<CreateShowcaseHotspotRequest>>(
                        hotspotsJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    if (parsed != null)
                    {
                        finalHotspots = parsed;
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Invalid hotspots JSON format: " + ex.Message);
                }
            }
            return finalHotspots;
        }

        private async Task<ShowcaseSlideDto> MapToDtoAsync(int slideId)
        {
            var culture = _localizationService.GetCurrentCulture();
            var isArabic = "ar".Equals(culture, StringComparison.OrdinalIgnoreCase);

            var slide = await _slideRepository.GetAllAsNoTracking()
                .Include(s => s.Hotspots.OrderBy(h => h.DisplayOrder))
                    .ThenInclude(h => h.Product)
                        .ThenInclude(p => p.Images)
                .Include(s => s.Hotspots)
                    .ThenInclude(h => h.Product)
                        .ThenInclude(p => p.Workshop)
                .Include(s => s.Hotspots)
                    .ThenInclude(h => h.Product)
                        .ThenInclude(p => p.ProductType)
                            .ThenInclude(pt => pt.SubCategory)
                                .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(s => s.Id == slideId);

            if (slide == null) return null!;

            var productIds = slide.Hotspots.Select(h => h.ProductId).Distinct().ToList();
            Dictionary<int, decimal> ratingDict = new();
            if (productIds.Any())
            {
                ratingDict = await _reviewRepository.GetAllAsNoTracking()
                    .Where(r => productIds.Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, AvgRating = g.Average(r => r.Rating) })
                    .ToDictionaryAsync(x => x.ProductId, x => Math.Round((decimal)x.AvgRating, 2));
            }

            var slideDto = new ShowcaseSlideDto
            {
                Id = slide.Id,
                TitleAr = slide.TitleAr,
                TitleEn = slide.TitleEn,
                SubtitleAr = slide.SubtitleAr,
                SubtitleEn = slide.SubtitleEn,
                ButtonTextAr = slide.ButtonTextAr,
                ButtonTextEn = slide.ButtonTextEn,
                ButtonLink = slide.ButtonLink,
                BackgroundImageUrl = slide.BackgroundImageUrl,
                DisplayOrder = slide.DisplayOrder,
                IsActive = slide.IsActive,
                WorkshopId = slide.WorkshopId,
                Title = isArabic ? slide.TitleAr : slide.TitleEn,
                Subtitle = (isArabic ? slide.SubtitleAr : slide.SubtitleEn) ?? string.Empty,
                ButtonText = (isArabic ? slide.ButtonTextAr : slide.ButtonTextEn) ?? string.Empty
            };

            foreach (var hotspot in slide.Hotspots)
            {
                var productDto = hotspot.Product.Adapt<ProductDto>();
                productDto.AverageRating = ratingDict.TryGetValue(hotspot.ProductId, out var rating) ? rating : 0m;
                productDto.Discount = null;

                slideDto.Hotspots.Add(new ShowcaseHotspotDto
                {
                    Id = hotspot.Id,
                    Product = productDto,
                    X = hotspot.X,
                    Y = hotspot.Y,
                    DisplayOrder = hotspot.DisplayOrder,
                    IsActive = hotspot.IsActive
                });
            }

            return slideDto;
        }
    }
}
