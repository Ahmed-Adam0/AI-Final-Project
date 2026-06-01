using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminProductDTO;
using Graduation_Application.DTOs.Common;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Graduation_Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services.Admin
{
    public class AdminProductService : IAdminProductService
    {
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<ProductReport> _reportRepository;
        private readonly IGenaricRepositories<Review> _reviewRepository;
        private readonly IGenaricRepositories<Category> _categoryRepository;
        private readonly IGenaricRepositories<ApplicationUser> _userRepository;

        public AdminProductService(
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<ProductReport> reportRepository,
            IGenaricRepositories<Review> reviewRepository,
            IGenaricRepositories<Category> categoryRepository,
            IGenaricRepositories<ApplicationUser> userRepository
        )
        {
            _productRepository = productRepository;
            _reportRepository = reportRepository;
            _reviewRepository = reviewRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
        }

        public async Task<PaginatedResult<AdminProductListDto>> GetProductsAsync(
            AdminProductFilterDto filter
        )
        {
            int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            IQueryable<Product> query = _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Images);

            // Filter by search term (name)
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string searchTerm = filter.Search.ToLower();
                query = query.Where(p =>
                    p.NameAr.ToLower().Contains(searchTerm)
                    || p.NameEn.ToLower().Contains(searchTerm)
                );
            }

            // Filter by category
            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            // Filter by vendor (userId)
            if (!string.IsNullOrWhiteSpace(filter.VendorId))
            {
                query = query.Where(p => p.UserId == filter.VendorId);
            }

            // Filter by status
            if (filter.Status.HasValue)
            {
                query = query.Where(p => p.Status == filter.Status.Value);
            }

            int totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDtos = products
                .Select(p => new AdminProductListDto
                {
                    Id = p.Id,
                    NameAr = p.NameAr,
                    NameEn = p.NameEn,
                    CategoryName = p.Category != null ? p.Category.NameEn : string.Empty,
                    VendorName = p.User != null ? p.User.FullName : "N/A",
                    Price = p.Price,
                    Status = p.Status,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    MainImageUrl = GetMainImageUrl(p.Images),
                })
                .ToList();

            return new PaginatedResult<AdminProductListDto>(
                productDtos,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<AdminProductDetailsDto> GetProductDetailsAsync(int id)
        {
            var product = await _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Workshop)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            // Get reviews stats
            var reviews = await _reviewRepository
                .WhereAsNoTracking(r => r.ProductId == id)
                .ToListAsync();
            int reviewsCount = reviews.Count;
            double averageRating = reviewsCount > 0 ? reviews.Average(r => r.Rating) : 0;

            return new AdminProductDetailsDto
            {
                Id = product.Id,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                Price = product.Price,
                Status = product.Status,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                CategoryId = product.CategoryId,
                CategoryNameAr = product.Category?.NameAr ?? string.Empty,
                CategoryNameEn = product.Category?.NameEn ?? string.Empty,
                VendorId = product.UserId,
                VendorName = product.User?.FullName ?? "N/A",
                VendorEmail = product.User?.Email ?? "N/A",
                WorkshopId = product.WorkshopId,
                WorkshopNameAr = product.Workshop?.WorkshopNameAr ?? string.Empty,
                WorkshopNameEn = product.Workshop?.WorkshopNameEn ?? string.Empty,
                Images =
                    product
                        .Images?.Select(i => new AdminProductImageDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            IsPrimary = i.IsPrimary,
                        })
                        .ToList()
                    ?? new List<AdminProductImageDto>(),
                ReviewsCount = reviewsCount,
                AverageRating = Math.Round(averageRating, 1),
            };
        }

        public async Task<bool> ActivateProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.Status = ProductStatus.Active;
            product.IsActive = true;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.Status = ProductStatus.Inactive;
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HideProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.Status = ProductStatus.Hidden;
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.Status = ProductStatus.Active;
            product.IsActive = true;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReportedProductDto>> GetReportedProductsAsync()
        {
            var reports = await _reportRepository
                .GetAllAsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports
                .Select(r => new ReportedProductDto
                {
                    ReportId = r.Id,
                    ProductId = r.ProductId,
                    ProductNameEn = r.Product?.NameEn ?? "Unknown",
                    ProductNameAr = r.Product?.NameAr ?? "Unknown",
                    Reason = r.Reason,
                    ReportedByName = r.User?.FullName ?? "Unknown",
                    ReportedByEmail = r.User?.Email ?? "Unknown",
                    CreatedAt = r.CreatedAt,
                    IsResolved = r.IsResolved,
                })
                .ToList();
        }

        public async Task<bool> ResolveReportAsync(int reportId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
                return false;

            report.IsResolved = true;
            report.UpdatedAt = DateTime.UtcNow;

            _reportRepository.Update(report);
            await _reportRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryDropdownDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository
                .GetAllAsNoTracking()
                .OrderBy(c => c.NameEn)
                .ToListAsync();

            return categories
                .Select(c => new CategoryDropdownDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                })
                .ToList();
        }

        public async Task<List<VendorDropdownDto>> GetAllVendorsAsync()
        {
            // Get distinct vendor user IDs from products
            var vendorUserIds = await _productRepository
                .GetAllAsNoTracking()
                .Where(p => p.UserId != null)
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();

            if (!vendorUserIds.Any())
                return new List<VendorDropdownDto>();

            var vendors = await _userRepository
                .GetAllAsNoTracking()
                .Where(u => vendorUserIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return vendors
                .Select(u => new VendorDropdownDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                })
                .ToList();
        }

        private static string GetMainImageUrl(List<ProductImage> images)
        {
            if (images == null || images.Count == 0)
                return string.Empty;

            var primaryImage = images.FirstOrDefault(img => img.IsPrimary);
            return primaryImage != null
                ? primaryImage.ImageUrl
                : images.FirstOrDefault()?.ImageUrl ?? string.Empty;
        }
    }
}
