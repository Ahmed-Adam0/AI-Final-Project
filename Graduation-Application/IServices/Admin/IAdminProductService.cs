using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AdminProductDTO;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminProductService
    {
        // Product listing with filters and pagination
        Task<PaginatedResult<AdminProductListDto>> GetProductsAsync(AdminProductFilterDto filter);

        // Product details
        Task<AdminProductDetailsDto> GetProductDetailsAsync(int id);

        // Product moderation actions
        Task<bool> ActivateProductAsync(int id);
        Task<bool> DeactivateProductAsync(int id);
        Task<bool> HideProductAsync(int id);
        Task<bool> RestoreProductAsync(int id);

        // Report management
        Task<List<ReportedProductDto>> GetReportedProductsAsync();
        Task<bool> ResolveReportAsync(int reportId);

        // Dropdown data for filters
        Task<List<CategoryDropdownDto>> GetAllCategoriesAsync();
        Task<List<VendorDropdownDto>> GetAllVendorsAsync();
    }
}
