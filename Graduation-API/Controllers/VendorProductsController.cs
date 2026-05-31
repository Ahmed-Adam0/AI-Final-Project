using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IServices;
using System.IdentityModel.Tokens.Jwt;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/vendor/products")]
    [Authorize(Roles = "Vendor")]
    public class VendorProductsController : ControllerBase
    {
        private readonly IVendorProductService _vendorProductService;

        public VendorProductsController(IVendorProductService vendorProductService)
        {
            _vendorProductService = vendorProductService;
        }

        /// <summary>
        /// Get paginated list of products owned by logged-in vendor
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVendorProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var filter = new ProductFilterDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IsActive = null // Show all products (active and inactive)
                };

                var result = await _vendorProductService.GetVendorProductsAsync(userId, filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get details of a specific vendor product
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorProductDetails(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var product = await _vendorProductService.GetVendorProductDetailsAsync(userId, id);
                if (product == null)
                    return NotFound(new { message = "Product not found" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update product status (activate/deactivate)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] UpdateVendorProductStatusDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                if (dto == null)
                    return BadRequest(new { message = "Status data is required" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _vendorProductService.UpdateVendorProductStatusAsync(userId, id, dto.IsActive);
                return Ok(new { message = "Product status updated successfully", product = result });
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
        /// Delete a vendor product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var result = await _vendorProductService.DeleteVendorProductAsync(userId, id);
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
        /// Get vendor product statistics for dashboard
        /// </summary>
        [HttpGet("stats/overview")]
        public async Task<IActionResult> GetProductStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var stats = await _vendorProductService.GetVendorProductStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get top-rated products owned by vendor
        /// </summary>
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int topCount = 5)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found in token" });

                var products = await _vendorProductService.GetVendorTopProductsAsync(userId, topCount);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to extract user ID from JWT token
        /// </summary>
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }
    }
}
