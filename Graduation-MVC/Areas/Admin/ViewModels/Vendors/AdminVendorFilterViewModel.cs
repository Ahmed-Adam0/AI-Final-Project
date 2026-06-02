namespace Graduation_MVC.Areas.Admin.ViewModels.Vendors
{
    public class AdminVendorFilterViewModel
    {
        public string? Search { get; set; }
        public string? VerificationStatus { get; set; }
        public string? AccountStatus { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

