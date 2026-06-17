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
        private readonly IGenaricRepositories<Product> _productRepository;
        private readonly IGenaricRepositories<ProductMaterialOption> _productMaterialOptionRepository;

        public CartService(
            IGenaricRepositories<Cart> cartRepository,
            IGenaricRepositories<CartItem> cartItemRepository,
            IGenaricRepositories<Product> productRepository,
            IGenaricRepositories<ProductMaterialOption> productMaterialOptionRepository
        )
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _productMaterialOptionRepository = productMaterialOptionRepository;
        }

        public async Task<CartResponseDto> GetCartAsync(string userId)
        {
            var cart = await _cartRepository
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Workshop)
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
                Items = new List<CartItemResponseDto>()
            };

            foreach (var ci in cart.Items)
            {
                var optionIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(ci.SelectedOptionsJson ?? "[]");
                decimal delta = 0;
                if (optionIds != null && optionIds.Any())
                {
                    delta = await _productMaterialOptionRepository
                        .Where(o => o.ProductId == ci.ProductId && optionIds.Contains(o.VendorMaterialOptionId))
                        .SumAsync(o => o.PriceOption);
                }

                cartDto.Items.Add(new CartItemResponseDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductNameEn = ci.Product?.NameEn ?? string.Empty,
                    ProductNameAr = ci.Product?.NameAr ?? string.Empty,
                    VendorNameEn = ci.Product?.Workshop?.WorkshopNameEn ?? string.Empty,
                    VendorNameAr = ci.Product?.Workshop?.WorkshopNameAr ?? string.Empty,
                    Quantity = ci.Quantity,
                    CachedPrice = ci.CachedPrice,
                    LivePrice = (ci.Product?.BasePrice ?? 0m) + delta,
                    SelectedAttributes = new List<SelectedAttributeDto>(),
                    ProductImages = ci.Product?.Images != null
                        ? ci.Product.Images.Select(i => NormalizeProductImageUrl(i.ImageUrl)).ToList()
                        : new List<string>()
                });
            }

            cartDto.TotalPrice = cartDto.Items.Sum(item => item.TotalPrice);
            return cartDto;
        }

        public async Task<CartItemResponseDto> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var product = await _productRepository
                .Where(p => p.Id == dto.ProductId)
                .Include(p => p.Images)
                .Include(p => p.Workshop)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new Exception("Product not found");
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

            decimal totalDelta = 0;
            if (dto.SelectedOptionIds != null && dto.SelectedOptionIds.Any())
            {
                totalDelta = await _productMaterialOptionRepository
                    .Where(o => o.ProductId == dto.ProductId && dto.SelectedOptionIds.Contains(o.VendorMaterialOptionId))
                    .SumAsync(o => o.PriceOption);
            }

            var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == dto.ProductId && ci.SelectedOptionsJson == System.Text.Json.JsonSerializer.Serialize(dto.SelectedOptionIds));
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
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    CachedPrice = product.BasePrice + totalDelta,
                    SelectedOptionsJson = System.Text.Json.JsonSerializer.Serialize(dto.SelectedOptionIds ?? new List<int>())
                };
                await _cartItemRepository.AddAsync(cartItem);
            }

            await _cartItemRepository.SaveChangesAsync();

            var addedItem = existingItem ?? await _cartItemRepository
                .Where(ci => ci.CartId == cart.Id && ci.ProductId == dto.ProductId)
                .FirstOrDefaultAsync();

            var cartItemResponseDto = new CartItemResponseDto
            {
                Id = addedItem.Id,
                ProductId = addedItem.ProductId,
                ProductNameEn = product.NameEn,
                ProductNameAr = product.NameAr,
                VendorNameEn = product.Workshop?.WorkshopNameEn ?? string.Empty,
                VendorNameAr = product.Workshop?.WorkshopNameAr ?? string.Empty,
                Quantity = addedItem.Quantity,
                CachedPrice = addedItem.CachedPrice,
                LivePrice = product.BasePrice + totalDelta,
                SelectedAttributes = new List<SelectedAttributeDto>(),
                ProductImages = product.Images != null
                    ? product.Images.Select(i => NormalizeProductImageUrl(i.ImageUrl)).ToList()
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
            if (string.IsNullOrWhiteSpace(imageUrl)) return imageUrl;
            if (imageUrl.StartsWith("/images/products/", StringComparison.OrdinalIgnoreCase)) return $"{ProductImagesBaseUrl}{imageUrl}";
            return imageUrl;
        }
    }
}
