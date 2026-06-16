using System.Collections.Generic;
using System.Linq;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Mapper.ProductMapping
{
    public static class ProductMappingConfig
    {
        public static void RegisterMappings()
        {
            // Product to ProductDto (catalog list view)
            TypeAdapterConfig<Product, ProductDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                // BasePrice is mapped automatically because property names match
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(
                    dest => dest.CategoryNameAr,
                    src => src.Category != null ? src.Category.NameAr : string.Empty
                )
                .Map(
                    dest => dest.CategoryNameEn,
                    src => src.Category != null ? src.Category.NameEn : string.Empty
                )
                // Workshop mapping
                .Map(dest => dest.WorkshopId, src => src.WorkshopId)
                .Map(dest => dest.WorkshopNameAr, src => src.Workshop != null ? src.Workshop.WorkshopNameAr : string.Empty)
                .Map(dest => dest.WorkshopNameEn, src => src.Workshop != null ? src.Workshop.WorkshopNameEn : string.Empty)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .AfterMapping((src, dest) =>
                {
                    dest.MainImageUrl = GetMainImageUrl(src.Images);
                });

            // Product to ProductDetailsDto (full detail view with vendor listings)
            TypeAdapterConfig<Product, ProductDetailsDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(
                    dest => dest.CategoryNameAr,
                    src => src.Category != null ? src.Category.NameAr : string.Empty
                )
                .Map(
                    dest => dest.CategoryNameEn,
                    src => src.Category != null ? src.Category.NameEn : string.Empty
                )
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.Images,
                    src => src.Images != null
                        ? src.Images.Adapt<List<ProductImageDto>>()
                        : new List<ProductImageDto>())
                // Attributes are mapped manually in the service layer
                .Ignore(dest => dest.Attributes);

            // ProductImage to ProductImageDto
            TypeAdapterConfig<ProductImage, ProductImageDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl)
                .Map(dest => dest.IsPrimary, src => src.IsPrimary);

            // Product to ProductResponseDto (for CRUD operations)
            TypeAdapterConfig<Product, ProductResponseDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .Map(dest => dest.IsActive, src => src.IsActive);

            // CreateProductDto to Product (for creation — no price or vendor here)
            TypeAdapterConfig<CreateProductDto, Product>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsActive)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Images)
                .Ignore(dest => dest.Category)
                .Ignore(dest => dest.Attributes);
        }

        private static string GetMainImageUrl(List<ProductImage> images)
        {
            if (images == null || images.Count == 0)
                return string.Empty;

            var primaryImage = images.FirstOrDefault(img => img.IsPrimary);
            if (primaryImage != null)
                return primaryImage.ImageUrl;

            return images.FirstOrDefault()?.ImageUrl ?? string.Empty;
        }
    }
}
