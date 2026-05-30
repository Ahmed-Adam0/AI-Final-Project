using Mapster;
using Google.Apis.Auth;
using Graduation_domain.Entities;

namespace Graduation_Application.Mapper.UsersMapping
{
    public static class GooglePayloadMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<GoogleJsonWebSignature.Payload, ApplicationUser>
                .NewConfig()
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.UserName, src => src.Email)
                .Map(dest => dest.FullName, src => src.Name ?? src.Email)
                .Map(dest => dest.ProfileImage, src => src.Picture ?? string.Empty)
                .Map(dest => dest.GoogleId, src => src.Subject)
                .Map(dest => dest.EmailConfirmed, src => true)
                .Ignore(dest => dest.PasswordHash);
        }
    }
}
