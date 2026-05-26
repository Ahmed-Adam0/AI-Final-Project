using Microsoft.AspNetCore.Mvc;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IServices;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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

        /// <summary>
        /// Create a new product (Vendor/Workshop only)
        /// </summary>
        /// <param name="createProductDto">Product creation data</param>
        /// <returns>Created product details</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                if (createProductDto == null)
                    return BadRequest(new { message = "Product data is required" });

                // Get workshop ID from JWT claim or header
                var workshopId = int.TryParse(User.FindFirst("WorkshopId")?.Value, out var id) ? id : 0;
                if (workshopId <= 0)
                    return Unauthorized(new { message = "Workshop ID not found in token" });

                var result = await _productService.CreateProductAsync(workshopId, createProductDto);
                return CreatedAtAction(nameof(GetProductDetails), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing product (Vendor/Workshop only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateProductDto">Updated product data</param>
        /// <returns>Updated product details</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                if (updateProductDto == null)
                    return BadRequest(new { message = "Product data is required" });

                // Get workshop ID from JWT claim
                var workshopId = int.TryParse(User.FindFirst("WorkshopId")?.Value, out var wId) ? wId : 0;
                if (workshopId <= 0)
                    return Unauthorized(new { message = "Workshop ID not found in token" });

                var result = await _productService.UpdateProductAsync(id, workshopId, updateProductDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a product (Vendor/Workshop only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                // Get workshop ID from JWT claim
                var workshopId = int.TryParse(User.FindFirst("WorkshopId")?.Value, out var wId) ? wId : 0;
                if (workshopId <= 0)
                    return Unauthorized(new { message = "Workshop ID not found in token" });

                var result = await _productService.DeleteProductAsync(id, workshopId);
                if (!result)
                    return NotFound(new { message = "Product not found" });

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
