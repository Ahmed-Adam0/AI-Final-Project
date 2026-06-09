using System;

namespace Graduation_Application.DTOs.Admin.UsersDTO
{
    public class AdminUserDetailsDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string? ProfileImage { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
