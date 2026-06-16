using Mapster;
using Graduation_Application.DTOs.VendorDTO;
using Graduation_domain.Entities;

namespace Graduation_Application.Mapper.VendorMapping
{
    public static class UpdateVendorProfileMappingConfig
    {
        public static void RegisterMappings()
        {
            // UpdateVendorProfileDto → ApplicationUser (ignore nulls and identity-only fields)
            TypeAdapterConfig<UpdateVendorProfileDto, ApplicationUser>
                .NewConfig()
                .IgnoreNullValues(true)
                .Ignore(dest => dest.Email)
                .Ignore(dest => dest.UserName)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.PasswordHash)
                .Ignore(dest => dest.SecurityStamp)
                .Ignore(dest => dest.ConcurrencyStamp)
                .Ignore(dest => dest.Addresses)
                .Ignore(dest => dest.Carts)
                .Ignore(dest => dest.Favorites)
                .Ignore(dest => dest.Orders)
                .Ignore(dest => dest.Workshops)
                .Ignore(dest => dest.Reviews);

            // UpdateVendorProfileDto → Workshop (ignore nulls)
            TypeAdapterConfig<UpdateVendorProfileDto, Workshop>
                .NewConfig()
                .IgnoreNullValues(true)
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
        }
    }
}
