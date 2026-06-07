using Graduation_Application.DTOs.Admin.AuthAdmin;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.Admin
{
    public static class AdminAuthMappingConfig
    {
        public static void RegisterMappings()
        {
            // ApplicationUser to AdminAuthResultDto
            TypeAdapterConfig<ApplicationUser, AdminAuthResultDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage)
                .Ignore(dest => dest.Role); // Role is set manually in service
        }
    }
}
