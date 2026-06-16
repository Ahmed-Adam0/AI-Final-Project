using System;
using System.Threading.Tasks;
using Graduation_Application.DTOs.CategoryDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        /// <returns>List of categories</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get category taxonomy tree (Categories -> SubCategories -> ProductTypes)
        /// </summary>
        /// <returns>Nested list of category taxonomy</returns>
        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            try
            {
                var tree = await _categoryService.GetCategoryTreeAsync();
                return Ok(tree);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
