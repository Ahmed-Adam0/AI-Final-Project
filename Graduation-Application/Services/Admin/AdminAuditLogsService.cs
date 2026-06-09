using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AuditLogsDTO;
using Graduation_Application.DTOs.Common;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices.Admin;
using Graduation_domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Application.Services.Admin
{
    public class AdminAuditLogsService : IAdminAuditLogsService
    {
        private readonly IGenaricRepositories<ActivityLog> _activityLogRepository;

        public AdminAuditLogsService(IGenaricRepositories<ActivityLog> activityLogRepository)
        {
            _activityLogRepository = activityLogRepository;
        }

        public async Task<PaginatedResult<AuditLogListItemDto>> GetLogsAsync(AuditLogsFilterDto filter)
        {
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var query = _activityLogRepository.GetAllAsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(x =>
                    (x.UserName ?? string.Empty).ToLower().Contains(search)
                    || (x.Action ?? string.Empty).ToLower().Contains(search)
                    || (x.ActionAr ?? string.Empty).ToLower().Contains(search)
                    || (x.EntityType ?? string.Empty).ToLower().Contains(search)
                    || (x.EntityTypeAr ?? string.Empty).ToLower().Contains(search)
                    || (x.EntityId ?? string.Empty).ToLower().Contains(search)
                    || (x.Description ?? string.Empty).ToLower().Contains(search)
                    || (x.UserRole ?? string.Empty).ToLower().Contains(search)
                    || (x.UserRoleAr ?? string.Empty).ToLower().Contains(search)
                    || (x.DescriptionAr ?? string.Empty).ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.UserRole))
            {
                var role = filter.UserRole.Trim();
                query = query.Where(x => x.UserRole == role);
            }

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                var action = filter.Action.Trim();
                query = query.Where(x => x.Action == action);
            }

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(x => x.CreatedAt >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var toExclusive = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedAt < toExclusive);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Adapt<List<AuditLogListItemDto>>();

            return new PaginatedResult<AuditLogListItemDto>(dtos, totalCount, page, pageSize);
        }

        public async Task<AuditLogDetailsDto> GetLogDetailsAsync(int id)
        {
            var entity = await _activityLogRepository.GetByIdAsNoTrackingAsync(id);
            if (entity == null)
            {
                throw new Exception("Activity log not found");
            }

            return entity.Adapt<AuditLogDetailsDto>();
        }

        public async Task CreateLogAsync(
            string userId,
            string userName,
            string userRole,
            string action,
            string entityType,
            string? entityId,
            string description,
            string? userRoleAr = null,
            string? actionAr = null,
            string? entityTypeAr = null,
            string? descriptionAr = null
        )
        {
            var entity = new ActivityLog
            {
                UserId = userId,
                UserName = userName,
                UserRole = userRole,
                UserRoleAr = userRoleAr ?? userRole,
                Action = action,
                ActionAr = actionAr ?? action,
                EntityType = entityType,
                EntityTypeAr = entityTypeAr ?? entityType,
                EntityId = entityId,
                Description = description,
                DescriptionAr = descriptionAr ?? description,
                CreatedAt = DateTime.UtcNow,
            };

            await _activityLogRepository.AddAsync(entity);
            await _activityLogRepository.SaveChangesAsync();
        }
    }
}
