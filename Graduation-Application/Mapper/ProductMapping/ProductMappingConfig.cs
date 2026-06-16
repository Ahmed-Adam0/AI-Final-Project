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
                // Category Hierarchy Mapping
                .Map(dest => dest.ProductTypeId, src => src.ProductTypeId)
                .Map(dest => dest.ProductTypeNameAr, src => src.ProductType != null ? src.ProductType.NameAr : string.Empty)
                .Map(dest => dest.ProductTypeNameEn, src => src.ProductType != null ? src.ProductType.NameEn : string.Empty)
                .Map(dest => dest.SubCategoryId, src => src.ProductType != null ? src.ProductType.SubCategoryId : 0)
                .Map(dest => dest.SubCategoryNameAr, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.NameAr : string.Empty)
                .Map(dest => dest.SubCategoryNameEn, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.NameEn : string.Empty)
                .Map(dest => dest.CategoryId, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.CategoryId : 0)
                .Map(dest => dest.CategoryNameAr, src => src.ProductType != null && src.ProductType.SubCategory != null && src.ProductType.SubCategory.Category != null ? src.ProductType.SubCategory.Category.NameAr : string.Empty)
                .Map(dest => dest.CategoryNameEn, src => src.ProductType != null && src.ProductType.SubCategory != null && src.ProductType.SubCategory.Category != null ? src.ProductType.SubCategory.Category.NameEn : string.Empty)
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
                // Category Hierarchy Mapping
                .Map(dest => dest.ProductTypeId, src => src.ProductTypeId)
                .Map(dest => dest.ProductTypeNameAr, src => src.ProductType != null ? src.ProductType.NameAr : string.Empty)
                .Map(dest => dest.ProductTypeNameEn, src => src.ProductType != null ? src.ProductType.NameEn : string.Empty)
                .Map(dest => dest.SubCategoryId, src => src.ProductType != null ? src.ProductType.SubCategoryId : 0)
                .Map(dest => dest.SubCategoryNameAr, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.NameAr : string.Empty)
                .Map(dest => dest.SubCategoryNameEn, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.NameEn : string.Empty)
                .Map(dest => dest.CategoryId, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.CategoryId : 0)
                .Map(dest => dest.CategoryNameAr, src => src.ProductType != null && src.ProductType.SubCategory != null && src.ProductType.SubCategory.Category != null ? src.ProductType.SubCategory.Category.NameAr : string.Empty)
                .Map(dest => dest.CategoryNameEn, src => src.ProductType != null && src.ProductType.SubCategory != null && src.ProductType.SubCategory.Category != null ? src.ProductType.SubCategory.Category.NameEn : string.Empty)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.Images,
                    src => src.Images != null
                        ? src.Images.Adapt<List<ProductImageDto>>()
                        : new List<ProductImageDto>())
                .Map(dest => dest.MaterialGroups, src => src.MaterialOptions != null
                    ? src.MaterialOptions
                        .Where(mo => mo.VendorMaterialOption != null && mo.VendorMaterialOption.Group != null)
                        .GroupBy(mo => mo.VendorMaterialOption.Group.Id)
                        .Select(g => new ProductMaterialGroupResponseDto
                        {
                            Id = g.Key,
                            NameAr = g.First().VendorMaterialOption.Group.NameAr,
                            NameEn = g.First().VendorMaterialOption.Group.NameEn,
                            Options = g.Select(mo => new ProductMaterialOptionDetailsDto
                            {
                                Id = mo.VendorMaterialOptionId,
                                VendorMaterialGroupId = mo.VendorMaterialOption.VendorMaterialGroupId,
                                ValueAr = mo.VendorMaterialOption.ValueAr,
                                ValueEn = mo.VendorMaterialOption.ValueEn,
                                PriceOption = mo.PriceOption
                            }).ToList()
                        }).ToList()
                    : new List<ProductMaterialGroupResponseDto>())
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
                // Category Hierarchy Mapping
                .Map(dest => dest.ProductTypeId, src => src.ProductTypeId)
                .Map(dest => dest.ProductTypeNameAr, src => src.ProductType != null ? src.ProductType.NameAr : string.Empty)
                .Map(dest => dest.ProductTypeNameEn, src => src.ProductType != null ? src.ProductType.NameEn : string.Empty)
                .Map(dest => dest.SubCategoryId, src => src.ProductType != null ? src.ProductType.SubCategoryId : 0)
                .Map(dest => dest.SubCategoryNameAr, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.NameAr : string.Empty)
                .Map(dest => dest.SubCategoryNameEn, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.NameEn : string.Empty)
                .Map(dest => dest.CategoryId, src => src.ProductType != null && src.ProductType.SubCategory != null ? src.ProductType.SubCategory.CategoryId : 0)
                .Map(dest => dest.CategoryNameAr, src => src.ProductType != null && src.ProductType.SubCategory != null && src.ProductType.SubCategory.Category != null ? src.ProductType.SubCategory.Category.NameAr : string.Empty)
                .Map(dest => dest.CategoryNameEn, src => src.ProductType != null && src.ProductType.SubCategory != null && src.ProductType.SubCategory.Category != null ? src.ProductType.SubCategory.Category.NameEn : string.Empty)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
                .Map(dest => dest.DescriptionEn, src => src.DescriptionEn)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.MaterialGroups, src => src.MaterialOptions != null
                    ? src.MaterialOptions
                        .Where(mo => mo.VendorMaterialOption != null && mo.VendorMaterialOption.Group != null)
                        .GroupBy(mo => mo.VendorMaterialOption.Group.Id)
                        .Select(g => new ProductMaterialGroupResponseDto
                        {
                            Id = g.Key,
                            NameAr = g.First().VendorMaterialOption.Group.NameAr,
                            NameEn = g.First().VendorMaterialOption.Group.NameEn,
                            Options = g.Select(mo => new ProductMaterialOptionDetailsDto
                            {
                                Id = mo.VendorMaterialOptionId,
                                VendorMaterialGroupId = mo.VendorMaterialOption.VendorMaterialGroupId,
                                ValueAr = mo.VendorMaterialOption.ValueAr,
                                ValueEn = mo.VendorMaterialOption.ValueEn,
                                PriceOption = mo.PriceOption
                            }).ToList()
                        }).ToList()
                    : new List<ProductMaterialGroupResponseDto>());

            // ProductMaterialOption to ProductMaterialOptionResponseDto
            TypeAdapterConfig<ProductMaterialOption, ProductMaterialOptionResponseDto>
                .NewConfig()
                .Map(dest => dest.VendorMaterialOptionId, src => src.VendorMaterialOptionId)
                .Map(dest => dest.PriceOption, src => src.PriceOption)
                .Map(dest => dest.ValueAr, src => src.VendorMaterialOption != null ? src.VendorMaterialOption.ValueAr : string.Empty)
                .Map(dest => dest.ValueEn, src => src.VendorMaterialOption != null ? src.VendorMaterialOption.ValueEn : string.Empty);

            // CreateProductDto to Product (for creation — no price or vendor here)
            TypeAdapterConfig<CreateProductDto, Product>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.IsActive)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Images)
                .Ignore(dest => dest.ProductType)
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
