using Microsoft.AspNetCore.Mvc;
using Graduation_Application.DTOs.ReviewDTO;
using Graduation_Application.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Get all reviews for a specific product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of reviews for the product</returns>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            try
            {
                if (productId <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var reviews = await _reviewService.GetProductReviewsAsync(productId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get average rating for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Average rating and review count</returns>
        [HttpGet("product/{productId}/rating")]
        public async Task<IActionResult> GetProductAverageRating(int productId)
        {
            try
            {
                if (productId <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var rating = await _reviewService.GetProductAverageRatingAsync(productId);
                return Ok(rating);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new review for a product (User only)
        /// </summary>
        /// <param name="createReviewDto">Review creation data</param>
        /// <returns>Created review details</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                if (createReviewDto == null)
                    return BadRequest(new { message = "Review data is required" });

                if (createReviewDto.Rating < 1 || createReviewDto.Rating > 5)
                    return BadRequest(new { message = "Rating must be between 1 and 5" });

                // Extract user ID from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                // Block vendors from creating reviews
                if (User.IsInRole("Vendor"))
                    return StatusCode(403, new { message = "Vendors are not allowed to create reviews." });

                var result = await _reviewService.CreateReviewAsync(userId, createReviewDto);
                return CreatedAtAction(nameof(GetReviewDetails), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a review (Owner or Admin only)
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid review ID" });

                // Extract user ID from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _reviewService.DeleteReviewAsync(id, userId);
                if (!result)
                    return NotFound(new { message = "Review not found" });

                return Ok(new { message = "Review deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get review details by ID
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>Review details with product and user information</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewDetails(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid review ID" });

                var review = await _reviewService.GetReviewDetailsAsync(id);
                if (review == null)
                    return NotFound(new { message = "Review not found" });

                return Ok(review);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all reviews for products owned by the logged-in vendor
        /// </summary>
        [HttpGet("vendor")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetVendorReviews()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var reviews = await _reviewService.GetVendorReviewsAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Vendor reply to a product review
        /// </summary>
        [HttpPost("{reviewId}/reply")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> ReplyToReview(int reviewId, [FromBody] VendorReplyDto replyDto)
        {
            try
            {
                if (reviewId <= 0)
                    return BadRequest(new { message = "Invalid review ID" });

                if (replyDto == null || string.IsNullOrWhiteSpace(replyDto.Reply))
                    return BadRequest(new { message = "Reply content is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _reviewService.ReplyToReviewAsync(reviewId, userId, replyDto.Reply);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Vendor report a product review
        /// </summary>
        [HttpPost("{reviewId}/report")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> ReportReview(int reviewId, [FromBody] ReportReviewDto reportDto)
        {
            try
            {
                if (reviewId <= 0)
                    return BadRequest(new { message = "Invalid review ID" });

                if (reportDto == null || string.IsNullOrWhiteSpace(reportDto.Reason))
                    return BadRequest(new { message = "Report reason is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _reviewService.ReportReviewAsync(reviewId, userId, reportDto.Reason);
                return Ok(new { message = "Review reported successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }
    }
}
