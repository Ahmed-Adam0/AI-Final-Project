using Mapster;
using Graduation_Application.DTOs.VendorDTO;
using Graduation_domain.Entities;

namespace Graduation_Application.Mapper.VendorMapping
{
    public static class VendorMappingConfig
    {
        public static void RegisterMappings()
        {
            // WorkshopAddressDto → WorkshopAddress
            TypeAdapterConfig<WorkshopAddressDto, WorkshopAddress>
                .NewConfig()
                .Map(dest => dest.City, src => src.City)
                .Map(dest => dest.Area, src => src.Area)
                .Map(dest => dest.Street, src => src.Street)
                .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
                .Map(dest => dest.Notes, src => src.Notes)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.WorkshopId)
                .Ignore(dest => dest.Workshop)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.CreatedBy)
                .Ignore(dest => dest.UpdatedBy)
                .Ignore(dest => dest.IsActive);

            // WorkshopAddress → WorkshopAddressDto
            TypeAdapterConfig<WorkshopAddress, WorkshopAddressDto>
                .NewConfig()
                .Map(dest => dest.City, src => src.City)
                .Map(dest => dest.Area, src => src.Area)
                .Map(dest => dest.Street, src => src.Street)
                .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
                .Map(dest => dest.Notes, src => src.Notes);

            // CreateVendorDto → ApplicationUser
            TypeAdapterConfig<CreateVendorDto, ApplicationUser>
                .NewConfig()
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.UserName, src => src.Email)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage)
                .Map(dest => dest.EmailConfirmed, src =>  false)
                .Map(dest => dest.IsActive, src => false)
                .Ignore(dest => dest.PasswordHash)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.NormalizedEmail)
                .Ignore(dest => dest.NormalizedUserName)
                .Ignore(dest => dest.SecurityStamp)
                .Ignore(dest => dest.ConcurrencyStamp);

            // CreateVendorDto → Workshop
            TypeAdapterConfig<CreateVendorDto, Workshop>
                .NewConfig()
                .Map(dest => dest.WorkshopNameAr, src => src.WorkshopNameAr)
                .Map(dest => dest.WorkshopNameEn, src => src.WorkshopNameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .Map(dest => dest.IsVerified, src => true)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.UserId)
                .Ignore(dest => dest.User)
                .Ignore(dest => dest.Products)
                .Ignore(dest => dest.Reviews)
                .Ignore(dest => dest.WorkshopAddress)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.CreatedBy)
                .Ignore(dest => dest.UpdatedBy)
                .Ignore(dest => dest.Rating);

            // UpdateVendorProfileDto → ApplicationUser
            TypeAdapterConfig<UpdateVendorProfileDto, ApplicationUser>
                .NewConfig()
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage)
                .IgnoreNullValues(true)
                .Ignore(dest => dest.Email)
                .Ignore(dest => dest.UserName)
                .Ignore(dest => dest.NormalizedEmail)
                .Ignore(dest => dest.NormalizedUserName)
                .Ignore(dest => dest.PasswordHash)
                .Ignore(dest => dest.SecurityStamp)
                .Ignore(dest => dest.ConcurrencyStamp)
                .Ignore(dest => dest.Id);

            // UpdateVendorProfileDto → Workshop
            TypeAdapterConfig<UpdateVendorProfileDto, Workshop>
                .NewConfig()
                .Map(dest => dest.WorkshopNameAr, src => src.WorkshopNameAr)
                .Map(dest => dest.WorkshopNameEn, src => src.WorkshopNameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .IgnoreNullValues(true)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.UserId)
                .Ignore(dest => dest.User)
                .Ignore(dest => dest.LogoUrl)
                .Ignore(dest => dest.Rating)
                .Ignore(dest => dest.IsVerified)
                .Ignore(dest => dest.Products)
                .Ignore(dest => dest.Reviews)
                .Ignore(dest => dest.WorkshopAddress)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.CreatedBy)
                .Ignore(dest => dest.UpdatedBy)
                .Ignore(dest => dest.IsActive);

            // (ApplicationUser, Workshop) → VendorProfileDto
            TypeAdapterConfig<(ApplicationUser user, Workshop workshop), VendorProfileDto>
                .NewConfig()
                .Map(dest => dest.UserId, src => src.user.Id)
                .Map(dest => dest.FullName, src => src.user.FullName)
                .Map(dest => dest.Email, src => src.user.Email)
                .Map(dest => dest.PhoneNumber, src => src.user.PhoneNumber)
                .Map(dest => dest.PreferredLanguage, src => src.user.PreferredLanguage)
                .Map(dest => dest.ProfileImage, src => src.user.ProfileImage)
                .Map(dest => dest.WorkshopId, src => src.workshop.Id)
                .Map(dest => dest.WorkshopNameAr, src => src.workshop.WorkshopNameAr)
                .Map(dest => dest.WorkshopNameEn, src => src.workshop.WorkshopNameEn)
                .Map(dest => dest.DescriptionAr, src => src.workshop.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.workshop.DescriptionEn)
                .Map(dest => dest.LogoUrl, src => src.workshop.LogoUrl)
                .Map(dest => dest.Rating, src => src.workshop.Rating)
                .Map(dest => dest.IsVerified, src => src.workshop.IsVerified)
                .Ignore(dest => dest.WorkshopAddress);
        }
    }
}
