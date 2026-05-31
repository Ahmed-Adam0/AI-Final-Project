using Mapster;
using Graduation_domain.Entities;
using Graduation_Application.DTOs.ProductDTO;
using System.Linq;
using System.Collections.Generic;

namespace Graduation_Application.Mapper.ProductMapping
{
    public static class ProductMappingConfig
    {
        public static void RegisterMappings()
        {
            // Product to ProductDto
            TypeAdapterConfig<Product, ProductDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(dest => dest.CategoryNameAr, src => src.Category != null ? src.Category.NameAr : string.Empty)
                .Map(dest => dest.CategoryNameEn, src => src.Category != null ? src.Category.NameEn : string.Empty)
                .Map(dest => dest.WorkshopId, src => src.WorkshopId)
                .Map(dest => dest.WorkshopNameAr, src => src.Workshop != null ? src.Workshop.WorkshopNameAr : string.Empty)
                .Map(dest => dest.WorkshopNameEn, src => src.Workshop != null ? src.Workshop.WorkshopNameEn : string.Empty)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .AfterMapping((src, dest) => 
                {
                    dest.MainImageUrl = GetMainImageUrl(src.Images);
                });

            // Product to ProductDetailsDto
            TypeAdapterConfig<Product, ProductDetailsDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(dest => dest.CategoryNameAr, src => src.Category != null ? src.Category.NameAr : string.Empty)
                .Map(dest => dest.CategoryNameEn, src => src.Category != null ? src.Category.NameEn : string.Empty)
                .Map(dest => dest.WorkshopId, src => src.WorkshopId)
                .Map(dest => dest.WorkshopNameAr, src => src.Workshop != null ? src.Workshop.WorkshopNameAr : string.Empty)
                .Map(dest => dest.WorkshopNameEn, src => src.Workshop != null ? src.Workshop.WorkshopNameEn : string.Empty)
                .Map(dest => dest.WorkshopDescriptionAr, src => src.Workshop != null ? src.Workshop.DescriptionAr : string.Empty)
                .Map(dest => dest.WorkshopDescriptionEn, src => src.Workshop != null ? src.Workshop.DescriptionEn : string.Empty)
                .Map(dest => dest.WorkshopAddress, src => src.Workshop != null && src.Workshop.WorkshopAddress != null ? (src.Workshop.WorkshopAddress.Street ?? string.Empty) : string.Empty)
                .Map(dest => dest.WorkshopLogoUrl, src => src.Workshop != null ? src.Workshop.LogoUrl ?? string.Empty : string.Empty)
                .Map(dest => dest.WorkshopRating, src => src.Workshop != null ? src.Workshop.Rating : null)
                .Map(dest => dest.WorkshopIsVerified, src => src.Workshop != null && src.Workshop.IsVerified)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.Images, src => src.Images != null ? src.Images.Adapt<List<ProductImageDto>>() : new List<ProductImageDto>());

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
                .Map(dest => dest.WorkshopId, src => src.WorkshopId)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .Map(dest => dest.Price, src => src.Price);

            // CreateProductDto to Product (for creation)
            TypeAdapterConfig<CreateProductDto, Product>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsActive)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Images)
                .Ignore(dest => dest.Category)
                .Ignore(dest => dest.Workshop);
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
