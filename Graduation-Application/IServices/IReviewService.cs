using Graduation_Application.DTOs.ReviewDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto);
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId);
        Task<AverageRatingDto> GetProductAverageRatingAsync(int productId);
        Task<bool> DeleteReviewAsync(int reviewId, string userId);
        Task<ReviewDetailsDto> GetReviewDetailsAsync(int reviewId);
        Task<IEnumerable<ReviewDetailsDto>> GetVendorReviewsAsync(int workshopId);
        Task<ReviewDetailsDto> ReplyToReviewAsync(int reviewId, int workshopId, string reply);
        Task<bool> ReportReviewAsync(int reviewId, int workshopId, string reason);
    }
}
