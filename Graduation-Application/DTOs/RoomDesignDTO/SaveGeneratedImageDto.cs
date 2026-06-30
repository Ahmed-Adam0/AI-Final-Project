using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.RoomDesignDTO
{
    public class SaveGeneratedImageDto
    {
        [Required]
        public string EmptyRoom { get; set; }

        [Required]
        public string GenerateImage { get; set; }

        public int? OrderID { get; set; }

        [Required]
        public decimal Length { get; set; }

        [Required]
        public decimal Width { get; set; }

        [Required]
        public decimal Height { get; set; }
    }
}
