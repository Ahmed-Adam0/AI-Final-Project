using Graduation_Application.DTOs.UserDTO;
using Graduation_domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graduation_Application.Mapper.UsersMapping
{
    public static class AuthResponseMappingConfig
    {
        public static void Response()
        {
            TypeAdapterConfig<(ApplicationUser user, string token, IList<string> roles), AuthResponseDto>
                .NewConfig()
                //.Map(dest => dest.Email, src => src.user.Email ?? string.Empty)
                //.Map(dest => dest.FullName, src => src.user.FullName)
                .Map(dest => dest.Token, src => src.token);
                //.Map(dest => dest.Roles, src => src.roles.ToList());
        }
    }
}
