using Graduation_Application.DTOs.RoomDesignDTO;
using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IGeminiRoomDesignService
    {
        Task<RoomDesignResponseDto> GenerateRoomDesignAsync(RoomDesignRequestDto request);
        Task<RoomDesignResponseDto> GenerateRoomDesignFromUrlAsync(string roomImageUrl, List<int> productIds);
    }
}
