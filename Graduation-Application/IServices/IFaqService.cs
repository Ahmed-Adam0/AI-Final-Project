using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.FaqDTO;

namespace Graduation_Application.IServices
{
    public interface IFaqService
    {
        Task<List<FaqDto>> GetAllFaqsAsync();
        Task<FaqDto?> GetFaqByIdAsync(int id);
        Task<FaqResponseDto> CreateFaqAsync(CreateFaqDto createFaqDto);
        Task<FaqResponseDto> UpdateFaqAsync(int faqId, UpdateFaqDto updateFaqDto);
        Task<bool> DeleteFaqAsync(int faqId);
        Task<bool> UpdateFaqStatusAsync(int faqId, bool isActive);
    }
}
