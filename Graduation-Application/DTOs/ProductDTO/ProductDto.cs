using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.DTOs.ProductDTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; }
        public string CategoryNameEn { get; set; }
        public int WorkshopId { get; set; }
        public string WorkshopNameAr { get; set; }
        public string WorkshopNameEn { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string MainImageUrl { get; set; }
    }
}
