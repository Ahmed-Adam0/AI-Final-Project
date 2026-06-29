using System.Collections.Generic;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ShowcaseDTO;

namespace Graduation_Application.IServices
{
    public interface IShowcaseService
    {
        Task<List<ShowcaseSlideDto>> GetActiveShowcaseAsync();
        Task<ShowcaseSlideDto> CreateShowcaseSlideAsync(int workshopId, CreateShowcaseSlideRequest request);
        Task<ShowcaseSlideDto> UpdateShowcaseSlideAsync(int id, int workshopId, UpdateShowcaseSlideRequest request);
        Task<bool> DeleteShowcaseSlideAsync(int id, int workshopId);
    }
}
