using Mapster;
using Graduation_Application.DTOs.VendorDTO;
using Graduation_domain.Entities;

namespace Graduation_Application.Mapper.VendorMapping
{
    public static class VendorProfileMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<(ApplicationUser user, Workshop workshop), VendorProfileDto>
                .NewConfig()
                .Map(dest => dest.UserId, src => src.user.Id)
                .Map(dest => dest.FullName, src => src.user.FullName)
                .Map(dest => dest.Email, src => src.user.Email ?? string.Empty)
                .Map(dest => dest.PhoneNumber, src => src.user.PhoneNumber)
                .Map(dest => dest.PreferredLanguage, src => src.user.PreferredLanguage)
                .Map(dest => dest.ProfileImage, src => src.user.ProfileImage)
                .Map(dest => dest.WorkshopId, src => src.workshop != null ? src.workshop.Id : 0)
                .Map(dest => dest.WorkshopNameAr, src => src.workshop != null ? src.workshop.WorkshopNameAr : string.Empty)
                .Map(dest => dest.WorkshopNameEn, src => src.workshop != null ? src.workshop.WorkshopNameEn : string.Empty)
                .Map(dest => dest.DescriptionAr, src => src.workshop != null ? src.workshop.DescriptionAr : string.Empty)
                .Map(dest => dest.DescriptionEn, src => src.workshop != null ? src.workshop.DescriptionEn : string.Empty)
                .Map(dest => dest.LogoUrl, src => src.workshop != null ? src.workshop.LogoUrl : null)
                .Map(dest => dest.Rating, src => src.workshop != null ? src.workshop.Rating : null)
                .Map(dest => dest.IsVerified, src => src.workshop != null ? src.workshop.IsVerified : false);
        }
    }
}
