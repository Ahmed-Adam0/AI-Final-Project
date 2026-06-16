using Graduation_Application.DTOs.CategoryDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IProductTypeService
    {
        Task<List<ProductTypeDto>> GetAllProductTypesAsync();
        Task<List<ProductTypeDto>> GetProductTypesBySubCategoryIdAsync(int subCategoryId);
        Task<ProductTypeDto> GetProductTypeByIdAsync(int id);
        Task<ProductTypeDto> CreateProductTypeAsync(CreateProductTypeDto createProductTypeDto);
        Task<ProductTypeDto> UpdateProductTypeAsync(int id, UpdateProductTypeDto updateProductTypeDto);
        Task<bool> DeleteProductTypeAsync(int id);
    }
}
