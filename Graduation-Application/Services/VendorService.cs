using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.Constants;
using Graduation_Application.DTOs.UserDTO;
using Graduation_Application.DTOs.VendorDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class VendorService : IVendorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IFileService _fileService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IAuthService _authService;

        public VendorService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IGenaricRepositories<Workshop> workshopRepository,
            IGenaricRepositories<Product> productRepository,
            IFileService fileService,
            IJwtTokenGenerator jwtTokenGenerator,
            IAuthService authService
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _workshopRepository = workshopRepository;
            _productRepository = productRepository;
            _fileService = fileService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _authService = authService;
        }

        public async Task CreateVendorAsync(CreateVendorDto dto)
        {
            // Check if email already exists
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
            {
                throw new Exception($"Email '{dto.Email}' already exists");
            }

            // Map CreateVendorDto → ApplicationUser using Mapster
            var user = dto.Adapt<ApplicationUser>();

            // Create user in database (without password)
            var createUserResult = await _userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(" ; ", createUserResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user: {errors}");
            }

            // Set password
            var setPasswordResult = await _userManager.AddPasswordAsync(user, dto.Password);
            if (!setPasswordResult.Succeeded)
            {
                // Rollback: delete user if password setup fails
                await _userManager.DeleteAsync(user);
                var errors = string.Join(
                    " ; ",
                    setPasswordResult.Errors.Select(e => e.Description)
                );
                throw new Exception($"Failed to set password: {errors}");
            }

            // Ensure Vendor role exists
            var vendorRoleExists = await _roleManager.RoleExistsAsync(Roles.Vendor);
            if (!vendorRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Vendor));
            }

            // Assign Vendor role to user
            var assignRoleResult = await _userManager.AddToRoleAsync(user, Roles.Vendor);
            if (!assignRoleResult.Succeeded)
            {
                // Rollback: delete user if role assignment fails
                await _userManager.DeleteAsync(user);
                var errors = string.Join(" ; ", assignRoleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign Vendor role: {errors}");
            }

            await _authService.ResendConfirmationEmailAsync(
                new ResendConfirmationDto { Email = dto.Email }
            );

            // Map CreateVendorDto → Workshop using Mapster
            var workshop = dto.Adapt<Workshop>();
            workshop.UserId = user.Id;
            workshop.CreatedAt = DateTime.UtcNow;
            workshop.CreatedBy = "System";
            workshop.IsActive = false;

            await _workshopRepository.AddAsync(workshop);
            await _workshopRepository.SaveChangesAsync();

            // If WorkshopAddress provided, create and link it to workshop
            if (dto.WorkshopAddress != null)
            {
                var address = dto.WorkshopAddress.Adapt<WorkshopAddress>();
                address.WorkshopId = workshop.Id;
                address.CreatedAt = DateTime.UtcNow;
                address.CreatedBy = "System";
                address.IsActive = true;

                workshop.WorkshopAddress = address;
                _workshopRepository.Update(workshop);
                await _workshopRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateVendorLogoAsync(string userId, IFormFile logo)
        {
            var workshop = await _workshopRepository.FirstOrDefaultAsync(w => w.UserId == userId);
            if (workshop == null)
                throw new Exception("Workshop not found for user");

            var newUrl = await _fileService.SaveImageAsync(logo, "workshops", workshop.LogoUrl);
            workshop.LogoUrl = newUrl;
            _workshopRepository.Update(workshop);
            await _workshopRepository.SaveChangesAsync();
        }

        public async Task<VendorAuthResponseDto> VendorLoginAsync(VendorLoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                throw new Exception("Invalid email or password");
            }

            // Check email confirmation first
            if (!user.EmailConfirmed)
            {
                await _authService.ResendConfirmationEmailAsync(
                    new ResendConfirmationDto { Email = dto.Email }
                );
                throw new Exception("Email not confirmed. OTP sent to your email.");
            }

            // Check admin approval
            if (!user.IsActive)
            {
                throw new Exception("Your account is pending admin approval.");
            }

            var isVendor = await _userManager.IsInRoleAsync(user, Roles.Vendor);
            if (!isVendor)
            {
                throw new Exception("Invalid email or password");
            }

            var workshop = await _workshopRepository.FirstOrDefaultAsync(w => w.UserId == user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            return (user, token, roles, workshop).Adapt<VendorAuthResponseDto>();
        }

        public async Task<VendorProfileDto> GetVendorProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Vendor not found");

            var workshop = await _workshopRepository
                .Where(w => w.UserId == userId)
                .Include(w => w.WorkshopAddress)
                .FirstOrDefaultAsync();
            if (workshop == null)
                throw new Exception("Workshop not found");
            if (!user.EmailConfirmed)
                throw new Exception("Email not confirmed.");

            if (!user.IsActive)
                throw new Exception("Your account is pending admin approval.");
            var dto = (user, workshop).Adapt<VendorProfileDto>();
            dto.WorkshopAddress = workshop.WorkshopAddress?.Adapt<WorkshopAddressDto>();
            return dto;
        }

        public async Task<VendorProfileDto> UpdateVendorProfileAsync(
            string userId,
            UpdateVendorProfileDto dto
        )
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Vendor not found");

            var workshop = await _workshopRepository
                .Where(w => w.UserId == userId)
                .Include(w => w.WorkshopAddress)
                .FirstOrDefaultAsync();
            if (workshop == null)
                throw new Exception("Workshop not found");
            if (!user.EmailConfirmed)
                throw new Exception("Email not confirmed.");

            if (!user.IsActive)
                throw new Exception("Your account is pending admin approval.");
            // 1. Update non-email user fields via Mapster
            dto.Adapt(user);

            // 2. Update Email via Identity if provided
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, dto.Email);
                if (!setEmailResult.Succeeded)
                {
                    var errors = string.Join(
                        " ; ",
                        setEmailResult.Errors.Select(e => e.Description)
                    );
                    throw new Exception($"Failed to set email: {errors}");
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.Email);
                if (!setUserNameResult.Succeeded)
                {
                    var errors = string.Join(
                        " ; ",
                        setUserNameResult.Errors.Select(e => e.Description)
                    );
                    throw new Exception($"Failed to set username: {errors}");
                }
            }
            else
            {
                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    var errors = string.Join(
                        " ; ",
                        updateUserResult.Errors.Select(e => e.Description)
                    );
                    throw new Exception($"Failed to update user: {errors}");
                }
            }

            // 3. Update workshop fields via Mapster
            dto.Adapt(workshop);

            // 4. WorkshopAddress — keep existing logic unchanged
            if (dto.WorkshopAddress != null)
            {
                if (workshop.WorkshopAddress != null)
                {
                    // Update existing WorkshopAddress
                    dto.WorkshopAddress.Adapt(workshop.WorkshopAddress);
                }
                else
                {
                    // Create new WorkshopAddress
                    workshop.WorkshopAddress = dto.WorkshopAddress.Adapt<WorkshopAddress>();
                    workshop.WorkshopAddress.WorkshopId = workshop.Id;
                    workshop.WorkshopAddress.CreatedAt = DateTime.UtcNow;
                    workshop.WorkshopAddress.CreatedBy = "System";
                    workshop.WorkshopAddress.IsActive = true;
                }
            }

            // 5. Save and return
            _workshopRepository.Update(workshop);
            await _workshopRepository.SaveChangesAsync();
            var result = (user, workshop).Adapt<VendorProfileDto>();
            result.WorkshopAddress = workshop.WorkshopAddress?.Adapt<WorkshopAddressDto>();
            return result;
        }

        /// <summary>
        /// Get total product count for vendor
        /// </summary>
        public async Task<int> GetVendorProductCountAsync(string userId)
        {
            var workshop = await _workshopRepository.FirstOrDefaultAsync(w => w.UserId == userId);
            if (workshop == null) return 0;

            var workshopWithProducts = await _workshopRepository
                .Where(w => w.Id == workshop.Id)
                .Include(w => w.Products)
                .FirstOrDefaultAsync();

            return workshopWithProducts?.Products?.Count ?? 0;
        }

        /// <summary>
        /// Link existing products to vendor by UserId
        /// Useful for migration from WorkshopId-based to UserId-based ownership
        /// </summary>
        public async Task LinkVendorProductsAsync(string userId)
        {
            // Note: Since we moved WorkshopId/UserId from Product to VendorProductListing,
            // products are linked to vendor via VendorProductListing.
            // This migration method is no longer needed in this format, so we can make it a no-op or log it.
            await Task.CompletedTask;
        }
    }
}
