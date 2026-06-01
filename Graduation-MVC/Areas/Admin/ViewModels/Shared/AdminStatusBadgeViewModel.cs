namespace Graduation_MVC.Areas.Admin.ViewModels.Shared
{
    public class AdminStatusBadgeViewModel
    {
        public string Status { get; set; } = string.Empty;
        public string? DisplayText { get; set; }
        public string? IconClass { get; set; }
        public string CssClass { get; set; } = "badge-status-active";
    }
}
