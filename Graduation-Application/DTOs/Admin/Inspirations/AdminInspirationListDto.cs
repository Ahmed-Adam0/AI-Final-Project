using System;

namespace Graduation_Application.DTOs.Admin.Inspirations
{
    public class AdminInspirationListDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string BeforeImageUrl { get; set; }
        public string AfterImageUrl { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
