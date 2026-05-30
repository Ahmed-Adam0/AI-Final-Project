using System;

namespace Graduation_Application.DTOs.ReviewDTO
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? VendorReply { get; set; }
        public DateTime? ReplyCreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class ReviewDetailsDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? VendorReply { get; set; }
        public DateTime? ReplyCreatedAt { get; set; }
    }

    public class AverageRatingDto
    {
        public int ProductId { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class VendorReplyDto
    {
        public string Reply { get; set; }
    }

    public class ReportReviewDto
    {
        public string Reason { get; set; }
    }
}
