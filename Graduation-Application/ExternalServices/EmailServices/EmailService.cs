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
                <html dir=""rtl"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: rtl; margin: 0; padding: 0;"">
                    <div dir=""rtl"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: right; direction: rtl;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <!-- Logo Header -->
                            <tr>
                                <td style=""padding: 25px 30px; text-align: center; background-color: #2B1A0A; border-bottom: 3px solid #C5A059;"">
                                    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin: 0 auto; direction: ltr;"">
                                        <tr>
                                            <td style=""padding-right: 12px; vertical-align: middle;"">
                                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width: 26px; height: 26px;"">
                                                    <!-- Top curve/line of F -->
                                                    <tr>
                                                        <td colspan=""5"" style=""height: 5px; background-color: #C5A059; border-radius: 3px 3px 0 0; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                    <!-- Spacer -->
                                                    <tr height=""2""><td colspan=""5"" style=""font-size: 1px; line-height: 1px;"">&nbsp;</td></tr>
                                                    <!-- 3 Pillars -->
                                                    <tr>
                                                        <!-- Pillar 1 (Left, tall) -->
                                                        <td valign=""top"" style=""width: 5px; height: 19px; background-color: #C5A059; border-radius: 0 0 2px 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 2 (Middle, medium) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 14px; background-color: #A38042; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 3 (Right, short) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 9px; background-color: #6D522B; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td style=""vertical-align: middle; line-height: 1;"">
                                                <span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #ffffff; letter-spacing: 0.5px;"">Furni</span><span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #C5A059; letter-spacing: 0.5px;"">Mind</span>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            
                            <!-- Card Body -->
                            <tr>
                                <td style=""padding: 30px 30px 20px 30px; background-color: #ffffff;"">
                                    <h2 style=""color: #2B1A0A; font-size: 24px; margin-top: 0; margin-bottom: 10px; font-weight: 700; text-align: center;"">رمز التحقق (OTP)</h2>
                                    <div style=""width: 80px; height: 2px; background-color: #E6DED4; margin: 0 auto 20px auto;""></div>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 20px; direction: rtl;"">
                                        مرحبًا،<br/>
                                        لقد طلبت رمز تحقق لتسجيل الدخول أو إتمام عملية حساسة على حسابك. يرجى استخدام الكود التالي:
                                    </p>
                                    
                                    <!-- Centered OTP Box -->
                                    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin: 20px auto; text-align: center; width: 100%; max-width: 400px;"">
                                        <tr>
                                            <td style=""background-color: #FAF6F0; border: 1.5px dashed #C5A059; border-radius: 8px; padding: 15px 0; text-align: center;"">
                                                <span style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 32px; font-weight: bold; color: #000000; letter-spacing: 5px; display: block; text-align: center;"">{otpCode}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    
                                    <p style=""color: #C5A059; font-size: 15px; font-weight: bold; text-align: center; margin-top: 15px; margin-bottom: 25px;"">
                                        صلاحية الرمز: {expiryMinutes} دقائق
                                    </p>
                                    
                                    <!-- Security Warning Banner -->
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF5F5; border-right: 4px solid #D9534F; border-radius: 4px; margin: 20px 0;"">
                                        <tr>
                                            <td style=""padding: 12px 15px; color: #D9534F; font-size: 14px; font-weight: bold; text-align: right; direction: rtl;"">
                                                ⚠️ لا تشارك هذا الرمز مع أي شخص لحماية حسابك.
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            
                            <!-- Footer -->
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; text-align: center; margin: 0; font-weight: 500;"">© 2026 FurniMind جميع الحقوق محفوظة.</p>
                                </td>
                            </tr>
                        </table>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        public async Task SendEmailConfirmationOtpAsync(string toEmail, string otpCode, int expiryMinutes)
        {
            var subject = "تأكيد بريدك الإلكتروني";
            var plainTextContent =
                $"رمز تأكيد بريدك الإلكتروني: {otpCode}\nصلاحية الرمز: {expiryMinutes} دقائق\n\nلا تشارك هذا الرمز مع أحد.";
            var htmlContent =
                $@"
                <html dir=""rtl"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: rtl; margin: 0; padding: 0;"">
                    <div dir=""rtl"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: right; direction: rtl;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <!-- Logo Header -->
                            <tr>
                                <td style=""padding: 25px 30px; text-align: center; background-color: #2B1A0A; border-bottom: 3px solid #C5A059;"">
                                    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin: 0 auto; direction: ltr;"">
                                        <tr>
                                            <td style=""padding-right: 12px; vertical-align: middle;"">
                                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width: 26px; height: 26px;"">
                                                    <!-- Top curve/line of F -->
                                                    <tr>
                                                        <td colspan=""5"" style=""height: 5px; background-color: #C5A059; border-radius: 3px 3px 0 0; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                    <!-- Spacer -->
                                                    <tr height=""2""><td colspan=""5"" style=""font-size: 1px; line-height: 1px;"">&nbsp;</td></tr>
                                                    <!-- 3 Pillars -->
                                                    <tr>
                                                        <!-- Pillar 1 (Left, tall) -->
                                                        <td valign=""top"" style=""width: 5px; height: 19px; background-color: #C5A059; border-radius: 0 0 2px 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 2 (Middle, medium) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 14px; background-color: #A38042; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 3 (Right, short) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 9px; background-color: #6D522B; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td style=""vertical-align: middle; line-height: 1;"">
                                                <span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #ffffff; letter-spacing: 0.5px;"">Furni</span><span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #C5A059; letter-spacing: 0.5px;"">Mind</span>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            
                            <!-- Card Body -->
                            <tr>
                                <td style=""padding: 30px 30px 20px 30px; background-color: #ffffff;"">
                                    <h2 style=""color: #2B1A0A; font-size: 24px; margin-top: 0; margin-bottom: 10px; font-weight: 700; text-align: center;"">تأكيد بريدك الإلكتروني</h2>
                                    <div style=""width: 80px; height: 2px; background-color: #E6DED4; margin: 0 auto 20px auto;""></div>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 20px; direction: rtl;"">
                                        مرحبًا،<br/>
                                        يرجى استخدام رمز التحقق التالي لتأكيد بريدك الإلكتروني وتفعيل حسابك في FurniMind:
                                    </p>
                                    
                                    <!-- Centered OTP Box -->
                                    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin: 20px auto; text-align: center; width: 100%; max-width: 400px;"">
                                        <tr>
                                            <td style=""background-color: #FAF6F0; border: 1.5px dashed #C5A059; border-radius: 8px; padding: 15px 0; text-align: center;"">
                                                <span style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 32px; font-weight: bold; color: #000000; letter-spacing: 5px; display: block; text-align: center;"">{otpCode}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    
                                    <p style=""color: #C5A059; font-size: 15px; font-weight: bold; text-align: center; margin-top: 15px; margin-bottom: 25px;"">
                                        صلاحية الرمز: {expiryMinutes} دقائق
                                    </p>
                                    
                                    <!-- Security Warning Banner -->
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF5F5; border-right: 4px solid #D9534F; border-radius: 4px; margin: 20px 0;"">
                                        <tr>
                                            <td style=""padding: 12px 15px; color: #D9534F; font-size: 14px; font-weight: bold; text-align: right; direction: rtl;"">
                                                ⚠️ لا تشارك هذا الرمز مع أي شخص لحماية حسابك.
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            
                            <!-- Footer -->
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; text-align: center; margin: 0; font-weight: 500;"">© 2026 FurniMind جميع الحقوق محفوظة.</p>
                                </td>
                            </tr>
                        </table>
                    </div>
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
                <html dir=""rtl"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: rtl; margin: 0; padding: 0;"">
                    <div dir=""rtl"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: right; direction: rtl;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <!-- Logo Header -->
                            <tr>
                                <td style=""padding: 25px 30px; text-align: center; background-color: #2B1A0A; border-bottom: 3px solid #C5A059;"">
                                    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin: 0 auto; direction: ltr;"">
                                        <tr>
                                            <td style=""padding-right: 12px; vertical-align: middle;"">
                                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width: 26px; height: 26px;"">
                                                    <!-- Top curve/line of F -->
                                                    <tr>
                                                        <td colspan=""5"" style=""height: 5px; background-color: #C5A059; border-radius: 3px 3px 0 0; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                    <!-- Spacer -->
                                                    <tr height=""2""><td colspan=""5"" style=""font-size: 1px; line-height: 1px;"">&nbsp;</td></tr>
                                                    <!-- 3 Pillars -->
                                                    <tr>
                                                        <!-- Pillar 1 (Left, tall) -->
                                                        <td valign=""top"" style=""width: 5px; height: 19px; background-color: #C5A059; border-radius: 0 0 2px 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 2 (Middle, medium) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 14px; background-color: #A38042; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 3 (Right, short) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 9px; background-color: #6D522B; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td style=""vertical-align: middle; line-height: 1;"">
                                                <span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #ffffff; letter-spacing: 0.5px;"">Furni</span><span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #C5A059; letter-spacing: 0.5px;"">Mind</span>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            
                            <!-- Card Body -->
                            <tr>
                                <td style=""padding: 30px 30px 20px 30px; background-color: #ffffff;"">
                                    <h2 style=""color: #2B1A0A; font-size: 24px; margin-top: 0; margin-bottom: 10px; font-weight: 700; text-align: center;"">تأكيد إنشاء الطلب</h2>
                                    <div style=""width: 80px; height: 2px; background-color: #E6DED4; margin: 0 auto 20px auto;""></div>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 20px; direction: rtl;"">
                                        مرحبًا،<br/>
                                        نسعد باختياركم لـ <strong>FurniMind</strong>! تم إنشاء طلبكم بنجاح وهو قيد المراجعة الآن.
                                    </p>
                                    
                                    <!-- Order Detail Card -->
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF9F6; border-radius: 10px; border: 1px solid #E6DED4; margin: 25px 0;"">
                                        <tr>
                                            <td style=""padding: 20px; text-align: center;"">
                                                <span style=""color: #8C7E72; font-size: 14px; display: block; margin-bottom: 5px;"">رقم الطلب</span>
                                                <span style=""color: #2B1A0A; font-size: 26px; font-weight: bold;"">#{orderId}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 15px;"">سنقوم بإرسال إشعارات وتحديثات أخرى بمجرد تغيير حالة الطلب أو البدء في التجهيز.</p>
                                </td>
                            </tr>
                            
                            <!-- Footer -->
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; text-align: center; margin: 0; font-weight: 500;"">© 2026 FurniMind جميع الحقوق محفوظة.</p>
                                </td>
                            </tr>
                        </table>
                    </div>
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
                <html dir=""rtl"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: rtl; margin: 0; padding: 0;"">
                    <div dir=""rtl"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: right; direction: rtl;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <!-- Logo Header -->
                            <tr>
                                <td style=""padding: 25px 30px; text-align: center; background-color: #2B1A0A; border-bottom: 3px solid #C5A059;"">
                                     <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin: 0 auto; direction: ltr;"">
                                        <tr>
                                            <td style=""padding-right: 12px; vertical-align: middle;"">
                                                <table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width: 26px; height: 26px;"">
                                                    <!-- Top curve/line of F -->
                                                    <tr>
                                                        <td colspan=""5"" style=""height: 5px; background-color: #C5A059; border-radius: 3px 3px 0 0; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                    <!-- Spacer -->
                                                    <tr height=""2""><td colspan=""5"" style=""font-size: 1px; line-height: 1px;"">&nbsp;</td></tr>
                                                    <!-- 3 Pillars -->
                                                    <tr>
                                                        <!-- Pillar 1 (Left, tall) -->
                                                        <td valign=""top"" style=""width: 5px; height: 19px; background-color: #C5A059; border-radius: 0 0 2px 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 2 (Middle, medium) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 14px; background-color: #A38042; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Spacer -->
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <!-- Pillar 3 (Right, short) -->
                                                        <td valign=""bottom"" style=""width: 5px; height: 9px; background-color: #6D522B; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td style=""vertical-align: middle; line-height: 1;"">
                                                <span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #ffffff; letter-spacing: 0.5px;"">Furni</span><span style=""font-family: 'Outfit', 'Segoe UI', Tahoma, sans-serif; font-size: 26px; font-weight: bold; color: #C5A059; letter-spacing: 0.5px;"">Mind</span>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            
                            <!-- Card Body -->
                            <tr>
                                <td style=""padding: 30px 30px 20px 30px; background-color: #ffffff;"">
                                    <h2 style=""color: #2B1A0A; font-size: 24px; margin-top: 0; margin-bottom: 10px; font-weight: 700; text-align: center;"">تحديث حالة الطلب</h2>
                                    <div style=""width: 80px; height: 2px; background-color: #E6DED4; margin: 0 auto 20px auto;""></div>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 20px; direction: rtl;"">
                                        مرحبًا،<br/>
                                        نود إعلامكم بأنه قد تم تحديث حالة طلبكم بنجاح.
                                    </p>
                                    
                                    <!-- Order Status Card -->
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF9F6; border-radius: 10px; border: 1px solid #E6DED4; margin: 25px 0;"">
                                        <tr>
                                            <td style=""padding: 20px; text-align: center; border-left: 1px solid #E6DED4; width: 50%;"">
                                                <span style=""color: #8C7E72; font-size: 14px; display: block; margin-bottom: 5px;"">رقم الطلب</span>
                                                <span style=""color: #2B1A0A; font-size: 20px; font-weight: bold;"">#{orderId}</span>
                                            </td>
                                            <td style=""padding: 20px; text-align: center; width: 50%;"">
                                                <span style=""color: #8C7E72; font-size: 14px; display: block; margin-bottom: 5px;"">الحالة الجديدة</span>
                                                <span style=""background-color: #C5A059; color: #ffffff; font-size: 14px; font-weight: bold; padding: 6px 16px; border-radius: 20px; display: inline-block;"">{newStatus}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 15px;"">شكراً لاختياركم FurniMind وثقتكم بنا.</p>
                                </td>
                            </tr>
                            
                            <!-- Footer -->
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; text-align: center; margin: 0; font-weight: 500;"">© 2026 FurniMind جميع الحقوق محفوظة.</p>
                                </td>
                            </tr>
                        </table>
                    </div>
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

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // No SendGrid API key configured; log and skip sending email.
                System.Console.Error.WriteLine("SendGrid ApiKey is not configured. Skipping email send.");
                return;
            }

            try
            {
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(string.IsNullOrWhiteSpace(fromEmail) ? "no-reply@example.com" : fromEmail, string.IsNullOrWhiteSpace(fromName) ? "NoReply" : fromName);
                var to = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plainTextContent,
                    htmlContent
                );

                var response = await client.SendEmailAsync(msg);

                // Log response for diagnostics
                var statusCode = (int)response.StatusCode;

                string responseBodyStr = string.Empty;
                var responseBodyObj = response.Body as object;
                if (responseBodyObj is string respString)
                {
                    responseBodyStr = respString;
                }
                else if (responseBodyObj is System.Net.Http.HttpContent httpContent)
                {
                    try
                    {
                        responseBodyStr = await httpContent.ReadAsStringAsync();
                    }
                    catch
                    {
                        responseBodyStr = response.ToString();
                    }
                }
                else
                {
                    responseBodyStr = response.ToString();
                }

                if (statusCode >= 200 && statusCode < 300)
                {
                    System.Console.WriteLine($"SendGrid email sent to {toEmail} with status {statusCode}. Response: {responseBodyStr}");
                }
                else
                {
                    System.Console.Error.WriteLine($"SendGrid failed to send email to {toEmail}. Status: {statusCode}. Response: {responseBodyStr}");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine($"Exception while sending email via SendGrid: {ex.Message}");
            }
        }
    }
}
