using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.CartDTO;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartResponseDto> GetCartAsync(string userId)
        {
            var cart = await _context
                .Carts.Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            var cartDto = new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart
                    .Items.Select(ci => new CartItemResponseDto
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.NameEn,
                        Quantity = ci.Quantity,
                        Price = ci.Price,
                        TotalPrice = ci.Price * ci.Quantity,
                    })
                    .ToList(),
            };

            cartDto.TotalPrice = cartDto.Items.Sum(item => item.TotalPrice);
            return cartDto;
        }

        public async Task<CartItemResponseDto> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var cart = await _context
                .Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    Price = product.Price,
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var addedItem =
                existingItem
                ?? (
                    await _context
                        .CartItems.OrderByDescending(ci => ci.Id)
                        .FirstAsync(ci => ci.CartId == cart.Id && ci.ProductId == dto.ProductId)
                );

            var cartItemResponseDto = new CartItemResponseDto
            {
                Id = addedItem.Id,
                ProductId = dto.ProductId,
                ProductName = product.NameEn,
                Quantity = addedItem.Quantity,
                Price = product.Price,
                TotalPrice = product.Price * addedItem.Quantity,
            };

            return cartItemResponseDto;
        }

        public async Task UpdateCartItemAsync(string userId, UpdateCartItemDto dto)
        {
            var cart = await _context
                .Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

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
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string userId, int cartItemId)
        {
            var cart = await _context
                .Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _context
                .Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }
    }
}
