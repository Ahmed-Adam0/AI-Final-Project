namespace Graduation_MVC.Areas.Admin.ViewModels.Users
{
    public class AdminUsersFilterViewModel
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? EmailConfirmed { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
