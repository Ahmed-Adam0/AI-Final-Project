using Mapster;
using Microsoft.AspNetCore.Identity;
using Graduation_Application.DTOs.RolesDTO;

namespace Graduation_Application.Mapper
{
    public static class RoleMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<IdentityRole, RoleResponseDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name ?? string.Empty);
        }
    }
}
