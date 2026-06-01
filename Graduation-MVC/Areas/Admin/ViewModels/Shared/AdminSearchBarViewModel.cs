namespace Graduation_MVC.Areas.Admin.ViewModels.Shared
{
    public class AdminSearchBarViewModel
    {
        public string Name { get; set; } = "Search";
        public string? Value { get; set; }
        public string Placeholder { get; set; } = "Search...";
        public string Label { get; set; } = "Search";
        public string IconClass { get; set; } = "fa-solid fa-magnifying-glass";
        public int ColumnClass { get; set; } = 4;
    }
}
