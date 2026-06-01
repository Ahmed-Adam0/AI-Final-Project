namespace Graduation_MVC.Areas.Admin.ViewModels.Shared
{
    public class AdminFilterPanelViewModel
    {
        public string Title { get; set; } = "Filters";
        public string IconClass { get; set; } = "fa-solid fa-filter";
        public string Area { get; set; } = "Admin";
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = "Index";
        public string Method { get; set; } = "get";
        public IReadOnlyList<AdminFilterFieldViewModel> Fields { get; set; } = Array.Empty<AdminFilterFieldViewModel>();
        public IReadOnlyDictionary<string, string> HiddenFields { get; set; } = new Dictionary<string, string>();
        public bool ShowReset { get; set; } = true;
        public string ResetAction { get; set; } = "Index";
    }
}
