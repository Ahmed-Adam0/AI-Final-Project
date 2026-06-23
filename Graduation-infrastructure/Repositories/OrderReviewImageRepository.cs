using Graduation_Application.IRepositories;
using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;

namespace Graduation_infrastructure.Repositories
{
    public class OrderReviewImageRepository : GenaricRepositories<OrderReviewImage>, IOrderReviewImageRepository
    {
        public OrderReviewImageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
