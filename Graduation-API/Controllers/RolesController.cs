using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Graduation_Application.IServices;
using Graduation_Application.DTOs.RolesDTO;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleDto dto)
        {
            try
            {
                var role = await _roleService.CreateRoleAsync(dto);
                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleAsync([FromBody] AssignRoleDto dto)
        {
            try
            {
                await _roleService.AssignRoleAsync(dto);
                return Ok(new { Message = "Role assigned" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
