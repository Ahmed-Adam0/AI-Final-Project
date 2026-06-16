using Graduation_Application.DTOs.CategoryDTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface ICategoryService
    {
        // Read operations (existing)
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<List<CategoryTreeDto>> GetCategoryTreeAsync();

        // Write operations (admin)
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateCategoryDto);
        Task<bool> DeleteCategoryAsync(int categoryId);
    }
}
