using System.Collections.Generic;
using Graduation_Application.DTOs.BannerDTO;

namespace Graduation_MVC.Areas.Admin.ViewModels.Banners
{
    public class AdminBannersPageViewModel
    {
        public List<BannerDto> Banners { get; set; } = new List<BannerDto>();
        public string? Search { get; set; }
    }
}
