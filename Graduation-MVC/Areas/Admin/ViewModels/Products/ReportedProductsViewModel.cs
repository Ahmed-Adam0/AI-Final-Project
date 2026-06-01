using System.Collections.Generic;

namespace Graduation_MVC.Areas.Admin.ViewModels.Products
{
    /// <summary>
    /// ViewModel for the Admin Reported Products page.
    /// </summary>
    public class ReportedProductsViewModel
    {
        public List<ReportedProductViewModel> Reports { get; set; } = new List<ReportedProductViewModel>();
    }
}
