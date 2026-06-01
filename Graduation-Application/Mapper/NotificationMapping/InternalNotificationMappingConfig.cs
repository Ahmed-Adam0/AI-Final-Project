using Graduation_Application.DTOs.NotificationDTO;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.NotificationMapping
{
    public static class InternalNotificationMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<InternalNotification, InternalNotificationDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.IsRead, src => src.IsRead)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Ignore(dest => dest.Title)
                .Ignore(dest => dest.Message);
        }
    }
}
