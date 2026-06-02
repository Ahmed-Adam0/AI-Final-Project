using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.Reviews;
using Graduation_Application.DTOs.Common;

namespace Graduation_Application.IServices.Admin
{
    public interface IAdminReviewService
    {
        Task<PaginatedResult<AdminReviewListDto>> GetReviewsAsync(AdminReviewFilterDto filter);
        Task<AdminReviewDetailsDto?> GetReviewDetailsAsync(int id);

        Task<List<ReportedReviewDto>> GetReportedReviewsAsync();

        Task<bool> ResolveReportAsync(int reviewId, string? adminUserId = null);
        Task<bool> IgnoreReportAsync(int reviewId, string? adminUserId = null);

        Task<bool> DeleteReviewAsync(int reviewId, string? adminUserId = null);
        Task<List<ReviewModerationLogDto>> GetModerationHistoryAsync(int reviewId);
    }
}

