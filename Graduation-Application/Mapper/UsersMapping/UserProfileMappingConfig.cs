using Graduation_Application.DTOs.UserDTO;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.UsersMapping
{
    public static class UserProfileMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<ApplicationUser, UserProfileDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.ProfileImage, src => src.ProfileImage)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.UserName, src => src.UserName);
        }
    }
}
