using System;

namespace Graduation_Application.DTOs.OrderDTO
{
    public class OrderStatusHistoryResponseDto
    {
        public int Id { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}