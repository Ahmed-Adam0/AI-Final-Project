using Graduation_Application.DTOs.Admin.AdminProfileDTO;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.Admin
{
    public static class AdminProfileMappingConfig
    {
        public static void RegisterMappings()
        {
            // ApplicationUser to AdminProfileDto
            TypeAdapterConfig<ApplicationUser, AdminProfileDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.ProfileImage, src => src.ProfileImage)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage);
        }
    }
}
