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
        Task<bool> HideProductAsync(int id);
        Task<bool> UnhideProductAsync(int id);

        // Report management
        Task<List<ReportedProductDto>> GetReportedProductsAsync();
        Task<bool> ResolveReportAsync(int reportId);

        // Dropdown data for filters
        Task<List<CategoryDropdownDto>> GetAllCategoriesAsync();
        Task<List<VendorDropdownDto>> GetAllVendorsAsync();
    }
}
