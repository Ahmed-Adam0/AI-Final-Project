using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest(new { Message = "Role Name Cannot be Empty" });
            }
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (roleExist)
            {
                return BadRequest(new { Message = "Role Already Exists" });
            }
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role Created Successfully" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound(new { Message = "User Not Found" });
            }
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                return NotFound(new { Message = "Role Not Found" });
            }
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role Assigned Successfully" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            if (roles == null || roles.Count == 0)
            {
                return NotFound(new { Message = "No Roles Found" });
            }
            return Ok(roles);
        }
    }
}
