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
        }
    }
}
