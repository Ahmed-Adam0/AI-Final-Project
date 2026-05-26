using Graduation_Application.DTOs.ReviewDTO;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.ReviewMapping
{
    public static class ReviewMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<Review, ReviewDto>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : string.Empty);

            TypeAdapterConfig<Review, ReviewDetailsDto>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : string.Empty)
                .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.NameEn : string.Empty);

            TypeAdapterConfig<CreateReviewDto, Review>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.UserId)
                .Ignore(dest => dest.User)
                .Ignore(dest => dest.Product)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt);
        }
    }
}
