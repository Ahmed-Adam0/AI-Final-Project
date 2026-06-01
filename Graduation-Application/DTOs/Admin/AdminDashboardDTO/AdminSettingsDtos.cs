using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.Admin.AdminDashboardDTO
{
    public class AdminSettingsDto
    {
        public PlatformSettingsDto Platform { get; set; } = new();
        public CommissionSettingsDto Commission { get; set; } = new();
        public SupportSettingsDto Support { get; set; } = new();
        public List<AdminAuditLogDto> AuditLogs { get; set; } = new();
    }

    public class PlatformSettingsDto
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

    public class CommissionSettingsDto
    {
        [Range(0, 100)]
        public decimal CommissionPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VendorFees { get; set; }

        [Range(0, 100)]
        public decimal TaxPercentage { get; set; }
    }

    public class SupportSettingsDto
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string WhatsApp { get; set; }
        public string SupportHours { get; set; }
    }

    public class AdminAuditLogDto
    {
        public DateTime CreatedAt { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public string Details { get; set; }
    }
}
