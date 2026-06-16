using Graduation_Application.DTOs.Admin.AdminProductDTO;
using Graduation_Application.IServices.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IAdminProductService _adminProductService;

        public AdminProductsController(IAdminProductService adminProductService)
        {
            _adminProductService = adminProductService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] AdminProductFilterDto filter)
        {
            try
            {
                var result = await _adminProductService.GetProductsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            try
            {
                var product = await _adminProductService.GetProductDetailsAsync(id);
                if (product == null)
                    return NotFound(new { message = "Product not found" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
