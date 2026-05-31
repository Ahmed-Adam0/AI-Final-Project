using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.ProductDTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IProductService
    {
        // Read operations (existing)
        Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDetailsDto> GetProductDetailsAsync(int id);

        // Write operations (vendor — ownership via UserId)
        Task<ProductResponseDto> CreateProductAsync(string userId, CreateProductDto createProductDto);
        Task<ProductResponseDto> UpdateProductAsync(int productId, string userId, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int productId, string userId);

        // Product Image System (ownership via UserId)
        Task<ProductImageDto> AddProductImageAsync(int productId, string userId, string imageUrl, bool isPrimary);
        Task<bool> RemoveProductImageAsync(int productId, string userId, int imageId);
        Task<ProductImageDto> ReplaceProductImageAsync(int productId, string userId, int imageId, string newImageUrl);
        Task<bool> SetPrimaryImageAsync(int productId, string userId, int imageId);
        Task<ProductResponseDto> SetProductStatusAsync(int productId, string userId, bool isActive);
    }
}
