using Mapster;
using Graduation_domain.Entities;
using Graduation_Application.DTOs.FavoriteDTO;
using System.Linq;
using System.Collections.Generic;

namespace Graduation_Application.Mapper.FavoriteMapping
{
    public static class FavoriteMappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<Favorite, FavoriteDto>
                .NewConfig()
                .Map(dest => dest.ProductId, src => src.ProductId)
                .Map(dest => dest.ProductNameAr, src => src.Product != null ? src.Product.NameAr : string.Empty)
                .Map(dest => dest.ProductNameEn, src => src.Product != null ? src.Product.NameEn : string.Empty)
                .Map(dest => dest.Price, src => src.Product != null ? src.Product.BasePrice : 0)
                .AfterMapping((src, dest) =>
                {
                    dest.MainImageUrl = GetMainImageUrl(src.Product?.Images);
                })
                .Map(dest => dest.AddedAt, src => src.CreatedAt);
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
