using Graduation_Application.DTOs.Admin.AdminProductDTO;

namespace Graduation_MVC.Areas.Admin.ViewModels.Products
{
    /// <summary>
    /// ViewModel for the Admin Product Details page.
    /// Wraps the AdminProductDetailsDto for view rendering.
    /// </summary>
    public class AdminProductDetailsViewModel
    {
        public AdminProductDetailsDto Product { get; set; }
    }
}
