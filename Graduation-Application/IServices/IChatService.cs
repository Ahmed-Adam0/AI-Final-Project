using System.Threading;
using System.Threading.Tasks;
using Graduation_Application.DTOs.ChatDTO;

namespace Graduation_Application.IServices
{
    public interface IChatService
    {
        Task<ChatReplyDto> SendMessageAsync(
            ChatRequestDto request,
            CancellationToken cancellationToken = default
        );
    }
}
