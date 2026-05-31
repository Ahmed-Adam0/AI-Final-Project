using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.DTOs.ProductDTO
{
    public class ProductDetailsDto
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
        public string WorkshopDescriptionAr { get; set; }
        public string WorkshopDescriptionEn { get; set; }
        public string WorkshopAddress { get; set; }
        public string WorkshopLogoUrl { get; set; }
        public decimal? WorkshopRating { get; set; }
        public bool WorkshopIsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<ProductImageDto> Images { get; set; }

        public ProductDetailsDto()
        {
            Images = new List<ProductImageDto>();
        }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }
}
