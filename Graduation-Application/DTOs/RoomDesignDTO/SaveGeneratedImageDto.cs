using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.RoomDesignDTO
{
    public class SaveGeneratedImageDto
    {
        [Required]
        public IFormFile EmptyRoom { get; set; }
        
        [Required]
        public decimal Length { get; set; }

        [Required]
        public decimal Width { get; set; }

        [Required]
        public decimal Height { get; set; }
    }
}
