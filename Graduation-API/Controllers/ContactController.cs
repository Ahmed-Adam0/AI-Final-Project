using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_Application.DTOs.ContactDTO;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILocalizationService _localizationService;

        public ContactController(IEmailService emailService, ILocalizationService localizationService)
        {
            _emailService = emailService;
            _localizationService = localizationService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitInquiry([FromBody] ContactFormDto model)
        {
            var lang = _localizationService.GetCurrentCulture();

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => {
                            // Translate default validation error messages to the active language
                            if (e.ErrorMessage.Contains("Name is required")) return _localizationService.Get("contact.nameRequired", lang);
                            if (e.ErrorMessage.Contains("Name must be")) return _localizationService.Get("contact.nameMinLength", lang);
                            if (e.ErrorMessage.Contains("Email is required")) return _localizationService.Get("contact.emailRequired", lang);
                            if (e.ErrorMessage.Contains("Invalid email address")) return _localizationService.Get("contact.emailInvalid", lang);
                            if (e.ErrorMessage.Contains("Phone number is required")) return _localizationService.Get("contact.phoneRequired", lang);
                            if (e.ErrorMessage.Contains("Invalid Egyptian phone number")) return _localizationService.Get("contact.phoneInvalid", lang);
                            if (e.ErrorMessage.Contains("Subject is required")) return _localizationService.Get("contact.subjectRequired", lang);
                            if (e.ErrorMessage.Contains("Subject must be")) return _localizationService.Get("contact.subjectMinLength", lang);
                            if (e.ErrorMessage.Contains("Message is required")) return _localizationService.Get("contact.messageRequired", lang);
                            if (e.ErrorMessage.Contains("Message must be")) return _localizationService.Get("contact.messageMinLength", lang);
                            return e.ErrorMessage;
                        }).ToArray()
                    );

                return BadRequest(new
                {
                    success = false,
                    errors
                });
            }

            try
            {
                await _emailService.SendContactInquiryEmailAsync(model);
                
                var successMessage = _localizationService.Get("contact.success", lang);
                
                return Ok(new
                {
                    success = true,
                    message = successMessage
                });
            }
            catch (Exception ex)
            {
                // Log exception for server side diagnostics
                Console.Error.WriteLine($"Contact form submission failure: {ex}");

                var errorMessage = _localizationService.Get("contact.error", lang);

                return StatusCode(500, new
                {
                    success = false,
                    message = errorMessage
                });
            }
        }
    }
}
