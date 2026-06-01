using Graduation_Application.DTOs.ReviewDTO;
using Graduation_Application.IServices;
using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Graduation_Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IGenaricRepositories<Review> _reviewRepository;
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IInternalNotificationService _internalNotificationService;

        public ReviewService(
            IGenaricRepositories<Review> reviewRepository,
            IGenaricRepositories<Product> productRepository,
            IInternalNotificationService internalNotificationService)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _internalNotificationService = internalNotificationService;
        }

        public async Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(createReviewDto.ProductId);
            if (product == null)
                throw new ArgumentException($"Product with ID {createReviewDto.ProductId} not found.");

            // Validate rating is between 1 and 5
            if (createReviewDto.Rating < 1 || createReviewDto.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            // Check if user already reviewed this product (one review per user per product)
            var existingReview = await _reviewRepository.GetAllAsNoTracking()
                .Where(r => r.ProductId == createReviewDto.ProductId && r.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingReview != null)
                throw new InvalidOperationException("You have already reviewed this product.");

            var review = new Review
            {
                UserId = userId,
                ProductId = createReviewDto.ProductId,
                WorkshopId = product.WorkshopId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

            // Send notification to vendor about new review
            var productNotification = await _productRepository.GetByIdAsync(review.ProductId);
            if (productNotification != null && !string.IsNullOrWhiteSpace(product.UserId))
            {
                await _internalNotificationService.CreateAsync(product.UserId, NotificationType.NewReview);
            }

            // Map to DTO
            var reviewDto = review.Adapt<ReviewDto>();

            return reviewDto;
        }

        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _reviewRepository.GetAllAsNoTracking()
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Adapt<List<ReviewDto>>();
        }

        public async Task<AverageRatingDto> GetProductAverageRatingAsync(int productId)
        {
            var reviews = await _reviewRepository.GetAllAsNoTracking()
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if (reviews.Count == 0)
            {
                return new AverageRatingDto
                {
                    ProductId = productId,
                    AverageRating = 0,
                    TotalReviews = 0
                };
            }

            var averageRating = (decimal)reviews.Average(r => r.Rating);

            return new AverageRatingDto
            {
                ProductId = productId,
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = reviews.Count
            };
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string userId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return false;

            // Only review owner can delete
            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews.");

            _reviewRepository.Delete(review);
            await _reviewRepository.SaveChangesAsync();

            return true;
        }

        public async Task<ReviewDetailsDto> GetReviewDetailsAsync(int reviewId)
        {
            var review = await _reviewRepository.GetAllAsNoTracking()
                .Where(r => r.Id == reviewId)
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync();

            if (review == null)
                return null;

            return review.Adapt<ReviewDetailsDto>();
        }

        public async Task<IEnumerable<ReviewDetailsDto>> GetVendorReviewsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required.");

            var reviews = await _reviewRepository.GetAllAsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => r.Product != null && r.Product.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Adapt<List<ReviewDetailsDto>>();
        }

        public async Task<ReviewDetailsDto> ReplyToReviewAsync(int reviewId, string userId, string reply)
        {
            var review = await LoadReviewForVendorActionAsync(reviewId);
            EnsureVendorOwnsReview(review, userId);

            review.VendorReply = reply;
            review.ReplyCreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync();

            return await LoadReviewDetailsAsync(review.Id);
        }

        public async Task<bool> ReportReviewAsync(int reviewId, string userId, string reason)
        {
            var review = await LoadReviewForVendorActionAsync(reviewId);
            EnsureVendorOwnsReview(review, userId);

            review.IsReported = true;
            review.ReportReason = reason;
            review.UpdatedAt = DateTime.UtcNow;

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync();

            return true;
        }

        private async Task<Review> LoadReviewForVendorActionAsync(int reviewId)
        {
            var review = await _reviewRepository.GetAll()
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                throw new ArgumentException($"Review with ID {reviewId} not found.");

            return review;
        }

        private async Task<ReviewDetailsDto> LoadReviewDetailsAsync(int reviewId)
        {
            var updatedReview = await _reviewRepository.GetAllAsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            return updatedReview.Adapt<ReviewDetailsDto>();
        }

        private static void EnsureVendorOwnsReview(Review review, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || review.Product == null || review.Product.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to manage this review.");
        }
    }
}
