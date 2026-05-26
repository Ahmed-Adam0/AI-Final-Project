using Microsoft.AspNetCore.Mvc;
using Graduation_Application.IServices;
using Graduation_Application.DTOs.CategoryDTO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
        /// Create a new category (Admin only)
        /// </summary>
        /// <param name="createCategoryDto">Category creation data</param>
        /// <returns>Created category details</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (createCategoryDto == null)
                    return BadRequest(new { message = "Category data is required" });

                var result = await _categoryService.CreateCategoryAsync(createCategoryDto);
                return CreatedAtAction(nameof(GetAllCategories), result);
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
        /// Update an existing category (Admin only)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="updateCategoryDto">Updated category data</param>
        /// <returns>Updated category details</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid category ID" });

                if (updateCategoryDto == null)
                    return BadRequest(new { message = "Category data is required" });

                var result = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
                return Ok(result);
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
        /// Delete a category (Admin only)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid category ID" });

                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound(new { message = "Category not found" });

                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
