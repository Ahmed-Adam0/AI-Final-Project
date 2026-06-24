using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.IServices;
using Graduation_Application.Mapper.UsersMapping;
using Graduation_domain.Enums;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly IMemoryCache _cache;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService,
            IConfiguration configuration,
            IInternalNotificationService internalNotificationService,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _configuration = configuration;
            _internalNotificationService = internalNotificationService;
            _cache = cache;
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
            var roleExists = await _roleManager.RoleExistsAsync(Roles.Customer);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Customer));
            }

            // Add role to user
           var userdb= await _userManager.AddToRoleAsync(user, Roles.Customer);
            if (!userdb.Succeeded)
            {
                // Rollback: delete user if role assignment fails
                await _userManager.DeleteAsync(user);
                var errors = string.Join(" ; ", userdb.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign Vendor role: {errors}");
            }
            // Send OTP for email confirmation
            int expiry = int.Parse(_configuration["OtpSettings:ExpiryInMinutes"]!);
            string otp = new Random().Next(100000, 999999).ToString();

            user.OtpEmail = otp;
            user.OtpEmailExpiry = DateTime.UtcNow.AddMinutes(expiry);
            await _userManager.UpdateAsync(user);
            
            // The SendEmailConfirmationOtpAsync might fail if the email is invalid,
            // but the user is already created at this point.
            await _emailService.SendEmailConfirmationOtpAsync(dto.Email, otp, expiry);

            // Get roles for response mapping
            var roles = await _userManager.GetRolesAsync(user);

            // Mapping → AuthResponseDto
            return (user, roles).Adapt<AuthResponseDto>();
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
            //only user Customer Can login
            if (!roles.Contains(Roles.Customer))
            {
                throw new Exception("Only customers can login from this endpoint.");
            }
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

            if (!string.IsNullOrEmpty(user.GoogleId))
            {
                throw new Exception("Password reset is not available for Google accounts.");
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

            if (!string.IsNullOrEmpty(user.GoogleId))
            {
                throw new Exception("Password reset is not available for Google accounts.");
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

            // Look up user by GoogleId (sub claim) first
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

            if (user != null)
            {
                // Auto-sync email if changed on Google side
                if (!string.Equals(user.Email, payload.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailConflict = await _userManager.FindByEmailAsync(payload.Email);
                    if (emailConflict != null)
                    {
                        throw new Exception($"Cannot update email from Google to '{payload.Email}' because another account is already using it.");
                    }

                    user.Email = payload.Email;
                    user.NormalizedEmail = payload.Email.ToUpperInvariant();

                    if (string.Equals(user.UserName, user.Email, StringComparison.OrdinalIgnoreCase) || user.UserName.Contains("@"))
                    {
                        user.UserName = payload.Email;
                        user.NormalizedUserName = payload.Email.ToUpperInvariant();
                    }

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

            // If not found by GoogleId, check if a user with that email already exists
            var userByEmail = await _userManager.FindByEmailAsync(payload.Email);
            if (userByEmail != null)
            {
                throw new Exception("This email address is already associated with another login method.");
            }

            // Generate a secure registration token
            var rawToken = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            var registrationToken = Guid.NewGuid().ToString("N") + Convert.ToBase64String(rawToken)
                .Replace("+", "").Replace("/", "").Replace("=", "");

            // Read cache expiry (default 15 minutes)
            var expiryMinutes = 15;
            if (int.TryParse(_configuration["Google:RegistrationExpiryMinutes"], out var configuredExpiry))
            {
                expiryMinutes = configuredExpiry;
            }

            var cacheItem = new GoogleRegistrationCacheItem
            {
                Email = payload.Email,
                GoogleId = payload.Subject,
                FullName = payload.Name ?? payload.Email,
                ProfileImage = payload.Picture ?? string.Empty
            };

            var cacheKey = $"GoogleReg_{registrationToken}";
            _cache.Set(cacheKey, cacheItem, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes)
            });

            return new AuthResponseDto
            {
                RegistrationRequired = true,
                RegistrationToken = registrationToken,
                GoogleProfile = new GoogleProfileDto
                {
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? string.Empty,
                    LastName = payload.FamilyName ?? string.Empty,
                    ProfileImage = payload.Picture ?? string.Empty
                }
            };
        }

        public async Task<AuthResponseDto> CompleteGoogleRegistrationAsync(CompleteGoogleRegistrationDto dto)
        {
            var cacheKey = $"GoogleReg_{dto.RegistrationToken}";
            if (!_cache.TryGetValue<GoogleRegistrationCacheItem>(cacheKey, out var cacheItem) || cacheItem == null)
            {
                throw new Exception("Registration session has expired or is invalid");
            }

            // Check if user already exists by GoogleId or Email (prevent duplicate/replay account creation)
            var existingGoogleUser = await _userManager.Users.FirstOrDefaultAsync(u => u.GoogleId == cacheItem.GoogleId);
            if (existingGoogleUser != null)
            {
                throw new Exception("This Google account is already registered.");
            }

            var existingUser = await _userManager.FindByEmailAsync(cacheItem.Email);
            if (existingUser != null)
            {
                throw new Exception($"User with email '{cacheItem.Email}' already exists");
            }

            // Map and create new user
            var newUser = new ApplicationUser
            {
                Email = cacheItem.Email,
                UserName = cacheItem.Email,
                FullName = cacheItem.FullName,
                ProfileImage = cacheItem.ProfileImage,
                GoogleId = cacheItem.GoogleId,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(newUser, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ; ", createResult.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            // Link Google Login Provider to the new account
            var loginInfo = new UserLoginInfo("Google", cacheItem.GoogleId, "Google");
            var addLoginResult = await _userManager.AddLoginAsync(newUser, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                // Rollback user creation if linking provider fails
                await _userManager.DeleteAsync(newUser);
                var errors = string.Join(" ; ", addLoginResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to link external provider: {errors}");
            }

            // Ensure Customer role exists and assign
            var roleExists = await _roleManager.RoleExistsAsync(Roles.Customer);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Customer));
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(newUser, Roles.Customer);
            if (!addToRoleResult.Succeeded)
            {
                // Rollback
                await _userManager.DeleteAsync(newUser);
                var errors = string.Join(" ; ", addToRoleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign role: {errors}");
            }

            // Evict registration token from cache (single-use)
            _cache.Remove(cacheKey);

            var roles = await _userManager.GetRolesAsync(newUser);
            var token = _jwtTokenGenerator.GenerateToken(newUser, roles);

            return (newUser, token, roles).Adapt<AuthResponseDto>();
        }

        private class GoogleRegistrationCacheItem
        {
            public string Email { get; set; } = string.Empty;
            public string GoogleId { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string ProfileImage { get; set; } = string.Empty;
        }
    }
}
