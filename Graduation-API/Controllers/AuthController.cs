using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Graduation_Application.IServices;
using Graduation_Application.DTOs.UserDTO;

namespace Graduation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(new { Success = true, Message = "Registration successful", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(new { Success = true, Message = "Login successful", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                await _authService.ForgotPasswordAsync(dto);
                return Ok(new { Success = true, Message = "OTP code has been sent to your email" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtpAsync([FromBody] VerifyOtpDto dto)
        {
            try
            {
                var result = await _authService.VerifyOtpAsync(dto);
                if (result)
                {
                    return Ok(new { Success = true, IsValid = result, Message = "OTP verified successfully" });
                }
                return BadRequest(new { Success = false, IsValid = result, Message = "Invalid OTP code or code has expired" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { Success = true, Message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("confirm-email-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailOtpAsync([FromBody] ConfirmEmailOtpDto dto)
        {
            try
            {
                await _authService.ConfirmEmailOtpAsync(dto);
                return Ok(new { Success = true, Message = "Email confirmed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmationAsync([FromBody] ResendConfirmationDto dto)
        {
            try
            {
                await _authService.ResendConfirmationEmailAsync(dto);
                return Ok(new { Success = true, Message = "OTP code For Confirmation Your Email has been sent to your email" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] Graduation_Application.DTOs.UserDTO.GoogleLoginDto dto)
        {
            try
            {
                var result = await _authService.GoogleLoginAsync(dto);
                return Ok(new { Success = true, Message = "Login successful", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}

