using System.Collections.Generic;
using Graduation_Application.DTOs.Admin.AdminProductDTO;
using Graduation_Application.DTOs.Common;
using Graduation_Domain.Enums;

namespace Graduation_MVC.Areas.Admin.ViewModels.Products
{
    /// <summary>
    /// ViewModel for the Admin Products Index page.
    /// Contains filter dropdowns, current filter state, and paginated product list.
    /// </summary>
    public class AdminProductsPageViewModel
    {
        // Current filter values (for form persistence)
        public string Search { get; set; }
        public int? CategoryId { get; set; }
        public string VendorId { get; set; }
        public ProductStatus? Status { get; set; }

        // Paginated product results
        public PaginatedResult<AdminProductListDto> Products { get; set; }

        // Dropdown data
        public List<CategoryDropdownDto> Categories { get; set; } = new List<CategoryDropdownDto>();
        public List<VendorDropdownDto> Vendors { get; set; } = new List<VendorDropdownDto>();
    }
}
