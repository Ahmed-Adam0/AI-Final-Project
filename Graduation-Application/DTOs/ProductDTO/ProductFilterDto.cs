using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.DTOs.ProductDTO
{
    public class ProductFilterDto
    {
        public string Search { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? ProductTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Material { get; set; }
        public int? WorkshopId { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
