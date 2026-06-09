using System;

namespace Graduation_Application.DTOs.Admin.AuditLogsDTO
{
    public class AuditLogListItemDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserRoleAr { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionAr { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityTypeAr { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
