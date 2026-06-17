using Graduation_Application.DTOs.VendorMaterialsDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graduation_Application.IServices.Vendor
{
    public interface IVendorMaterialService
    {
        Task<VendorMaterialGroupDto> CreateGroupAsync(int workshopId, CreateVendorMaterialGroupDto dto);
        Task<VendorMaterialOptionDto> AddOptionAsync(int workshopId, int groupId, CreateVendorMaterialOptionDto dto);
        Task<VendorMaterialOptionDto> UpdateOptionAsync(int workshopId, int optionId, UpdateVendorMaterialOptionDto dto);
        Task<List<VendorMaterialGroupDto>> GetVendorMaterialsAsync(int workshopId);
        Task DeleteGroupAsync(int workshopId, int groupId);
        Task DeleteOptionAsync(int workshopId, int optionId);
    }
}
