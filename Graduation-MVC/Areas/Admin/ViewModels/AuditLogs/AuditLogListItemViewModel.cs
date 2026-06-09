using System;

namespace Graduation_MVC.Areas.Admin.ViewModels.AuditLogs
{
    public class AuditLogListItemViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
