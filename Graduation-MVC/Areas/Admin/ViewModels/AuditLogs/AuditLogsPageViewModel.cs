using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.AuditLogs
{
    public class AuditLogsPageViewModel
    {
        public AuditLogsFilterViewModel Filter { get; set; } = new AuditLogsFilterViewModel();
        public List<AuditLogListItemViewModel> Logs { get; set; } = new List<AuditLogListItemViewModel>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<string> RoleOptions { get; set; } = new List<string>();
        public List<string> ActionOptions { get; set; } = new List<string>();
        public Dictionary<string, List<string>> AllowedActionsByRole { get; set; } =
            new Dictionary<string, List<string>>();
    }
}
