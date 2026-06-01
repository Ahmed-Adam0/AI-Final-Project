namespace Graduation_MVC.Areas.Admin.ViewModels.Shared
{
    public class AdminPaginationViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string? PageNumberParamName { get; set; } = "PageNumber";
        public string? Area { get; set; } = "Admin";
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = "Index";
        public object? RouteValues { get; set; }
        public string? AriaLabel { get; set; } = "Pagination";
    }
}
