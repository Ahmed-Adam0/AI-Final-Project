using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.IServices;
using Graduation_Application.Mapper.UsersMapping;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Graduation_Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IInternalNotificationService _internalNotificationService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService,
            IConfiguration configuration,
            IInternalNotificationService internalNotificationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _configuration = configuration;
            _internalNotificationService = internalNotificationService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);

            if (userExists != null)
            {
                throw new Exception($"User with email '{dto.Email}' already exists");
            }

            // Mapping RegisterDto → ApplicationUser
            var user = dto.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ; ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            // Create role if not exists
            var roleExists = await _roleManager.RoleExistsAsync("Customer");

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // Add role to user
            await _userManager.AddToRoleAsync(user, "Customer");

            // Generate token
            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            // Mapping → AuthResponseDto
            return (user, token, roles).Adapt<AuthResponseDto>();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception($"InValid Email Or Password");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordValid)
            {
                throw new Exception("InValid Email Or Password");
            }

            if (!user.IsActive)
            {
                throw new Exception("User account is inactive");
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                int expiry = int.Parse(_configuration["OtpSettings:ExpiryInMinutes"]!);
                string otp = new Random().Next(100000, 999999).ToString();

                user.OtpEmail = otp;
                user.OtpEmailExpiry = DateTime.UtcNow.AddMinutes(expiry);

                await _userManager.UpdateAsync(user);
                await _emailService.SendEmailConfirmationOtpAsync(dto.Email, otp, expiry);

                throw new Exception("OTP sent to your email to confirm your account.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            // Mapping → AuthResponseDto
            return (user, token, roles).Adapt<AuthResponseDto>();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception($"User with email '{dto.Email}' not found");
            }

            if (!user.IsActive)
            {
                throw new Exception("Email address is not Active. Please Active your email first");
            }

            int expiry = int.Parse(_configuration["OtpSettings:ExpiryInMinutes"]!);
            string otp = new Random().Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(expiry);

            await _userManager.UpdateAsync(user);
            await _emailService.SendOtpEmailAsync(dto.Email, otp, expiry);
            await _internalNotificationService.CreateAsync(user.Id, NotificationType.PasswordReset);
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception($"User with email '{dto.Email}' not found");
            }

            if (user.OtpCode != dto.OtpCode)
            {
                return false;
            }

            if (user.OtpExpiry <= DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception($"User with email '{dto.Email}' not found");
            }

            if (user.OtpCode != dto.OtpCode)
            {
                throw new Exception("Invalid OTP code");
            }

            if (user.OtpExpiry <= DateTime.UtcNow)
            {
                throw new Exception("OTP code has expired");
            }

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ; ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            user.OtpCode = null;
            user.OtpExpiry = null;

            await _userManager.UpdateAsync(user);
            await _internalNotificationService.CreateAsync(user.Id, NotificationType.PasswordReset);
        }

        public async Task ConfirmEmailOtpAsync(ConfirmEmailOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception($"User with email '{dto.Email}' not found");
            }

            if (user.EmailConfirmed)
            {
                throw new Exception("Email is already confirmed");
            }

            if (user.OtpEmail != dto.OtpCodeEmail)
            {
                throw new Exception("Invalid OTP code");
            }

            if (user.OtpEmailExpiry <= DateTime.UtcNow)
            {
                throw new Exception("OTP code has expired");
            }

            user.EmailConfirmed = true;
            user.OtpEmail = null;
            user.OtpEmailExpiry = null;

            await _userManager.UpdateAsync(user);
        }

        public async Task ResendConfirmationEmailAsync(ResendConfirmationDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new Exception($"User with email '{dto.Email}' not found");
            }

            if (user.EmailConfirmed)
            {
                throw new Exception("Email is already confirmed");
            }

            int expiry = int.Parse(_configuration["OtpSettings:ExpiryInMinutes"]!);
            string otp = new Random().Next(100000, 999999).ToString();

            user.OtpEmail = otp;
            user.OtpEmailExpiry = DateTime.UtcNow.AddMinutes(expiry);

            await _userManager.UpdateAsync(user);
            await _emailService.SendEmailConfirmationOtpAsync(dto.Email, otp, expiry);
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
        {
            // Validate Google IdToken
            var settingsClientId = _configuration["Google:ClientId"];
            if (string.IsNullOrWhiteSpace(settingsClientId))
            {
                throw new Exception("Google ClientId is not configured.");
            }

            Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
            try
            {
                var validationSettings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { settingsClientId }
                };

                payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.IdToken, validationSettings);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid Google Id token: " + ex.Message);
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new Exception("Google token does not contain email.");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user != null)
            {
                // Existing user: update GoogleId if not set
                if (string.IsNullOrWhiteSpace(user.GoogleId))
                {
                    user.GoogleId = payload.Subject; // sub claim
                    await _userManager.UpdateAsync(user);
                }

                if (!user.IsActive)
                {
                    throw new Exception("User account is inactive");
                }

                var rolesExisting = await _userManager.GetRolesAsync(user);
                var tokenExisting = _jwtTokenGenerator.GenerateToken(user, rolesExisting);
                return (user, tokenExisting, rolesExisting).Adapt<AuthResponseDto>();
            }

            // Auto-register new user via Mapster mapping from Google payload
            var newUser = payload.Adapt<ApplicationUser>();

            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ; ", createResult.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            // Ensure Customer role exists and assign
            var roleExists = await _roleManager.RoleExistsAsync("Customer");
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            await _userManager.AddToRoleAsync(newUser, "Customer");

            var roles = await _userManager.GetRolesAsync(newUser);
            var token = _jwtTokenGenerator.GenerateToken(newUser, roles);

            return (newUser, token, roles).Adapt<AuthResponseDto>();
        }
    }
}
