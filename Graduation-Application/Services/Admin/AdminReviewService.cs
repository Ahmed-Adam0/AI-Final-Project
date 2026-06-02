using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.Reviews;
using Graduation_Application.DTOs.Common;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services.Admin
{
    public class AdminReviewService : IAdminReviewService
    {
        private readonly IGenaricRepositories<Review> _reviewRepository;
        private readonly IGenaricRepositories<ReviewModerationLog> _moderationLogRepository;

        public AdminReviewService(
            IGenaricRepositories<Review> reviewRepository,
            IGenaricRepositories<ReviewModerationLog> moderationLogRepository)
        {
            _reviewRepository = reviewRepository;
            _moderationLogRepository = moderationLogRepository;
        }

        public async Task<PaginatedResult<AdminReviewListDto>> GetReviewsAsync(AdminReviewFilterDto filter)
        {
            int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            IQueryable<Review> query = _reviewRepository
                .GetAllAsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .ThenInclude(p => p.User)
                .Where(r => r.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(r =>
                    (r.Comment != null && r.Comment.ToLower().Contains(s)) ||
                    (r.User != null && r.User.FullName != null && r.User.FullName.ToLower().Contains(s)) ||
                    (r.Product != null && (
                        (r.Product.NameEn != null && r.Product.NameEn.ToLower().Contains(s)) ||
                        (r.Product.NameAr != null && r.Product.NameAr.ToLower().Contains(s))
                    ))
                );
            }

            if (filter.Rating.HasValue && filter.Rating.Value is >= 1 and <= 5)
            {
                query = query.Where(r => r.Rating == filter.Rating.Value);
            }

            if (filter.IsReported.HasValue)
            {
                query = query.Where(r => r.IsReported == filter.IsReported.Value);
            }

            if (filter.ProductId.HasValue && filter.ProductId.Value > 0)
            {
                query = query.Where(r => r.ProductId == filter.ProductId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= filter.ToDate.Value);
            }

            var totalCount = await query.CountAsync();

            var page = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var list = page.Select(r => new AdminReviewListDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.NameEn ?? r.Product?.NameAr ?? $"Product #{r.ProductId}",
                VendorName = r.Product?.User?.FullName ?? "N/A",
                UserName = r.User?.FullName ?? "N/A",
                Rating = r.Rating,
                IsReported = r.IsReported,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new PaginatedResult<AdminReviewListDto>(list, totalCount, pageNumber, pageSize);
        }

        public async Task<AdminReviewDetailsDto?> GetReviewDetailsAsync(int id)
        {
            var review = await _reviewRepository
                .GetAllAsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return null;

            var history = await GetModerationHistoryAsync(id);

            return new AdminReviewDetailsDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductName = review.Product?.NameEn ?? review.Product?.NameAr ?? $"Product #{review.ProductId}",
                VendorName = review.Product?.User?.FullName ?? "N/A",
                VendorEmail = review.Product?.User?.Email ?? "N/A",
                UserName = review.User?.FullName ?? "N/A",
                UserEmail = review.User?.Email ?? "N/A",
                Rating = review.Rating,
                Comment = review.Comment ?? string.Empty,
                CreatedAt = review.CreatedAt,
                IsActive = review.IsActive,
                IsReported = review.IsReported,
                ReportReason = review.ReportReason,
                ReportedAt = review.IsReported ? (review.UpdatedAt ?? review.CreatedAt) : null,
                VendorReply = review.VendorReply,
                ReplyCreatedAt = review.ReplyCreatedAt,
                ModerationHistory = history
            };
        }

        public async Task<List<ReportedReviewDto>> GetReportedReviewsAsync()
        {
            var reviews = await _reviewRepository
                .GetAllAsNoTracking()
                .Where(r => r.IsReported)
                .Include(r => r.User)
                .Include(r => r.Product)
                .ThenInclude(p => p.User)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => new ReportedReviewDto
            {
                ReviewId = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.NameEn ?? r.Product?.NameAr ?? $"Product #{r.ProductId}",
                VendorName = r.Product?.User?.FullName ?? "N/A",
                UserName = r.User?.FullName ?? "N/A",
                ReportReason = r.ReportReason,
                ReportedAt = r.UpdatedAt ?? r.CreatedAt,
                IsActive = r.IsActive
            }).ToList();
        }

        public async Task<bool> ResolveReportAsync(int reviewId, string? adminUserId = null)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.IsReported = false;
            review.ReportReason = null;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = adminUserId;

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync();

            await AddLogAsync(reviewId, "ResolveReport", "Report resolved.", adminUserId);
            return true;
        }

        public async Task<bool> IgnoreReportAsync(int reviewId, string? adminUserId = null)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.IsReported = false;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = adminUserId;

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync();

            await AddLogAsync(reviewId, "IgnoreReport", "Report ignored.", adminUserId);
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string? adminUserId = null)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.IsActive = false;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = adminUserId;

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync();

            await AddLogAsync(reviewId, "DeleteReview", "Review deactivated by admin.", adminUserId);
            return true;
        }

        public async Task<List<ReviewModerationLogDto>> GetModerationHistoryAsync(int reviewId)
        {
            try
            {
                var logs = await _moderationLogRepository
                    .GetAllAsNoTracking()
                    .Where(l => l.ReviewId == reviewId)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();

                return logs.Select(l => new ReviewModerationLogDto
                {
                    CreatedAt = l.CreatedAt,
                    Action = l.Action,
                    Notes = l.Notes,
                    AdminUserId = l.AdminUserId
                }).ToList();
            }
            catch
            {
                // Safe fallback when table is unavailable or query fails.
                return new List<ReviewModerationLogDto>();
            }
        }

        private async Task AddLogAsync(int reviewId, string action, string? notes, string? adminUserId)
        {
            var log = new ReviewModerationLog
            {
                ReviewId = reviewId,
                Action = action,
                Notes = notes,
                AdminUserId = adminUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                await _moderationLogRepository.AddAsync(log);
                await _moderationLogRepository.SaveChangesAsync();
            }
            catch
            {
                // Logging should not break moderation flow.
            }
        }
    }
}

