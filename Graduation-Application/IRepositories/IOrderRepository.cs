using System.Threading.Tasks;
using Graduation_domain.Entities;

namespace Graduation_Application.IRepositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
