using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_domain.Entities;

namespace Graduation_Application.IRepositories
{
    public interface IInternalNotificationRepository
    {
        Task AddAsync(InternalNotification notification);
        Task<List<InternalNotification>> GetByUserIdAsync(string userId, int page, int pageSize);
        Task<int> GetTotalCountAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<InternalNotification?> GetByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
