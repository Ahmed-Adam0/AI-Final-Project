using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Admin.AuditLogsDTO;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    [Route("Admin/AuditLogs/[action]")]
    public class AuditLogsController : Controller
    {
        private static readonly string[] DefaultRoles = new[] { "SuperAdmin", "Customer", "Vendor" };
        private static readonly string[] DefaultActions = new[]
        {
            "CreateOrder",
            "CancelOrder",
            "SuspendUser",
            "ActivateUser",
            "DeleteUser",
            "DeleteProduct",
            "ApproveVendor",
            "RejectVendor",
            "UpdateOrderStatus",
            "CreateReview",
        };

        private static readonly Dictionary<string, List<string>> AllowedActionsByRole = new()
        {
            ["SuperAdmin"] = new List<string>
            {
                "SuspendUser",
                "ActivateUser",
                "DeleteUser",
                "ApproveVendor",
                "RejectVendor",
                "DeleteProduct",
                "UpdateOrderStatus",
            },
            ["Customer"] = new List<string>
            {
                "CreateOrder",
                "CancelOrder",
                "CreateReview",
            },
            ["Vendor"] = new List<string>
            {
                "UpdateOrderStatus",
            },
        };

        private readonly IAdminAuditLogsService _adminAuditLogsService;

        public AuditLogsController(IAdminAuditLogsService adminAuditLogsService)
        {
            _adminAuditLogsService = adminAuditLogsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AuditLogsFilterViewModel filter)
        {
            var actionFilter = Request.Query["ActionFilter"].FirstOrDefault();

            var dtoFilter = new AuditLogsFilterDto
            {
                Search = filter.Search,
                UserRole = filter.UserRole,
                Action = string.IsNullOrWhiteSpace(actionFilter) ? null : actionFilter,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                Page = filter.Page <= 0 ? 1 : filter.Page,
                PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize,
            };

            var result = await _adminAuditLogsService.GetLogsAsync(dtoFilter);

            var vm = new AuditLogsPageViewModel
            {
                Filter = new AuditLogsFilterViewModel
                {
                    Search = filter.Search,
                    UserRole = filter.UserRole,
                    Action = dtoFilter.Action,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    Page = dtoFilter.Page,
                    PageSize = dtoFilter.PageSize,
                },
                Logs = result
                    .Items.Select(x => new AuditLogListItemViewModel
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        UserRole = x.UserRole,
                        Action = x.Action,
                        EntityType = x.EntityType,
                        EntityId = x.EntityId,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                    })
                    .ToList(),
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                Page = result.PageNumber,
                PageSize = result.PageSize,
                RoleOptions = DefaultRoles.ToList(),
                ActionOptions = DefaultActions.ToList(),
                AllowedActionsByRole = AllowedActionsByRole,
            };

            return View(vm);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var dto = await _adminAuditLogsService.GetLogDetailsAsync(id);
                var vm = new AuditLogDetailsViewModel
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    UserName = dto.UserName,
                    UserRole = dto.UserRole,
                    Action = dto.Action,
                    EntityType = dto.EntityType,
                    EntityId = dto.EntityId,
                    Description = dto.Description,
                    CreatedAt = dto.CreatedAt,
                };

                return View(vm);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
