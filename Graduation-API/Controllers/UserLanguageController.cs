using Graduation_Application.DTOs.UserLanguageDTO;
using Graduation_Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/user-language")]
    [Authorize]
    public class UserLanguageController : ControllerBase
    {
        private readonly ILanguageUserService _languageService;

        public UserLanguageController(ILanguageUserService languageService)
        {
            _languageService = languageService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetLanguage()
        {
            var userId = GetUserId();

            var lang = await _languageService.GetUserLanguageAsync(userId);

            return Ok(new UserLanguageDto
            {
                Language = lang
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLanguage([FromBody] UpdateUserLanguageDto dto)
        {
            var userId = GetUserId();

            var command = new UpdateUserLanguageCommand
            {
                UserId = userId,
                Language = dto.Language
            };

            await _languageService.UpdateUserLanguageAsync(command);

            return Ok(new
            {
                message = "Language updated successfully",
                language = dto.Language
            });
        }
    }
}
