using System.Collections.Generic;
using Graduation_Application.DTOs.FaqDTO;

namespace Graduation_MVC.Areas.Admin.ViewModels.FAQ
{
    public class AdminFaqPageViewModel
    {
        public List<FaqDto> Faqs { get; set; } = new List<FaqDto>();
        public string? Search { get; set; }
    }
}
