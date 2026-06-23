namespace Graduation_Application.DTOs.Admin.Inspirations
{
    public class AdminInspirationFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsApproved { get; set; }
        public string? Search { get; set; }
    }
}
