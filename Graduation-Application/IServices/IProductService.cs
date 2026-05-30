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

        // Write operations (vendor)
        Task<ProductResponseDto> CreateProductAsync(int workshopId, CreateProductDto createProductDto);
        Task<ProductResponseDto> UpdateProductAsync(int productId, int workshopId, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int productId, int workshopId);

        // Product Image System
        Task<ProductImageDto> AddProductImageAsync(int productId, int workshopId, string imageUrl, bool isPrimary);
        Task<bool> RemoveProductImageAsync(int productId, int workshopId, int imageId);
        Task<ProductImageDto> ReplaceProductImageAsync(int productId, int workshopId, int imageId, string newImageUrl);
        Task<bool> SetPrimaryImageAsync(int productId, int workshopId, int imageId);
    }
}
