using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Application.DTOs.InspirationDTOs
{
    public class UploadInspirationsDto
    {
        [Required(ErrorMessage = "OrderId is required")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "BeforeImages list is required")]
        public List<IFormFile> BeforeImages { get; set; }

        [Required(ErrorMessage = "AfterImages list is required")]
        public List<IFormFile> AfterImages { get; set; }
    }
}
