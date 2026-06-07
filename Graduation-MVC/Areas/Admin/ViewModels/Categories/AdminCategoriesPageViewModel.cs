using System.Collections.Generic;
using Graduation_Application.DTOs.CategoryDTO;

namespace Graduation_MVC.Areas.Admin.ViewModels.Categories
{
    public class AdminCategoriesPageViewModel
    {
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public string? Search { get; set; }
    }
}
