using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.Repositories
{
    public class InternalNotificationRepository : IInternalNotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public InternalNotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InternalNotification notification)
        {
            await _context.InternalNotifications.AddAsync(notification);
        }

        public async Task<List<InternalNotification>> GetByUserIdAsync(string userId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return await _context.InternalNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string userId)
        {
            return await _context.InternalNotifications
                .Where(n => n.UserId == userId)
                .CountAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.InternalNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<InternalNotification?> GetByIdAsync(int id)
        {
            return await _context.InternalNotifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
