using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.ProductDTO;

namespace Graduation_Application.IServices
{
    public interface IVendorProductService
    {
        /// <summary>
        /// Get paginated list of products owned by the vendor
        /// </summary>
        Task<PaginatedResult<ProductDto>> GetVendorProductsAsync(string userId, ProductFilterDto filter);

        /// <summary>
        /// Get detailed information about a specific vendor product
        /// </summary>
        Task<ProductDetailsDto> GetVendorProductDetailsAsync(string userId, int productId);

        /// <summary>
        /// Update product status (IsActive) - vendor only
        /// </summary>
        Task<ProductResponseDto> UpdateVendorProductStatusAsync(string userId, int productId, bool isActive);

        /// <summary>
        /// Delete a product owned by vendor
        /// </summary>
        Task<bool> DeleteVendorProductAsync(string userId, int productId);

        /// <summary>
        /// Get product statistics for vendor dashboard (count, avg rating, etc.)
        /// </summary>
        Task<VendorProductStatsDto> GetVendorProductStatsAsync(string userId);

        /// <summary>
        /// Get top-rated products owned by vendor
        /// </summary>
        Task<IEnumerable<ProductDto>> GetVendorTopProductsAsync(string userId, int topCount = 5);
    }
}
