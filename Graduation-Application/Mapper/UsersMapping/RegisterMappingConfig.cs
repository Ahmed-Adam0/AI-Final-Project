using Mapster;
using Graduation_domain.Entities;
using Graduation_Application.DTOs.UserDTO;

namespace Graduation_Application.Mapper.UsersMapping
{
    public static class RegisterMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<RegisterDto, ApplicationUser>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.Email)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage)
                .Ignore(dest => dest.PasswordHash);
        }
    }
}
