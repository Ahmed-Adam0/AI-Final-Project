using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_MVC.Areas.Admin.ViewModels.Settings
{
    public class AdminSettingsViewModel
    {
        public PlatformSettingsViewModel Platform { get; set; } = new();
        public CommissionSettingsViewModel Commission { get; set; } = new();
        public SupportSettingsViewModel Support { get; set; } = new();
        public IReadOnlyList<AdminAuditLogItemViewModel> AuditLogs { get; set; } = Array.Empty<AdminAuditLogItemViewModel>();
    }

    public class PlatformSettingsViewModel
    {
        [Required]
        [StringLength(150)]
        public string PlatformName { get; set; }

        [StringLength(250)]
        public string LogoUrl { get; set; }

        [StringLength(250)]
        public string ContactInformation { get; set; }

        public bool MaintenanceMode { get; set; }
    }

    public class CommissionSettingsViewModel
    {
        [Range(0, 100)]
        public decimal CommissionPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VendorFees { get; set; }

        [Range(0, 100)]
        public decimal TaxPercentage { get; set; }
    }

    public class SupportSettingsViewModel
    {
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(50)]
        public string WhatsApp { get; set; }

        [StringLength(200)]
        public string SupportHours { get; set; }
    }

    public class AdminAuditLogItemViewModel
    {
        public DateTime CreatedAt { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public string Details { get; set; }
    }
}
