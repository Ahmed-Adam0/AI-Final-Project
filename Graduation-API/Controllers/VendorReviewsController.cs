using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Graduation_Application.DTOs.ReviewDTO;
using Graduation_Application.IServices;
using System.IdentityModel.Tokens.Jwt;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/vendor/reviews")]
    [Authorize(Roles = "Vendor")]
    public class VendorReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public VendorReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Get all reviews for products owned by logged-in vendor
        /// </summary>
        [HttpGet]
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
        /// Reply to a review of vendor's product
        /// </summary>
        [HttpPost("{reviewId}/reply")]
        public async Task<IActionResult> ReplyToReview(int reviewId, [FromBody] VendorReplyDto dto)
        {
            try
            {
                if (reviewId <= 0)
                    return BadRequest(new { message = "Invalid review ID" });

                if (dto == null || string.IsNullOrWhiteSpace(dto.Reply))
                    return BadRequest(new { message = "Reply content is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _reviewService.ReplyToReviewAsync(reviewId, userId, dto.Reply);
                if (result == null)
                    return NotFound(new { message = "Review not found" });

                return Ok(new { message = "Reply posted successfully", review = result });
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
        /// Report a review for vendor's product
        /// </summary>
        [HttpPost("{reviewId}/report")]
        public async Task<IActionResult> ReportReview(int reviewId, [FromBody] ReportReviewDto dto)
        {
            try
            {
                if (reviewId <= 0)
                    return BadRequest(new { message = "Invalid review ID" });

                if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                    return BadRequest(new { message = "Report reason is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _reviewService.ReportReviewAsync(reviewId, userId, dto.Reason);
                if (!result)
                    return NotFound(new { message = "Review not found" });

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

        /// <summary>
        /// Helper method to extract user ID from JWT token
        /// </summary>
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }
    }
}
