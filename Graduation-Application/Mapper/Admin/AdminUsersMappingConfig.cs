using Graduation_Application.DTOs.Admin.UsersDTO;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.Admin
{
    public static class AdminUsersMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<ApplicationUser, AdminUserListItemDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.EmailConfirmed, src => src.EmailConfirmed)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);

            TypeAdapterConfig<ApplicationUser, AdminUserDetailsDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.ProfileImage, src => src.ProfileImage)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.EmailConfirmed, src => src.EmailConfirmed)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.TotalOrders)
                .Ignore(dest => dest.TotalSpent);
        }
    }
}
