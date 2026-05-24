using System.Threading.Tasks;

namespace Graduation_Application.IServices
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode, int expiryMinutes);
    }
}
