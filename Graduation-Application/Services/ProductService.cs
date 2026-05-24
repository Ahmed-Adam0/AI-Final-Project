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

        public ProductService(IGenaricRepositories<Product> productRepository)
        {
            _productRepository = productRepository;
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
    }
}
