using Graduation_Application.DTOs.InspirationDTOs;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.InspirationMapping
{
    public static class InspirationMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<OrderReviewImage, InspirationItemDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id.ToString())
                .Map(dest => dest.BeforeImageUrl, src => src.BeforeImageUrl)
                .Map(dest => dest.AfterImageUrl, src => src.AfterImageUrl);
        }
    }
}
