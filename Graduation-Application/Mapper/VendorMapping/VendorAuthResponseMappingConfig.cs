using System.Collections.Generic;
using Graduation_Application.DTOs.VendorDTO;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.VendorMapping
{
    public static class VendorAuthResponseMappingConfig
    {
        public static void Response()
        {
            TypeAdapterConfig<
                (ApplicationUser user, string token, IList<string> roles, Workshop workshop),
                VendorAuthResponseDto
            >
                .NewConfig()
                .Map(dest => dest.Token, src => src.token)
                .Map(dest => dest.UserId, src => src.user.Id)
                .Map(dest => dest.FullName, src => src.user.FullName)
                .Map(dest => dest.Email, src => src.user.Email ?? string.Empty)
                .Map(dest => dest.WorkshopId, src => src.workshop != null ? src.workshop.Id : 0)
                .Map(dest => dest.WorkshopNameAr, src => src.workshop != null ? src.workshop.WorkshopNameAr : string.Empty)
                .Map(dest => dest.WorkshopNameEn, src => src.workshop != null ? src.workshop.WorkshopNameEn : string.Empty)
                .Map(dest => dest.IsVerified, src => src.workshop != null ? src.workshop.IsVerified : false)
                .Map(dest => dest.LogoUrl, src => src.workshop != null ? src.workshop.LogoUrl : null);
        }
    }
}
