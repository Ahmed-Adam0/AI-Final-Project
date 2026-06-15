using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface INotificationHub
    {
        Task SendAsync(string userId, string title, string message);
    }
}
