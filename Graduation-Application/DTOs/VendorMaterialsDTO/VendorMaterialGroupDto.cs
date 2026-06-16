using System.Collections.Generic;

namespace Graduation_Application.DTOs.VendorMaterialsDTO
{
    public class VendorMaterialGroupDto
    {
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;

        public List<VendorMaterialOptionDto> Options { get; set; } = new List<VendorMaterialOptionDto>();
    }
}
