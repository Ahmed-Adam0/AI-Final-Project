using System.Threading.Tasks;
using System.Security.Claims;
using Graduation_Application.DTOs.Admin.AdminProductDTO;
using Graduation_Application.IServices.Admin;
using Graduation_MVC.Areas.Admin.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ProductsController : Controller
    {
        private readonly IAdminProductService _adminProductService;
        private readonly IAdminAuditLogsService _adminAuditLogsService;
        private readonly ILocalizationService _localizationService;

        public ProductsController(
            IAdminProductService adminProductService,
            IAdminAuditLogsService adminAuditLogsService,
            ILocalizationService localizationService
        )
        {
            _adminProductService = adminProductService;
            _adminAuditLogsService = adminAuditLogsService;
            _localizationService = localizationService;
        }

        // GET: /Admin/Products
        [HttpGet]
        public async Task<IActionResult> Index(AdminProductFilterDto filter)
        {
            var products = await _adminProductService.GetProductsAsync(filter);
            var categories = await _adminProductService.GetAllCategoriesAsync();
            var vendors = await _adminProductService.GetAllVendorsAsync();

            var viewModel = new AdminProductsPageViewModel
            {
                Search = filter.Search,
                CategoryId = filter.CategoryId,
                VendorId = filter.VendorId,
                Status = filter.Status,
                Products = products,
                Categories = categories,
                Vendors = vendors,
            };

            return View(viewModel);
        }

        // GET: /Admin/Products/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _adminProductService.GetProductDetailsAsync(id);
            if (product == null)
                return NotFound();

            var viewModel = new AdminProductDetailsViewModel { Product = product };

            return View(viewModel);
        }

        // POST: /Admin/Products/{id}/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            await _adminProductService.ActivateProductAsync(id);
            TempData["SuccessMessage"] = _localizationService.Get("admin.products.activate.success");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Products/{id}/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _adminProductService.DeactivateProductAsync(id);
            TempData["SuccessMessage"] = _localizationService.Get("admin.products.deactivate.success");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Products/{id}/Hide
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var product = await _adminProductService.GetProductDetailsAsync(id);
            await _adminProductService.HideProductAsync(id);
            await _adminAuditLogsService.CreateLogAsync(
                GetCurrentUserId(),
                GetCurrentUserName(),
                GetCurrentUserRole(),
                "DeleteProduct",
                "Product",
                id.ToString(),
                $"Deleted product '{product?.NameEn ?? "Unknown"}' (Id: {id}) by hiding it from marketplace."
            );
            TempData["SuccessMessage"] = _localizationService.Get("admin.products.hide.success");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Products/{id}/Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            await _adminProductService.RestoreProductAsync(id);
            TempData["SuccessMessage"] = _localizationService.Get("admin.products.restore.success");
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Products/Reported
        [HttpGet]
        public async Task<IActionResult> Reported()
        {
            var reportsDto = await _adminProductService.GetReportedProductsAsync();
            var reportsVm = new List<ReportedProductViewModel>();

            if (reportsDto != null)
            {
                foreach (var r in reportsDto)
                {
                    reportsVm.Add(
                        new ReportedProductViewModel
                        {
                            ReportId = r.ReportId,
                            ProductId = r.ProductId,
                            ProductNameEn = r.ProductNameEn,
                            ProductNameAr = r.ProductNameAr,
                            Reason = r.Reason,
                            ReportedByName = r.ReportedByName,
                            ReportedByEmail = r.ReportedByEmail,
                            CreatedAt = r.CreatedAt,
                            IsResolved = r.IsResolved,
                        }
                    );
                }
            }

            var viewModel = new ReportedProductsViewModel { Reports = reportsVm };

            return View(viewModel);
        }

        // POST: /Admin/Products/ResolveReport/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(int id)
        {
            await _adminProductService.ResolveReportAsync(id);
            TempData["SuccessMessage"] = _localizationService.Get("admin.products.resolve.success");
            return RedirectToAction(nameof(Reported));
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        private string GetCurrentUserName() =>
            User.FindFirstValue(ClaimTypes.Name)
            ?? User.Identity?.Name
            ?? "SuperAdmin";

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "SuperAdmin";
    }
}
