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
                // MinPrice: lowest CurrentPrice across all active vendor variants
                .Map(dest => dest.Price,
                    src => src.VendorListings != null && src.VendorListings.Count > 0
                        ? src.VendorListings
                            .SelectMany(l => l.Variants ?? Enumerable.Empty<ProductVariant>())
                            .Select(v => (decimal?)v.CurrentPrice)
                            .Min() ?? 0m
                        : 0m)
                .Map(dest => dest.CategoryId, src => src.CategoryId)
                .Map(
                    dest => dest.CategoryNameAr,
                    src => src.Category != null ? src.Category.NameAr : string.Empty
                )
                .Map(
                    dest => dest.CategoryNameEn,
                    src => src.Category != null ? src.Category.NameEn : string.Empty
                )
                // WorkshopId: first vendor's workshop (backward compat for list view)
                .Map(dest => dest.WorkshopId,
                    src => src.VendorListings != null && src.VendorListings.Count > 0
                        ? src.VendorListings[0].WorkshopId
                        : 0)
                .Map(
                    dest => dest.WorkshopNameAr,
                    src => src.VendorListings != null && src.VendorListings.Count > 0 && src.VendorListings[0].Workshop != null
                        ? src.VendorListings[0].Workshop.WorkshopNameAr
                        : string.Empty
                )
                .Map(
                    dest => dest.WorkshopNameEn,
                    src => src.VendorListings != null && src.VendorListings.Count > 0 && src.VendorListings[0].Workshop != null
                        ? src.VendorListings[0].Workshop.WorkshopNameEn
                        : string.Empty
                )
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
                // Attributes and VendorListings are mapped manually in the service layer
                // because they require complex nested projections
                .Ignore(dest => dest.Attributes)
                .Ignore(dest => dest.VendorListings);

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
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.VendorCount,
                    src => src.VendorListings != null ? src.VendorListings.Count : 0)
                .Map(dest => dest.MinPrice,
                    src => src.VendorListings != null && src.VendorListings.Count > 0
                        ? src.VendorListings
                            .SelectMany(l => l.Variants ?? Enumerable.Empty<ProductVariant>())
                            .Select(v => (decimal?)v.CurrentPrice)
                            .Min()
                        : null);

            // CreateProductDto to Product (for creation — no price or vendor here)
            TypeAdapterConfig<CreateProductDto, Product>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsActive)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Images)
                .Ignore(dest => dest.Category)
                .Ignore(dest => dest.VendorListings)
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
