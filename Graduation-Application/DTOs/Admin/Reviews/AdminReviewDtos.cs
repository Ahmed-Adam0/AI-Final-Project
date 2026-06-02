using System;
using System.Collections.Generic;

namespace Graduation_Application.DTOs.Admin.Reviews
{
    public class AdminReviewFilterDto
    {
        public string? Search { get; set; }
        public int? Rating { get; set; }
        public bool? IsReported { get; set; }
        public int? ProductId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminReviewListDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VendorName { get; set; } = "N/A";
        public string UserName { get; set; } = "N/A";
        public int Rating { get; set; }
        public bool IsReported { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminReviewDetailsDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VendorName { get; set; } = "N/A";
        public string VendorEmail { get; set; } = "N/A";
        public string UserName { get; set; } = "N/A";
        public string UserEmail { get; set; } = "N/A";

        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public bool IsReported { get; set; }
        public string? ReportReason { get; set; }
        public DateTime? ReportedAt { get; set; }

        public string? VendorReply { get; set; }
        public DateTime? ReplyCreatedAt { get; set; }

        public List<ReviewModerationLogDto> ModerationHistory { get; set; } = new();
    }

    public class ReportedReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VendorName { get; set; } = "N/A";
        public string UserName { get; set; } = "N/A";
        public string? ReportReason { get; set; }
        public DateTime? ReportedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReviewModerationLogDto
    {
        public DateTime CreatedAt { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? AdminUserId { get; set; }
    }
}

