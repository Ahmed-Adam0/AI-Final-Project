using System;
using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.Reports
{
    public class AdminReportsPageViewModel
    {
        public AdminReportsFilterViewModel Filter { get; set; } = new();
        public IReadOnlyList<AdminReportListItemViewModel> Reports { get; set; } = Array.Empty<AdminReportListItemViewModel>();
    }

    public class AdminReportsFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string VendorId { get; set; }
        public string CategoryId { get; set; }
        public string ReportType { get; set; }
    }

    public class AdminReportListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string VendorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DownloadUrl { get; set; }
        public string ExportPdfUrl { get; set; }
        public string ExportExcelUrl { get; set; }
    }
}
