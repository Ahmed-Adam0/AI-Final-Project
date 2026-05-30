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
                .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : string.Empty)
                .Map(dest => dest.VendorReply, src => src.VendorReply)
                .Map(dest => dest.ReplyCreatedAt, src => src.ReplyCreatedAt);

            TypeAdapterConfig<Review, ReviewDetailsDto>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : string.Empty)
                .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.NameEn : string.Empty)
                .Map(dest => dest.VendorReply, src => src.VendorReply)
                .Map(dest => dest.ReplyCreatedAt, src => src.ReplyCreatedAt);

            TypeAdapterConfig<CreateReviewDto, Review>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.UserId)
                .Ignore(dest => dest.User)
                .Ignore(dest => dest.Product)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.WorkshopId)
                .Ignore(dest => dest.Workshop)
                .Ignore(dest => dest.VendorReply)
                .Ignore(dest => dest.ReplyCreatedAt)
                .Ignore(dest => dest.IsReported)
                .Ignore(dest => dest.ReportReason);
        }
    }
}
