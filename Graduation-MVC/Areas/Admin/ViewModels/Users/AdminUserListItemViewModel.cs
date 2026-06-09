using System;

namespace Graduation_MVC.Areas.Admin.ViewModels.Users
{
    public class AdminUserListItemViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
    }
}
