using System;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.Inspirations;
using Graduation_Application.DTOs.Common;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services.Admin
{
    public class AdminInspirationService : IAdminInspirationService
    {
        private readonly IOrderReviewImageRepository _repository;

        public AdminInspirationService(IOrderReviewImageRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedResult<AdminInspirationListDto>> GetInspirationsAsync(AdminInspirationFilterDto filter)
        {
            var query = _repository.GetAll()
                .Include(x => x.Order)
                    .ThenInclude(o => o.User)
                .AsQueryable();

            if (filter.IsApproved.HasValue)
            {
                query = query.Where(x => x.IsApproved == filter.IsApproved.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x => x.Order.Id.ToString().Contains(filter.Search) 
                                      || (x.Order.FirstName + " " + x.Order.LastName).Contains(filter.Search)
                                      || (x.Order.User != null && x.Order.User.FullName.Contains(filter.Search)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new AdminInspirationListDto
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    BeforeImageUrl = x.BeforeImageUrl,
                    AfterImageUrl = x.AfterImageUrl,
                    IsApproved = x.IsApproved,
                    CreatedAt = x.CreatedAt,
                    CustomerName = string.IsNullOrWhiteSpace(x.Order.FirstName) && string.IsNullOrWhiteSpace(x.Order.LastName)
                        ? (x.Order.User != null ? x.Order.User.FullName : "Unknown")
                        : (x.Order.FirstName + " " + x.Order.LastName).Trim()
                })
                .ToListAsync();

            return new PaginatedResult<AdminInspirationListDto>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<bool> ApproveInspirationAsync(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return false;

            item.IsApproved = true;
            item.UpdatedAt = DateTime.UtcNow;
            _repository.Update(item);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
