using Graduation_Application.IServices;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Graduation_Application.ExternalServices.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode, int expiryMinutes)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"];

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var subject = "رمز التحقق من حسابك";
            var to = new EmailAddress(toEmail);
            var plainTextContent = $"رمز التحقق الخاص بك: {otpCode}\nصلاحية الرمز: {expiryMinutes} دقائق\n\nلا تشارك هذا الرمز مع أحد.";
            var htmlContent = $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; direction: rtl;'>
                    <h2>رمز التحقق من حسابك</h2>
                    <p>استخدم الرمز التالي لإعادة تعيين كلمة المرور:</p>
                    <h1 style='color: #007bff; text-align: center;'>{otpCode}</h1>
                    <p>صلاحية الرمز: <strong>{expiryMinutes} دقائق</strong></p>
                    <p style='color: #666;'>لا تشارك هذا الرمز مع أحد.</p>
                    <hr/>
                    <p style='color: #999; font-size: 12px;'>إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني.</p>
                </body>
                </html>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);
        }
    }
}

