using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.CartDTO;
using Graduation_Application.DTOs.ProductDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services
{
    public class CartService : ICartService
    {
        private const string ProductImagesBaseUrl = "http://home-ai.runasp.net";
        private readonly IGenaricRepositories<Cart> _cartRepository;
        private readonly IGenaricRepositories<CartItem> _cartItemRepository;
        private readonly IGenaricRepositories<ProductVariant> _variantRepository;

        public CartService(
            IGenaricRepositories<Cart> cartRepository,
            IGenaricRepositories<CartItem> cartItemRepository,
            IGenaricRepositories<ProductVariant> variantRepository
        )
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _variantRepository = variantRepository;
        }

        public async Task<CartResponseDto> GetCartAsync(string userId)
        {
            var cart = await _cartRepository
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Listing)
                            .ThenInclude(l => l.Product)
                                .ThenInclude(p => p.Images)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Listing)
                            .ThenInclude(l => l.Workshop)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.VariantAttributeValues)
                            .ThenInclude(vav => vav.AttributeValue)
                                .ThenInclude(av => av.Attribute)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                await _cartRepository.AddAsync(cart);
                await _cartRepository.SaveChangesAsync();
            }

            var cartDto = new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart
                    .Items.Select(ci => new CartItemResponseDto
                    {
                        Id = ci.Id,
                        ProductVariantId = ci.ProductVariantId,
                        ProductId = ci.ProductVariant?.Listing?.ProductId ?? 0,
                        ProductNameEn = ci.ProductVariant?.Listing?.Product?.NameEn ?? string.Empty,
                        ProductNameAr = ci.ProductVariant?.Listing?.Product?.NameAr ?? string.Empty,
                        VendorNameEn = ci.ProductVariant?.Listing?.Workshop?.WorkshopNameEn ?? string.Empty,
                        VendorNameAr = ci.ProductVariant?.Listing?.Workshop?.WorkshopNameAr ?? string.Empty,
                        Quantity = ci.Quantity,
                        CachedPrice = ci.CachedPrice,
                        LivePrice = ci.ProductVariant?.CurrentPrice ?? 0m,
                        VariantImageUrl = ci.ProductVariant?.VariantImageUrl,
                        SelectedAttributes = ci.ProductVariant?.VariantAttributeValues?.Select(vav => new SelectedAttributeDto
                        {
                            AttributeNameEn = vav.AttributeValue?.Attribute?.NameEn ?? string.Empty,
                            AttributeNameAr = vav.AttributeValue?.Attribute?.NameAr ?? string.Empty,
                            ValueEn = vav.AttributeValue?.ValueEn ?? string.Empty,
                            ValueAr = vav.AttributeValue?.ValueAr ?? string.Empty
                        }).ToList() ?? new List<SelectedAttributeDto>(),
                        ProductImages = ci.ProductVariant?.Listing?.Product?.Images != null
                            ? ci.ProductVariant.Listing.Product.Images.Select(i => NormalizeProductImageUrl(i.ImageUrl)).ToList()
                            : new List<string>()
                    })
                    .ToList(),
            };

            cartDto.TotalPrice = cartDto.Items.Sum(item => item.TotalPrice);
            return cartDto;
        }

        public async Task<CartItemResponseDto> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var variant = await _variantRepository
                .Where(v => v.Id == dto.ProductVariantId)
                .Include(v => v.Listing)
                    .ThenInclude(l => l.Product)
                        .ThenInclude(p => p.Images)
                .Include(v => v.Listing)
                    .ThenInclude(l => l.Workshop)
                .Include(v => v.VariantAttributeValues)
                    .ThenInclude(vav => vav.AttributeValue)
                        .ThenInclude(av => av.Attribute)
                .FirstOrDefaultAsync();

            if (variant == null)
            {
                throw new Exception("Product Variant not found");
            }

            var cart = await _cartRepository
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                await _cartRepository.AddAsync(cart);
                await _cartRepository.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductVariantId == dto.ProductVariantId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                _cartItemRepository.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity,
                    CachedPrice = variant.CurrentPrice,
                };
                await _cartItemRepository.AddAsync(cartItem);
            }

            await _cartItemRepository.SaveChangesAsync();

            var addedItem = existingItem ?? await _cartItemRepository
                .Where(ci => ci.CartId == cart.Id && ci.ProductVariantId == dto.ProductVariantId)
                .FirstOrDefaultAsync();

            var cartItemResponseDto = new CartItemResponseDto
            {
                Id = addedItem.Id,
                ProductVariantId = addedItem.ProductVariantId,
                ProductId = variant.Listing?.ProductId ?? 0,
                ProductNameEn = variant.Listing?.Product?.NameEn ?? string.Empty,
                ProductNameAr = variant.Listing?.Product?.NameAr ?? string.Empty,
                VendorNameEn = variant.Listing?.Workshop?.WorkshopNameEn ?? string.Empty,
                VendorNameAr = variant.Listing?.Workshop?.WorkshopNameAr ?? string.Empty,
                Quantity = addedItem.Quantity,
                CachedPrice = addedItem.CachedPrice,
                LivePrice = variant.CurrentPrice,
                VariantImageUrl = variant.VariantImageUrl,
                SelectedAttributes = variant.VariantAttributeValues?.Select(vav => new SelectedAttributeDto
                {
                    AttributeNameEn = vav.AttributeValue?.Attribute?.NameEn ?? string.Empty,
                    AttributeNameAr = vav.AttributeValue?.Attribute?.NameAr ?? string.Empty,
                    ValueEn = vav.AttributeValue?.ValueEn ?? string.Empty,
                    ValueAr = vav.AttributeValue?.ValueAr ?? string.Empty
                }).ToList() ?? new List<SelectedAttributeDto>(),
                ProductImages = variant.Listing?.Product?.Images != null
                    ? variant.Listing.Product.Images.Select(i => NormalizeProductImageUrl(i.ImageUrl)).ToList()
                    : new List<string>()
            };

            return cartItemResponseDto;
        }

        public async Task UpdateCartItemAsync(string userId, UpdateCartItemDto dto)
        {
            var cart = await _cartRepository
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == dto.CartItemId);
            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }

            if (dto.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0");
            }

            cartItem.Quantity = dto.Quantity;
            _cartItemRepository.Update(cartItem);
            await _cartItemRepository.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string userId, int cartItemId)
        {
            var cart = await _cartRepository
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }

            _cartItemRepository.Delete(cartItem);
            await _cartItemRepository.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _cartRepository
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            _cartItemRepository.DeleteRange(cart.Items);
            await _cartItemRepository.SaveChangesAsync();
        }

        private static string NormalizeProductImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return imageUrl;
            }

            if (imageUrl.StartsWith("/images/products/", StringComparison.OrdinalIgnoreCase))
            {
                return $"{ProductImagesBaseUrl}{imageUrl}";
            }

            return imageUrl;
        }
    }
}
