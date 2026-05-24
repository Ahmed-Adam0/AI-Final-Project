using Graduation_Application.DTOs.FavoriteDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IFavoriteService
    {
        Task<string> AddToFavoritesAsync(string userId, int productId);
        Task<string> RemoveFromFavoritesAsync(string userId, int productId);
        Task<List<FavoriteDto>> GetUserFavoritesAsync(string userId);
    }
}
