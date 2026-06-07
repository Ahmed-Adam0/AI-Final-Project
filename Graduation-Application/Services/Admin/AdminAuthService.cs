using System;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.Admin.AuthAdmin;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Graduation_Application.Services.Admin
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AdminAuthService(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<AdminAuthResultDto> LoginAsync(AdminLoginDto dto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }

            // Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                throw new Exception("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw new Exception("Your account is inactive");
            }

            // Check if user is SuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(user, Roles.SuperAdmin);
            if (!isSuperAdmin)
            {
                throw new Exception("Unauthorized access");
            }

            // Map to DTO
            var result = user.Adapt<AdminAuthResultDto>();
            result.Role = Roles.SuperAdmin;

            return result;
        }

        public async Task ForgotPasswordAsync(AdminForgotPasswordDto dto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw new Exception("Account is inactive");
            }

            // Check if user is SuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(user, Roles.SuperAdmin);
            if (!isSuperAdmin)
            {
                throw new Exception("Unauthorized access");
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Get OTP expiry from configuration
            var otpExpiryMinutes = int.Parse(_configuration["OtpSettings:ExpiryInMinutes"] ?? "10");
            var otpExpiry = DateTime.UtcNow.AddMinutes(otpExpiryMinutes);

            // Set OTP on user
            user.OtpCode = otp;
            user.OtpExpiry = otpExpiry;

            // Update user
            await _userManager.UpdateAsync(user);

            // Send OTP via email
            await _emailService.SendOtpEmailAsync(user.Email, otp, otpExpiryMinutes);
        }

        public async Task<bool> VerifyOtpAsync(AdminVerifyOtpDto dto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Check if OTP matches
            if (user.OtpCode != dto.OtpCode)
            {
                return false;
            }

            // Check if OTP is expired
            if (user.OtpExpiry <= DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task ResetPasswordAsync(AdminResetPasswordDto dto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Check if OTP matches
            if (user.OtpCode != dto.OtpCode)
            {
                throw new Exception("Invalid OTP");
            }

            // Check if OTP is expired
            if (user.OtpExpiry <= DateTime.UtcNow)
            {
                throw new Exception("OTP expired");
            }

            // Remove current password and add new password
            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, dto.NewPassword);

            // Clear OTP
            user.OtpCode = null;
            user.OtpExpiry = null;

            // Update user
            await _userManager.UpdateAsync(user);
        }
    }
}
