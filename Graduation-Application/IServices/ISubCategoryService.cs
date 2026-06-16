using Graduation_Application.DTOs.CategoryDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface ISubCategoryService
    {
        Task<List<SubCategoryDto>> GetAllSubCategoriesAsync();
        Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(int categoryId);
        Task<SubCategoryDto> GetSubCategoryByIdAsync(int id);
        Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createSubCategoryDto);
        Task<SubCategoryDto> UpdateSubCategoryAsync(int id, UpdateSubCategoryDto updateSubCategoryDto);
        Task<bool> DeleteSubCategoryAsync(int id);
    }
}
