using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Graduation_Application.DTOs.VendorDTO;
using Graduation_Application.IServices;
using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Graduation_Application.Services
{
    public class VendorService : IVendorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IGenaricRepositories<Workshop> _workshopRepository;
        private readonly IFileService _fileService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public VendorService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IGenaricRepositories<Workshop> workshopRepository,
            IFileService fileService,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _workshopRepository = workshopRepository;
            _fileService = fileService;
            _jwtTokenGenerator = jwtTokenGenerator;
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
                var errors = string.Join(" ; ", setPasswordResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to set password: {errors}");
            }

            // Ensure Vendor role exists
            var vendorRoleExists = await _roleManager.RoleExistsAsync("Vendor");
            if (!vendorRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Vendor"));
            }

            // Assign Vendor role to user
            var assignRoleResult = await _userManager.AddToRoleAsync(user, "Vendor");
            if (!assignRoleResult.Succeeded)
            {
                // Rollback: delete user if role assignment fails
                await _userManager.DeleteAsync(user);
                var errors = string.Join(" ; ", assignRoleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign Vendor role: {errors}");
            }

            // Map CreateVendorDto → Workshop using Mapster
            var workshop = dto.Adapt<Workshop>();
            workshop.UserId = user.Id;
            workshop.CreatedAt = DateTime.UtcNow;
            workshop.CreatedBy = "System";
            workshop.IsActive = true;

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
            if (workshop == null) throw new Exception("Workshop not found for user");

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

            if (!user.IsActive)
            {
                throw new Exception("Account is inactive");
            }

            var isVendor = await _userManager.IsInRoleAsync(user, "Vendor");
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
            if (user == null) throw new Exception("Vendor not found");

            var workshop = await _workshopRepository.Where(w => w.UserId == userId).Include(w => w.WorkshopAddress).FirstOrDefaultAsync();
            if (workshop == null) throw new Exception("Workshop not found");

            var dto = (user, workshop).Adapt<VendorProfileDto>();
            if (workshop.WorkshopAddress != null)
            {
                dto.WorkshopAddress = workshop.WorkshopAddress.Adapt<WorkshopAddressDto>();
            }

            return dto;
        }

        public async Task<VendorProfileDto> UpdateVendorProfileAsync(string userId, UpdateVendorProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Vendor not found");

            var workshop = await _workshopRepository.Where(w => w.UserId == userId).Include(w => w.WorkshopAddress).FirstOrDefaultAsync();
            if (workshop == null) throw new Exception("Workshop not found");

            // Update user fields if not null (FullName, PhoneNumber, PreferredLanguage)
            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.PhoneNumber = dto.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(dto.PreferredLanguage))
            {
                user.PreferredLanguage = dto.PreferredLanguage;
            }

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                var errors = string.Join(" ; ", updateUserResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update user: {errors}");
            }

            // Update workshop fields if not null
            if (!string.IsNullOrWhiteSpace(dto.WorkshopNameAr))
            {
                workshop.WorkshopNameAr = dto.WorkshopNameAr;
            }

            if (!string.IsNullOrWhiteSpace(dto.WorkshopNameEn))
            {
                workshop.WorkshopNameEn = dto.WorkshopNameEn;
            }

            if (!string.IsNullOrWhiteSpace(dto.DescriptionAr))
            {
                workshop.DescriptionAr = dto.DescriptionAr;
            }

            if (!string.IsNullOrWhiteSpace(dto.DescriptionEn))
            {
                workshop.DescriptionEn = dto.DescriptionEn;
            }

            // Handle WorkshopAddress updates
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

            _workshopRepository.Update(workshop);
            await _workshopRepository.SaveChangesAsync();

            // Return updated VendorProfileDto
            var result = (user, workshop).Adapt<VendorProfileDto>();
            if (workshop.WorkshopAddress != null)
            {
                result.WorkshopAddress = workshop.WorkshopAddress.Adapt<WorkshopAddressDto>();
            }

            return result;
        }
    }
}
