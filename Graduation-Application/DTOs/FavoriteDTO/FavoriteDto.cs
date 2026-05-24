using System;

namespace Graduation_Application.DTOs.FavoriteDTO
{
    public class FavoriteDto
    {
        public int ProductId { get; set; }
        public string ProductNameAr { get; set; }
        public string ProductNameEn { get; set; }
        public decimal Price { get; set; }
        public string MainImageUrl { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
