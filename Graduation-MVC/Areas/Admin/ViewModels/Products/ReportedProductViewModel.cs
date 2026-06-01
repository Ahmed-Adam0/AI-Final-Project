using System;

namespace Graduation_MVC.Areas.Admin.ViewModels.Products
{
    /// <summary>
    /// ViewModel for displaying a single reported product in MVC views.
    /// Maps from ReportedProductDto to ensure DTO is not used directly in views.
    /// </summary>
    public class ReportedProductViewModel
    {
        public int ReportId { get; set; }
        public int ProductId { get; set; }
        public string ProductNameEn { get; set; } = string.Empty;
        public string ProductNameAr { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ReportedByName { get; set; } = string.Empty;
        public string ReportedByEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
