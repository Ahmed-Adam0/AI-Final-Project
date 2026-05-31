using Microsoft.AspNetCore.Mvc;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IServices;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductService productService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
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
            [FromQuery] bool? isActive = null,
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
                    IsActive = isActive,
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
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                if (createProductDto == null)
                    return BadRequest(new { message = "Product data is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _productService.CreateProductAsync(userId, createProductDto);
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
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                if (updateProductDto == null)
                    return BadRequest(new { message = "Product data is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _productService.UpdateProductAsync(id, userId, updateProductDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
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
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _productService.DeleteProductAsync(id, userId);
                if (!result)
                    return NotFound(new { message = "Product not found" });

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Upload and add an image to a product (Vendor only)
        /// </summary>
        [HttpPost("{productId}/images")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UploadImage(int productId, IFormFile file, [FromQuery] bool isPrimary = false)
        {
            try
            {
                if (productId <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var imageUrl = await SaveImageAsync(file);
                var result = await _productService.AddProductImageAsync(productId, userId, imageUrl, isPrimary);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a product image (Vendor only)
        /// </summary>
        [HttpDelete("{productId}/images/{imageId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> RemoveImage(int productId, int imageId)
        {
            try
            {
                if (productId <= 0 || imageId <= 0)
                    return BadRequest(new { message = "Invalid request parameters" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var productDetails = await _productService.GetProductDetailsAsync(productId);
                var imageDto = productDetails?.Images?.Find(img => img.Id == imageId);

                var result = await _productService.RemoveProductImageAsync(productId, userId, imageId);
                if (!result)
                    return NotFound(new { message = "Image not found for this product" });

                if (imageDto != null)
                {
                    DeleteImageFromDisk(imageDto.ImageUrl);
                }

                return Ok(new { message = "Image removed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Replace a product image (Vendor only)
        /// </summary>
        [HttpPut("{productId}/images/{imageId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> ReplaceImage(int productId, int imageId, IFormFile file)
        {
            try
            {
                if (productId <= 0 || imageId <= 0)
                    return BadRequest(new { message = "Invalid request parameters" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var productDetails = await _productService.GetProductDetailsAsync(productId);
                var oldImageDto = productDetails?.Images?.Find(img => img.Id == imageId);

                var newImageUrl = await SaveImageAsync(file);
                var result = await _productService.ReplaceProductImageAsync(productId, userId, imageId, newImageUrl);

                if (oldImageDto != null)
                {
                    DeleteImageFromDisk(oldImageDto.ImageUrl);
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Set primary image for a product (Vendor only)
        /// </summary>
        [HttpPut("{productId}/images/{imageId}/primary")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> SetPrimaryImage(int productId, int imageId)
        {
            try
            {
                if (productId <= 0 || imageId <= 0)
                    return BadRequest(new { message = "Invalid request parameters" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _productService.SetPrimaryImageAsync(productId, userId, imageId);
                if (!result)
                    return NotFound(new { message = "Image not found for this product" });

                return Ok(new { message = "Primary image updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var productsFolder = Path.Combine(webRootPath, "images", "products");

            if (!Directory.Exists(productsFolder))
            {
                Directory.CreateDirectory(productsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(productsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/products/{uniqueFileName}";
        }

        private void DeleteImageFromDisk(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl)) return;

                var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var relativePath = imageUrl.TrimStart('/');
                var physicalPath = Path.Combine(webRootPath, relativePath);

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            catch
            {
                // Soft fail disk write error to prevent DB rollback
            }
        }

        /// <summary>
        /// Set product status (Active / Inactive) (Vendor only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> SetProductStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _productService.SetProductStatusAsync(id, userId, isActive);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
