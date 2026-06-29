using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.RoomDesignDTO
{
    public class RoomDesignRequestDto
    {
        [Required]
        public IFormFile RoomImage { get; set; }

        public List<int> ProductIds { get; set; } = new List<int>();
    }
}
