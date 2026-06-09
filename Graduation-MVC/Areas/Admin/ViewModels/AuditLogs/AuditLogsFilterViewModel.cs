using System;

namespace Graduation_MVC.Areas.Admin.ViewModels.AuditLogs
{
    public class AuditLogsFilterViewModel
    {
        public string? Search { get; set; }
        public string? UserRole { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
