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

            // SubCategory mappings
            TypeAdapterConfig<SubCategory, SubCategoryDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(dest => dest.CategoryNameAr, src => src.Category != null ? src.Category.NameAr : string.Empty)
                .Map(dest => dest.CategoryNameEn, src => src.Category != null ? src.Category.NameEn : string.Empty);

            // ProductType mappings
            TypeAdapterConfig<ProductType, ProductTypeDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.SubCategoryId, src => src.SubCategoryId)
                .Map(dest => dest.SubCategoryNameAr, src => src.SubCategory != null ? src.SubCategory.NameAr : string.Empty)
                .Map(dest => dest.SubCategoryNameEn, src => src.SubCategory != null ? src.SubCategory.NameEn : string.Empty)
                .Map(dest => dest.CategoryId, src => src.SubCategory != null ? src.SubCategory.CategoryId : 0)
                .Map(dest => dest.CategoryNameAr, src => src.SubCategory != null && src.SubCategory.Category != null ? src.SubCategory.Category.NameAr : string.Empty)
                .Map(dest => dest.CategoryNameEn, src => src.SubCategory != null && src.SubCategory.Category != null ? src.SubCategory.Category.NameEn : string.Empty);
        }
    }
}

