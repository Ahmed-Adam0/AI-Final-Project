using System.Threading.Tasks;
using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
