using System.Threading.Tasks;
using Graduation_Application.IServices;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

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
            var subject = "رمز التحقق من حسابك";
            var plainTextContent =
                $"رمز التحقق الخاص بك: {otpCode}\nصلاحية الرمز: {expiryMinutes} دقائق\n\nلا تشارك هذا الرمز مع أحد.";
            var htmlContent =
                $@"
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

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        public async Task SendOrderCreatedEmailAsync(string toEmail, int orderId)
        {
            var subject = "تأكيد إنشاء الطلب";
            var plainTextContent = $"تم إنشاء طلبك رقم #{orderId} بنجاح.";
            var htmlContent =
                $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; direction: rtl;'>
                    <h2>تم إنشاء طلبك بنجاح</h2>
                    <p>رقم الطلب: <strong>#{orderId}</strong></p>
                    <p>شكراً لاستخدامك خدمتنا.</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        public async Task SendOrderStatusChangedEmailAsync(
            string toEmail,
            int orderId,
            string newStatus
        )
        {
            var subject = "تحديث حالة الطلب";
            var plainTextContent = $"تم تحديث حالة طلبك رقم #{orderId} إلى: {newStatus}.";
            var htmlContent =
                $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; direction: rtl;'>
                    <h2>تم تحديث حالة الطلب</h2>
                    <p>رقم الطلب: <strong>#{orderId}</strong></p>
                    <p>الحالة الجديدة: <strong>{newStatus}</strong></p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        private async Task SendEmailAsync(
            string toEmail,
            string subject,
            string plainTextContent,
            string htmlContent
        )
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"];

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlContent
            );
            await client.SendEmailAsync(msg);
        }
    }
}
