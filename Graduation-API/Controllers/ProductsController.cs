using Microsoft.AspNetCore.Mvc;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IServices;
using System;
using System.Threading.Tasks;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get products with search, filters, and pagination
        /// </summary>
        /// <param name="search">Search by product name or description</param>
        /// <param name="categoryId">Filter by category ID</param>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="material">Filter by material (searches in description)</param>
        /// <param name="workshopId">Filter by workshop ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of products</returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string material = null,
            [FromQuery] int? workshopId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new ProductFilterDto
                {
                    Search = search,
                    CategoryId = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Material = material,
                    WorkshopId = workshopId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _productService.GetProductsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get product details by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details with images and workshop information</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var product = await _productService.GetProductDetailsAsync(id);

                if (product == null)
                    return NotFound(new { message = "Product not found" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
