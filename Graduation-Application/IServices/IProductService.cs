using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.ProductDTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IProductService
    {
        Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDetailsDto> GetProductDetailsAsync(int id);
    }
}
