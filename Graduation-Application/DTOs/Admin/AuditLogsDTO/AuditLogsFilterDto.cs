using System;

namespace Graduation_Application.DTOs.Admin.AuditLogsDTO
{
    public class AuditLogsFilterDto
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
