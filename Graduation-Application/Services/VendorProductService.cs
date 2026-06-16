using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class VendorProductService : IVendorProductService
    {
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<Review> _reviewRepository;

        public VendorProductService(
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<Review> reviewRepository)
        {
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        /// Get paginated list of products owned by vendor
        /// Uses UserId for ownership, falls back to workshop if UserId not available
        /// </summary>
        public async Task<PaginatedResult<ProductDto>> GetVendorProductsAsync(string userId, ProductFilterDto filter)
        {
            // Validate pagination
            int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            // Start query: get products owned by this vendor via their VendorProductListings
            IQueryable<Product> query = _productRepository.GetAllAsNoTracking()
                .Where(p => p.Workshop != null && p.Workshop.UserId == userId)
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Images);

            // Apply status filter
            bool activeFilter = filter.IsActive ?? true;
            query = query.Where(p => p.IsActive == activeFilter);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string searchTerm = filter.Search.ToLower();
                query = query.Where(p =>
                    p.NameAr.ToLower().Contains(searchTerm) ||
                    p.NameEn.ToLower().Contains(searchTerm) ||
                    p.DescriptionAr.ToLower().Contains(searchTerm) ||
                    p.DescriptionEn.ToLower().Contains(searchTerm)
                );
            }

            // Apply category filters (3-tier)
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

            // Apply price range filter (against minimum variant price across listings)
            if (filter.MinPrice.HasValue && filter.MinPrice > 0)
            {
                query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue && filter.MaxPrice > 0)
            {
                query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);
            }

            // Get total count
            int totalCount = await query.CountAsync();

            // Apply pagination
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
            var productDtos = products.Adapt<List<ProductDto>>();

            return new PaginatedResult<ProductDto>
            {
                Items = productDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        /// <summary>
        /// Get detailed information about specific vendor product
        /// </summary>
        public async Task<ProductDetailsDto> GetVendorProductDetailsAsync(string userId, int productId)
        {
            var product = await _productRepository.GetAllAsNoTracking()
                .Where(p => p.Id == productId
                    && p.Workshop != null && p.Workshop.UserId == userId
                    && p.IsActive)
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Attributes)
                    .ThenInclude(a => a.Values)
                .Include(p => p.Images)
                .FirstOrDefaultAsync();

            if (product == null)
                return null;

            return product.Adapt<ProductDetailsDto>();
        }

        /// <summary>
        /// Update product status (activate/deactivate)
        /// </summary>
        public async Task<ProductResponseDto> UpdateVendorProductStatusAsync(string userId, int productId, bool isActive)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            // Verify ownership via listings
            var isOwner = product.Workshop?.UserId == userId;
            if (!isOwner)
            {
                // Fallback: load workshop if not already included
                var hasListing = await _productRepository.GetAllAsNoTracking()
                    .Where(p => p.Id == productId && p.Workshop != null && p.Workshop.UserId == userId)
                    .AnyAsync();
                if (!hasListing)
                    throw new UnauthorizedAccessException("You do not have permission to modify this product.");
            }

            product.IsActive = isActive;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return product.Adapt<ProductResponseDto>();
        }

        /// <summary>
        /// Delete vendor product
        /// </summary>
        public async Task<bool> DeleteVendorProductAsync(string userId, int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            // Verify ownership via listings
            var hasListing = await _productRepository.GetAllAsNoTracking()
                .Where(p => p.Id == productId && p.Workshop != null && p.Workshop.UserId == userId)
                .AnyAsync();
            if (!hasListing)
                throw new UnauthorizedAccessException("You do not have permission to delete this product.");

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Get statistics for vendor dashboard
        /// </summary>
        public async Task<VendorProductStatsDto> GetVendorProductStatsAsync(string userId)
        {
            var products = await _productRepository.GetAllAsNoTracking()
                .Where(p => p.Workshop != null && p.Workshop.UserId == userId)
                .ToListAsync();

            var reviews = await _reviewRepository.GetAllAsNoTracking()
                .Where(r => r.Product.Workshop != null && r.Product.Workshop.UserId == userId)
                .ToListAsync();

            var totalProducts = products.Count;
            var activeProducts = products.Count(p => p.IsActive);
            var inactiveProducts = totalProducts - activeProducts;

            var averageRating = reviews.Count > 0
                ? Math.Round((decimal)reviews.Average(r => r.Rating), 2)
                : 0;

            // Revenue is now calculated from OrderItems with snapshots, not from Product.Price
            // Returning 0 here as a placeholder — connect to OrderItem.SnapshotUnitPrice for accuracy
            var totalRevenue = 0m;

            return new VendorProductStatsDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                InactiveProducts = inactiveProducts,
                AverageRating = averageRating,
                TotalReviews = reviews.Count,
                TotalRevenue = totalRevenue
            };
        }

        public async Task<IEnumerable<ProductDto>> GetVendorTopProductsAsync(string userId, int topCount = 5)
        {
            var products = await _productRepository.GetAllAsNoTracking()
                .Where(p => p.Workshop != null && p.Workshop.UserId == userId && p.IsActive)
                .Include(p => p.ProductType)
                    .ThenInclude(pt => pt.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Images)
                .ToListAsync();

            // Get reviews for each product
            var reviews = await _reviewRepository.GetAllAsNoTracking()
                .Where(r => r.Product.Workshop != null && r.Product.Workshop.UserId == userId)
                .GroupBy(r => r.ProductId)
                .Select(g => new { ProductId = g.Key, AvgRating = g.Average(r => r.Rating) })
                .ToListAsync();

            // Join products with reviews and sort
            var topProducts = products
                .Join(reviews,
                    p => p.Id,
                    r => r.ProductId,
                    (p, r) => new { Product = p, Rating = r.AvgRating })
                .OrderByDescending(x => x.Rating)
                .Take(topCount)
                .Select(x => x.Product)
                .ToList();

            return topProducts.Adapt<List<ProductDto>>();
        }


    }
}
