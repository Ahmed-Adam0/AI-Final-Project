using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.Admin.AdminDashboardDTO
{
    public class AdminReportsFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string VendorId { get; set; }
        public string CategoryId { get; set; }
        public string ReportType { get; set; }
    }

    public class AdminReportsPageDto
    {
        public AdminReportsFilterDto Filter { get; set; } = new();
        public List<AdminReportListItemDto> Reports { get; set; } = new();
    }
}
