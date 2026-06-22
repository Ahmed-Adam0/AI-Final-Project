using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminProductDTO;
using Graduation_Application.DTOs.Common;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
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
        private readonly IGenaricRepositories<Workshop> _workshopRepository;

        public AdminProductService(
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<ProductReport> reportRepository,
            IGenaricRepositories<Review> reviewRepository,
            IGenaricRepositories<Category> categoryRepository,
            IGenaricRepositories<ApplicationUser> userRepository,
            IGenaricRepositories<Workshop> workshopRepository
        )
        {
            _productRepository = productRepository;
            _reportRepository = reportRepository;
            _reviewRepository = reviewRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _workshopRepository = workshopRepository;
        }

        public async Task<PaginatedResult<AdminProductListDto>> GetProductsAsync(
            AdminProductFilterDto filter
        )
        {
            int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            IQueryable<Product> query = _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                    .ThenInclude(w => w.User)
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

            // Filter by category (3-tier)
            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
            {
                query = query.Where(p => p.ProductType.SubCategory.CategoryId == filter.CategoryId.Value);
            }

            if (filter.SubCategoryId.HasValue && filter.SubCategoryId > 0)
            {
                query = query.Where(p => p.ProductType.SubCategoryId == filter.SubCategoryId.Value);
            }

            if (filter.ProductTypeId.HasValue && filter.ProductTypeId > 0)
            {
                query = query.Where(p => p.ProductTypeId == filter.ProductTypeId.Value);
            }

            // Filter by vendor (workshopId via product)
            if (!string.IsNullOrWhiteSpace(filter.VendorId))
            {
                if (int.TryParse(filter.VendorId, out int workshopId))
                    query = query.Where(p => p.WorkshopId == workshopId);
            }

            // Filter by hidden status
            if (filter.IsHidden.HasValue)
            {
                query = query.Where(p => p.IsHidden == filter.IsHidden.Value);
            }

            int totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDtos = products
                .Select(p =>
                {
                    var minPrice = p.BasePrice;

                    return new AdminProductListDto
                    {
                        Id = p.Id,
                        NameAr = p.NameAr,
                        NameEn = p.NameEn,
                        CategoryName = p.ProductType != null && p.ProductType.SubCategory != null && p.ProductType.SubCategory.Category != null 
                            ? p.ProductType.SubCategory.Category.NameEn : string.Empty,
                        CategoryNameAr = p.ProductType != null && p.ProductType.SubCategory != null && p.ProductType.SubCategory.Category != null 
                            ? p.ProductType.SubCategory.Category.NameAr : string.Empty,
                        VendorName = p.Workshop?.User?.FullName ?? "N/A",
                        Price = minPrice,
                        IsHidden = p.IsHidden,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        MainImageUrl = GetMainImageUrl(p.Images),
                    };
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
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                    .ThenInclude(w => w.User)
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

            var minPrice = product.BasePrice;

            return new AdminProductDetailsDto
            {
                Id = product.Id,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                Price = minPrice,
                IsHidden = product.IsHidden,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                ProductTypeId = product.ProductTypeId,
                ProductTypeNameAr = product.ProductType?.NameAr ?? string.Empty,
                ProductTypeNameEn = product.ProductType?.NameEn ?? string.Empty,
                SubCategoryId = product.ProductType != null ? product.ProductType.SubCategoryId : 0,
                SubCategoryNameAr = product.ProductType?.SubCategory?.NameAr ?? string.Empty,
                SubCategoryNameEn = product.ProductType?.SubCategory?.NameEn ?? string.Empty,
                CategoryId = product.ProductType != null && product.ProductType.SubCategory != null ? product.ProductType.SubCategory.CategoryId : 0,
                CategoryNameAr = product.ProductType?.SubCategory?.Category?.NameAr ?? string.Empty,
                CategoryNameEn = product.ProductType?.SubCategory?.Category?.NameEn ?? string.Empty,
                VendorId = product.Workshop?.UserId ?? string.Empty,
                VendorName = product.Workshop?.User?.FullName ?? "N/A",
                VendorEmail = product.Workshop?.User?.Email ?? "N/A",
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


        public async Task<bool> HideProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            // Hidden mapping: just use IsHidden = true
            product.IsHidden = true;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnhideProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.IsHidden = false;
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
            // Vendors are now identified by workshops that have at least one listing
            var vendors = await _workshopRepository
                .GetAllAsNoTracking()
                .Include(w => w.User)
                .Where(w => w.Products != null && w.Products.Any())
                .OrderBy(w => w.WorkshopNameEn)
                .ToListAsync();

            return vendors
                .Select(w => new VendorDropdownDto
                {
                    UserId = w.UserId,
                    FullName = w.User?.FullName ?? w.WorkshopNameEn,
                    Email = w.User?.Email ?? string.Empty,
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
