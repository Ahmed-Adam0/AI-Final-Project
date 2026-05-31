using Graduation_Application.DTOs.UserDTO;
using Graduation_domain.Entities;
using Mapster;
using System.Collections.Generic;

namespace Graduation_Application.Mapper.UsersMapping
{
    public static class UserProfileMappingConfig
    {
        public static void RegisterMappings()
        {
            // Address → AddressDto
            TypeAdapterConfig<Address, AddressDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.City, src => src.City)
                .Map(dest => dest.Area, src => src.Area)
                .Map(dest => dest.Street, src => src.Street)
                .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
                .Map(dest => dest.Notes, src => src.Notes);

            // AddressDto → Address
            TypeAdapterConfig<AddressDto, Address>
                .NewConfig()
                .Map(dest => dest.City, src => src.City)
                .Map(dest => dest.Area, src => src.Area)
                .Map(dest => dest.Street, src => src.Street)
                .Map(dest => dest.BuildingNumber, src => src.BuildingNumber)
                .Map(dest => dest.Notes, src => src.Notes)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.UserId)
                .Ignore(dest => dest.User);

            // ApplicationUser → UserProfileDto
            TypeAdapterConfig<ApplicationUser, UserProfileDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.ProfileImage, src => src.ProfileImage)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.Addresses, src => src.Addresses);

            // UpdateProfileDto → ApplicationUser
            TypeAdapterConfig<UpdateProfileDto, ApplicationUser>
                .NewConfig()
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.PreferredLanguage, src => src.PreferredLanguage)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.NormalizedEmail,
                    src => src.Email == null ? null : src.Email.ToUpper())
                //.Map(dest => dest.ProfileImage, src => src.ProfileImage)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.NormalizedUserName,
                    src => src.UserName == null ? null : src.UserName.ToUpper())
                .Map(dest => dest.ConcurrencyStamp,
                    src => System.Guid.NewGuid().ToString())
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.PasswordHash)
                .Ignore(dest => dest.SecurityStamp)
                .Ignore(dest => dest.Addresses)
                .Ignore(dest => dest.Carts)
                .Ignore(dest => dest.Favorites)
                .Ignore(dest => dest.Orders)
                .Ignore(dest => dest.Workshops)
                .Ignore(dest => dest.Reviews);
        }
    }
}