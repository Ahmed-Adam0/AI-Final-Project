using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<Category> _categoryRepository;
        private readonly IGenaricRepositories<ProductImage> _productImageRepository;

        public ProductService(
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<Category> categoryRepository,
            IGenaricRepositories<ProductImage> productImageRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            // Validate pagination parameters
            int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            // Start with base query (AsNoTracking for performance)
            var query = _productRepository.GetAllAsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Images)
                .Where(p => p.IsActive);

            // Apply Search Filter (Name and Description)
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

            // Apply Category Filter
            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            // Apply Workshop Filter
            if (filter.WorkshopId.HasValue && filter.WorkshopId > 0)
            {
                query = query.Where(p => p.WorkshopId == filter.WorkshopId.Value);
            }

            // Apply Price Range Filter
            if (filter.MinPrice.HasValue && filter.MinPrice > 0)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue && filter.MaxPrice > 0)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // Apply Material Filter (search in description if not a separate field)
            if (!string.IsNullOrWhiteSpace(filter.Material))
            {
                string materialTerm = filter.Material.ToLower();
                query = query.Where(p =>
                    p.DescriptionAr.ToLower().Contains(materialTerm) ||
                    p.DescriptionEn.ToLower().Contains(materialTerm)
                );
            }

            // Get total count before pagination
            int totalCount = await query.CountAsync();

            // Apply Pagination (AFTER all filters, BEFORE execution)
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
            var productDtos = products.Adapt<List<ProductDto>>();

            // Return paginated result
            return new PaginatedResult<ProductDto>(
                productDtos,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<ProductDetailsDto> GetProductDetailsAsync(int id)
        {
            var product = await _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Workshop)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return null;

            return product.Adapt<ProductDetailsDto>();
        }

        public async Task<ProductResponseDto> CreateProductAsync(int workshopId, CreateProductDto createProductDto)
        {
            // Validate that product details are provided
            if (createProductDto == null)
                throw new ArgumentNullException(nameof(createProductDto));

            // Validate Category exists
            var categoryExists = await _categoryRepository.AnyAsync(c => c.Id == createProductDto.CategoryId);
            if (!categoryExists)
                throw new ArgumentException($"Category with ID {createProductDto.CategoryId} does not exist.");

            // Create new product entity
            var product = new Product
            {
                WorkshopId = workshopId,
                CategoryId = createProductDto.CategoryId,
                NameAr = createProductDto.NameAr,
                NameEn = createProductDto.NameEn,
                DescriptionAr = createProductDto.DescriptionAr,
                DescriptionEn = createProductDto.DescriptionEn,
                Price = createProductDto.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return product.Adapt<ProductResponseDto>();
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int productId, int workshopId, UpdateProductDto updateProductDto)
        {
            // Get the product
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            // Verify workshop ownership
            if (product.WorkshopId != workshopId)
                throw new UnauthorizedAccessException("You do not have permission to update this product.");

            // Validate Category exists
            var categoryExists = await _categoryRepository.AnyAsync(c => c.Id == updateProductDto.CategoryId);
            if (!categoryExists)
                throw new ArgumentException($"Category with ID {updateProductDto.CategoryId} does not exist.");

            // Update product properties
            product.CategoryId = updateProductDto.CategoryId;
            product.NameAr = updateProductDto.NameAr;
            product.NameEn = updateProductDto.NameEn;
            product.DescriptionAr = updateProductDto.DescriptionAr;
            product.DescriptionEn = updateProductDto.DescriptionEn;
            product.Price = updateProductDto.Price;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return product.Adapt<ProductResponseDto>();
        }

        public async Task<bool> DeleteProductAsync(int productId, int workshopId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            // Verify workshop ownership
            if (product.WorkshopId != workshopId)
                throw new UnauthorizedAccessException("You do not have permission to delete this product.");

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();

            return true;
        }

        public async Task<ProductImageDto> AddProductImageAsync(int productId, int workshopId, string imageUrl, bool isPrimary)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found.");

            if (product.WorkshopId != workshopId)
                throw new UnauthorizedAccessException("You do not have permission to manage this product.");

            // Check if this is the first image for this product
            var hasImages = await _productImageRepository.AnyAsync(pi => pi.ProductId == productId);
            if (!hasImages)
            {
                isPrimary = true;
            }
            else if (isPrimary)
            {
                // Set all other images to false
                var existingImages = await _productImageRepository.Where(pi => pi.ProductId == productId).ToListAsync();
                foreach (var img in existingImages)
                {
                    if (img.IsPrimary)
                    {
                        img.IsPrimary = false;
                        _productImageRepository.Update(img);
                    }
                }
            }

            var newImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary,
                CreatedAt = DateTime.UtcNow
            };

            await _productImageRepository.AddAsync(newImage);
            await _productImageRepository.SaveChangesAsync();

            return newImage.Adapt<ProductImageDto>();
        }

        public async Task<bool> RemoveProductImageAsync(int productId, int workshopId, int imageId)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
                return false;

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || product.WorkshopId != workshopId)
                throw new UnauthorizedAccessException("You do not have permission to manage this product.");

            bool wasPrimary = image.IsPrimary;

            _productImageRepository.Delete(image);
            await _productImageRepository.SaveChangesAsync();

            // If we deleted the primary image, set another remaining image as primary (if any)
            if (wasPrimary)
            {
                var remainingImages = await _productImageRepository.Where(pi => pi.ProductId == productId).ToListAsync();
                var firstRemaining = remainingImages.FirstOrDefault();
                if (firstRemaining != null)
                {
                    firstRemaining.IsPrimary = true;
                    firstRemaining.UpdatedAt = DateTime.UtcNow;
                    _productImageRepository.Update(firstRemaining);
                    await _productImageRepository.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<ProductImageDto> ReplaceProductImageAsync(int productId, int workshopId, int imageId, string newImageUrl)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
                throw new ArgumentException($"Image with ID {imageId} not found for this product.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || product.WorkshopId != workshopId)
                throw new UnauthorizedAccessException("You do not have permission to manage this product.");

            image.ImageUrl = newImageUrl;
            image.UpdatedAt = DateTime.UtcNow;

            _productImageRepository.Update(image);
            await _productImageRepository.SaveChangesAsync();

            return image.Adapt<ProductImageDto>();
        }

        public async Task<bool> SetPrimaryImageAsync(int productId, int workshopId, int imageId)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
                return false;

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || product.WorkshopId != workshopId)
                throw new UnauthorizedAccessException("You do not have permission to manage this product.");

            var otherImages = await _productImageRepository.Where(pi => pi.ProductId == productId && pi.Id != imageId).ToListAsync();
            foreach (var img in otherImages)
            {
                if (img.IsPrimary)
                {
                    img.IsPrimary = false;
                    _productImageRepository.Update(img);
                }
            }

            image.IsPrimary = true;
            image.UpdatedAt = DateTime.UtcNow;

            _productImageRepository.Update(image);
            await _productImageRepository.SaveChangesAsync();

            return true;
        }
    }
}
