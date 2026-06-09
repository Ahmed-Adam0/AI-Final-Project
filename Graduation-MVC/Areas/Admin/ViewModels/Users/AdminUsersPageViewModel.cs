using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.Users
{
    public class AdminUsersPageViewModel
    {
        public AdminUsersFilterViewModel Filter { get; set; }
        public List<AdminUserListItemViewModel> Users { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
