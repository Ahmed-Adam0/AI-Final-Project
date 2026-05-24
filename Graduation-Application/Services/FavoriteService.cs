using Graduation_Application.DTOs.FavoriteDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Graduation_Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IGenaricRepositories<Favorite> _favoriteRepository;
        private readonly IGenaricRepositories<Product> _productRepository;

        public FavoriteService(
            IGenaricRepositories<Favorite> favoriteRepository,
            IGenaricRepositories<Product> productRepository)
        {
            _favoriteRepository = favoriteRepository;
            _productRepository = productRepository;
        }

        public async Task<string> AddToFavoritesAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            if (productId <= 0)
                throw new ArgumentException("Invalid product ID", nameof(productId));

            var productExists = await _productRepository.AnyAsync(p => p.Id == productId && p.IsActive);
            if (!productExists)
                throw new InvalidOperationException("Product not found or inactive");

            var favoriteExists = await _favoriteRepository.AnyAsync(f => f.UserId == userId && f.ProductId == productId && f.IsActive);
            if (favoriteExists)
                throw new InvalidOperationException("Product is already in favorites");

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _favoriteRepository.AddAsync(favorite);
            await _favoriteRepository.SaveChangesAsync();

            return "Product added to favorites successfully";
        }

        public async Task<string> RemoveFromFavoritesAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            if (productId <= 0)
                throw new ArgumentException("Invalid product ID", nameof(productId));

            var favorite = await _favoriteRepository.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId && f.IsActive);
            if (favorite == null)
                throw new InvalidOperationException("Favorite not found");

            favorite.IsActive = false;
            favorite.UpdatedAt = DateTime.UtcNow;
            _favoriteRepository.Update(favorite);
            await _favoriteRepository.SaveChangesAsync();

            return "Product removed from favorites successfully";
        }

        public async Task<List<FavoriteDto>> GetUserFavoritesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            var favorites = await _favoriteRepository
                .GetAllAsNoTracking()
                .Include(f => f.Product)
                .ThenInclude(p => p.Images)
                .Where(f => f.UserId == userId && f.IsActive && f.Product.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return favorites.Adapt<List<FavoriteDto>>();
        }
    }
}
