using Mapster;
using Graduation_domain.Entities;
using Graduation_Application.DTOs.CategoryDTO;

namespace Graduation_Application.Mapper.CategoryMapping
{
    public static class CategoryMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<Category, CategoryDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl ?? string.Empty);

            // Category to CategoryResponseDto (for CRUD operations)
            TypeAdapterConfig<Category, CategoryResponseDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl ?? string.Empty);

            // CreateCategoryDto to Category (for creation)
            TypeAdapterConfig<CreateCategoryDto, Category>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsActive)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt);
        }
    }
}

