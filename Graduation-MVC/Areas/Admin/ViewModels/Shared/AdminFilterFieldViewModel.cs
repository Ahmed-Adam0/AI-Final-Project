namespace Graduation_MVC.Areas.Admin.ViewModels.Shared
{
    public class AdminFilterFieldViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = "text";
        public string? Value { get; set; }
        public string? Placeholder { get; set; }
        public int ColumnClass { get; set; } = 3;
        public IReadOnlyList<AdminFilterOptionViewModel> Options { get; set; } = Array.Empty<AdminFilterOptionViewModel>();
    }

    public class AdminFilterOptionViewModel
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}
