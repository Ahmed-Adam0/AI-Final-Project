namespace Graduation_Application.DTOs.VendorDTO
{
    public class VendorAuthResponseDto
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; } = "";
        public string WorkshopNameEn { get; set; } = "";
        public bool IsVerified { get; set; }
        public string? LogoUrl { get; set; }
    }
}
