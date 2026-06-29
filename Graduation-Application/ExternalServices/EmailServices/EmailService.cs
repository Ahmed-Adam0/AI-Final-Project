using System;
using System.Threading.Tasks;
using Graduation_Application.IServices;
using Graduation_Application.IServices.Admin;
using Microsoft.Extensions.Configuration;
//using SendGrid;
//using SendGrid.Helpers.Mail;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Graduation_Application.ExternalServices.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILocalizationService _localizationService;

        public EmailService(IConfiguration configuration, ILocalizationService localizationService)
        {
            _configuration = configuration;
            _localizationService = localizationService;
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

        public async Task SendNewOrderVendorEmailAsync(string toEmail, int vendorOrderId)
        {
            var subject = "لديك طلب جديد في FurniMind";
            var plainTextContent = $"لديك طلب جديد رقم #{vendorOrderId} بانتظار موافقتك وتجهيزه.";
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
                                                    <tr>
                                                        <td colspan=""5"" style=""height: 5px; background-color: #C5A059; border-radius: 3px 3px 0 0; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                    </tr>
                                                    <tr height=""2""><td colspan=""5"" style=""font-size: 1px; line-height: 1px;"">&nbsp;</td></tr>
                                                    <tr>
                                                        <td valign=""top"" style=""width: 5px; height: 19px; background-color: #C5A059; border-radius: 0 0 2px 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <td valign=""bottom"" style=""width: 5px; height: 14px; background-color: #A38042; border-radius: 2px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
                                                        <td style=""width: 4px; font-size: 1px; line-height: 1px;"">&nbsp;</td>
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
                                    <h2 style=""color: #2B1A0A; font-size: 24px; margin-top: 0; margin-bottom: 10px; font-weight: 700; text-align: center;"">طلب بائع جديد</h2>
                                    <div style=""width: 80px; height: 2px; background-color: #E6DED4; margin: 0 auto 20px auto;""></div>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 20px; direction: rtl;"">
                                        مرحبًا،<br/>
                                        لقد تلقيت طلبًا جديدًا من أحد العملاء في <strong>FurniMind</strong>. يرجى الدخول إلى لوحة التحكم الخاصة بك للقبول والبدء بالتجهيز:
                                    </p>
                                    
                                    <!-- Order Detail Card -->
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF9F6; border-radius: 10px; border: 1px solid #E6DED4; margin: 25px 0;"">
                                        <tr>
                                            <td style=""padding: 20px; text-align: center;"">
                                                <span style=""color: #8C7E72; font-size: 14px; display: block; margin-bottom: 5px;"">رقم طلب البائع الخاص بك</span>
                                                <span style=""color: #2B1A0A; font-size: 26px; font-weight: bold;"">#{vendorOrderId}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6; text-align: right; margin-bottom: 15px;"">يرجى الالتزام بمواعيد التجهيز المحددة وجودة التصنيع المطلوبة.</p>
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

        public async Task SendDeliveryDateProposedEmailAsync(string toEmail, int vendorOrderId, System.DateTime proposedDate)
        {
            var subject = "مقترح تاريخ التوصيل لطلبكم - FurniMind";
            var plainTextContent = $"تم تقديم مقترح لتاريخ التوصيل لطلب البائع رقم #{vendorOrderId}. التاريخ المقترح: {proposedDate:yyyy-MM-dd}.";
            var htmlContent =
                $@"
                <html dir=""rtl"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: rtl; margin: 0; padding: 0;"">
                    <div dir=""rtl"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: right; direction: rtl;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <tr style=""background-color: #2B1A0A; text-align: center; border-bottom: 3px solid #C5A059;"">
                                <td style=""padding: 25px 30px; color: #ffffff; font-size: 24px; font-weight: bold;"">مقترح تاريخ التوصيل</td>
                            </tr>
                            <tr>
                                <td style=""padding: 30px; background-color: #ffffff;"">
                                    <p>مرحبًا،</p>
                                    <p>تم تقديم مقترح لتاريخ التوصيل لطلب البائع رقم <strong>#{vendorOrderId}</strong>:</p>
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF9F6; border-radius: 10px; border: 1px solid #E6DED4; margin: 25px 0;"">
                                        <tr>
                                            <td style=""padding: 20px; text-align: center;"">
                                                <span style=""color: #8C7E72; font-size: 14px; display: block; margin-bottom: 5px;"">التاريخ المقترح</span>
                                                <span style=""color: #2B1A0A; font-size: 22px; font-weight: bold;"">{proposedDate:yyyy-MM-dd}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    <p>يرجى الدخول إلى حسابك للمراجعة والقبول أو الرفض.</p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; margin: 0;"">© 2026 FurniMind جميع الحقوق محفوظة.</p>
                                </td>
                            </tr>
                        </table>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        public async Task SendDeliveryDateApprovedEmailAsync(string toEmail, int vendorOrderId)
        {
            var subject = "قبول تاريخ التوصيل - FurniMind";
            var plainTextContent = $"قام العميل بقبول تاريخ التوصيل المقترح لطلب البائع رقم #{vendorOrderId}.";
            var htmlContent =
                $@"
                <html dir=""rtl"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: rtl; margin: 0; padding: 0;"">
                    <div dir=""rtl"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: right; direction: rtl;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <tr style=""background-color: #2B1A0A; text-align: center; border-bottom: 3px solid #C5A059;"">
                                <td style=""padding: 25px 30px; color: #ffffff; font-size: 24px; font-weight: bold;"">تم قبول تاريخ التوصيل</td>
                            </tr>
                            <tr>
                                <td style=""padding: 30px; background-color: #ffffff;"">
                                    <p>مرحبًا،</p>
                                    <p>نود إعلامكم بأن العميل قد <strong>قبل</strong> تاريخ التوصيل المقترح لطلب البائع رقم <strong>#{vendorOrderId}</strong>.</p>
                                    <p>يمكنكم الآن البدء في عملية التجهيز والتنفيذ.</p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; margin: 0;"">© 2026 FurniMind جميع الحقوق محفوظة.</p>
                                </td>
                            </tr>
                        </table>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        public async Task SendDeliveryDateRejectedEmailAsync(string toEmail, int vendorOrderId)
        {
            var subject = "رفض تاريخ التوصيل المقترح - FurniMind";
            var plainTextContent = $"قام العميل برفض تاريخ التوصيل المقترح لطلب البائع رقم #{vendorOrderId}. يرجى تقديم مقترح جديد.";
            var htmlContent =
                $@"
                <html dir=""rtl"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: rtl; margin: 0; padding: 0;"">
                    <div dir=""rtl"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: right; direction: rtl;"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <tr style=""background-color: #FAF5F5; text-align: center; border-bottom: 3px solid #D9534F;"">
                                <td style=""padding: 25px 30px; color: #D9534F; font-size: 24px; font-weight: bold;"">تم رفض تاريخ التوصيل</td>
                            </tr>
                            <tr>
                                <td style=""padding: 30px; background-color: #ffffff;"">
                                    <p>مرحبًا،</p>
                                    <p>قام العميل <strong>برفض</strong> تاريخ التوصيل المقترح لطلب البائع رقم <strong>#{vendorOrderId}</strong>.</p>
                                    <p>يرجى الدخول إلى حسابك وتقديم مقترح جديد لتاريخ التوصيل لتفادي تأخر الطلب.</p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; margin: 0;"">© 2026 FurniMind جميع الحقوق محفوظة.</p>
                                </td>
                            </tr>
                        </table>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        public async Task SendMilestoneCreatedEmailAsync(string toEmail, int vendorOrderId, string milestoneName, decimal amount, string lang)
        {
            var subjectTemplate = _localizationService.Get("email.milestoneCreatedSubject", lang);
            var bodyTemplate = _localizationService.Get("email.milestoneCreatedBodyTemplate", lang);

            // Fetch name of the milestone status (localized)
            var milestoneStatusName = _localizationService.Get($"orders.status{milestoneName}", lang);
            
            var subject = string.Format(subjectTemplate, vendorOrderId);
            var plainTextContent = string.Format(bodyTemplate, milestoneStatusName, amount, vendorOrderId);
            var htmlContent = GetMilestoneCreatedHtml(lang, milestoneStatusName, amount, vendorOrderId);

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        public async Task SendMilestonePaymentSuccessEmailAsync(string toEmail, int vendorOrderId, string milestoneName, decimal amount, string lang)
        {
            var subjectTemplate = _localizationService.Get("email.milestonePaymentSuccessSubject", lang);
            var bodyTemplate = _localizationService.Get("email.milestonePaymentSuccessBodyTemplate", lang);

            // Fetch name of the milestone status (localized)
            var milestoneStatusName = _localizationService.Get($"orders.status{milestoneName}", lang);

            var subject = string.Format(subjectTemplate, vendorOrderId);
            var plainTextContent = string.Format(bodyTemplate, milestoneStatusName, amount, vendorOrderId);
            var htmlContent = GetMilestonePaidHtml(lang, milestoneStatusName, amount, vendorOrderId);

            await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
        }

        private string GetMilestoneCreatedHtml(string lang, string milestoneName, decimal amount, int vendorOrderId)
        {
            var isAr = lang == "ar";
            var dir = isAr ? "rtl" : "ltr";
            var align = isAr ? "right" : "left";
            var title = isAr ? "مرحلة دفع جديدة قيد الانتظار" : "New Payment Milestone Pending";
            var greeting = isAr ? "مرحبًا،" : "Hello,";
            var intro = isAr 
                ? $"تم إنشاء مرحلة الدفع <strong>({milestoneName})</strong> لطلب البائع رقم <strong>#{vendorOrderId}</strong> بقيمة:"
                : $"A new payment milestone <strong>({milestoneName})</strong> has been created for vendor order <strong>#{vendorOrderId}</strong> with amount:";
            var note = isAr 
                ? "يرجى سداد هذه الدفعة لبدء أو مواصلة معالجة طلبكم وشحنه."
                : "Please proceed with this payment to initiate or continue processing and shipping of your order.";
            var copyright = isAr ? "© 2026 FurniMind جميع الحقوق محفوظة." : "© 2026 FurniMind. All rights reserved.";

            return $@"
                <html dir=""{dir}"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: {dir}; margin: 0; padding: 0;"">
                    <div dir=""{dir}"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: {align}; direction: {dir};"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <tr style=""background-color: #2B1A0A; text-align: center; border-bottom: 3px solid #C5A059;"">
                                <td style=""padding: 25px 30px; color: #ffffff; font-size: 24px; font-weight: bold;"">{title}</td>
                            </tr>
                            <tr>
                                <td style=""padding: 30px; background-color: #ffffff;"">
                                    <p style=""color: #4A3F35; font-size: 16px; margin-top: 0;"">{greeting}</p>
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6;"">{intro}</p>
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF9F6; border-radius: 10px; border: 1px solid #E6DED4; margin: 25px 0;"">
                                        <tr>
                                            <td style=""padding: 20px; text-align: center;"">
                                                <span style=""color: #8C7E72; font-size: 14px; display: block; margin-bottom: 5px;"">{(isAr ? "مبلغ الدفعة" : "Milestone Amount")}</span>
                                                <span style=""color: #2B1A0A; font-size: 28px; font-weight: bold;"">{(isAr ? $"{amount} ج.م" : $"EGP {amount}")}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style=""color: #8C7E72; font-size: 14px; line-height: 1.6;"">{note}</p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; margin: 0;"">{copyright}</p>
                                </td>
                            </tr>
                        </table>
                    </div>
                </body>
                </html>";
        }

        private string GetMilestonePaidHtml(string lang, string milestoneName, decimal amount, int vendorOrderId)
        {
            var isAr = lang == "ar";
            var dir = isAr ? "rtl" : "ltr";
            var align = isAr ? "right" : "left";
            var title = isAr ? "تأكيد سداد الدفعة" : "Payment Milestone Confirmed";
            var greeting = isAr ? "مرحبًا،" : "Hello,";
            var intro = isAr 
                ? $"تم سداد مرحلة الدفع <strong>({milestoneName})</strong> لطلب البائع رقم <strong>#{vendorOrderId}</strong> بقيمة:"
                : $"The payment milestone <strong>({milestoneName})</strong> for vendor order <strong>#{vendorOrderId}</strong> has been successfully paid in the amount of:";
            var note = isAr 
                ? "تم استلام الدفعة بنجاح، وسنواصل تجهيز وتوصيل طلبكم."
                : "Your payment has been successfully received. We will continue processing and delivering your order.";
            var copyright = isAr ? "© 2026 FurniMind جميع الحقوق محفوظة." : "© 2026 FurniMind. All rights reserved.";

            return $@"
                <html dir=""{dir}"">
                <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F7F4EB; direction: {dir}; margin: 0; padding: 0;"">
                    <div dir=""{dir}"" style=""background-color: #F7F4EB; padding: 35px 15px; text-align: {align}; direction: {dir};"">
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(43, 26, 10, 0.06); border: 1px solid #E6DED4;"">
                            <tr style=""background-color: #2B1A0A; text-align: center; border-bottom: 3px solid #C5A059;"">
                                <td style=""padding: 25px 30px; color: #ffffff; font-size: 24px; font-weight: bold;"">{title}</td>
                            </tr>
                            <tr>
                                <td style=""padding: 30px; background-color: #ffffff;"">
                                    <p style=""color: #4A3F35; font-size: 16px; margin-top: 0;"">{greeting}</p>
                                    <p style=""color: #4A3F35; font-size: 15px; line-height: 1.6;"">{intro}</p>
                                    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #FAF9F6; border-radius: 10px; border: 1px solid #E6DED4; margin: 25px 0;"">
                                        <tr>
                                            <td style=""padding: 20px; text-align: center;"">
                                                <span style=""color: #8C7E72; font-size: 14px; display: block; margin-bottom: 5px;"">{(isAr ? "المبلغ المدفوع" : "Amount Paid")}</span>
                                                <span style=""color: #2B1A0A; font-size: 28px; font-weight: bold;"">{(isAr ? $"{amount} ج.م" : $"EGP {amount}")}</span>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style=""color: #8C7E72; font-size: 14px; line-height: 1.6;"">{note}</p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 20px; background-color: #FAF9F6; border-top: 1px solid #F3ECE3; text-align: center;"">
                                    <p style=""color: #8C7E72; font-size: 12px; margin: 0;"">{copyright}</p>
                                </td>
                            </tr>
                        </table>
                    </div>
                </body>
                </html>";
        }

        //private async Task SendEmailAsync(
        //    string toEmail,
        //    string subject,
        //    string plainTextContent,
        //    string htmlContent
        //)
        //{
        //    var apiKey = _configuration["SendGrid:ApiKey"];
        //    var fromEmail = _configuration["SendGrid:FromEmail"];
        //    var fromName = _configuration["SendGrid:FromName"];
        //
        //    if (string.IsNullOrWhiteSpace(apiKey))
        //    {
        //        // No SendGrid API key configured; log and skip sending email.
        //        System.Console.Error.WriteLine("SendGrid ApiKey is not configured. Skipping email send.");
        //        return;
        //    }
        //
        //    try
        //    {
        //        var client = new SendGridClient(apiKey);
        //        var from = new EmailAddress(string.IsNullOrWhiteSpace(fromEmail) ? "no-reply@example.com" : fromEmail, string.IsNullOrWhiteSpace(fromName) ? "NoReply" : fromName);
        //        var to = new EmailAddress(toEmail);
        //
        //        var msg = MailHelper.CreateSingleEmail(
        //            from,
        //            to,
        //            subject,
        //            plainTextContent,
        //            htmlContent
        //        );
        //
        //        var response = await client.SendEmailAsync(msg);
        //
        //        // Log response for diagnostics
        //        var statusCode = (int)response.StatusCode;
        //
        //        string responseBodyStr = string.Empty;
        //        var responseBodyObj = response.Body as object;
        //        if (responseBodyObj is string respString)
        //        {
        //            responseBodyStr = respString;
        //        }
        //        else if (responseBodyObj is System.Net.Http.HttpContent httpContent)
        //        {
        //            try
        //            {
        //                responseBodyStr = await httpContent.ReadAsStringAsync();
        //            }
        //            catch
        //            {
        //                responseBodyStr = response.ToString();
        //            }
        //        }
        //        else
        //        {
        //            responseBodyStr = response.ToString();
        //        }
        //
        //        if (statusCode >= 200 && statusCode < 300)
        //        {
        //            System.Console.WriteLine($"SendGrid email sent to {toEmail} with status {statusCode}. Response: {responseBodyStr}");
        //        }
        //        else
        //        {
        //            System.Console.Error.WriteLine($"SendGrid failed to send email to {toEmail}. Status: {statusCode}. Response: {responseBodyStr}");
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        System.Console.Error.WriteLine($"Exception while sending email via SendGrid: {ex.Message}");
        //    }
        //}

        //for smtp email
        private async Task SendEmailAsync(
            string toEmail,
            string subject,
            string plainTextContent,
            string htmlContent
        )
        {
            var emailAddress = _configuration["EmailSettings:Email"];
            var displayName = _configuration["EmailSettings:DisplayName"];
            var password = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]!);

            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new Exception("Email address is not configured");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Email password is not configured");

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    displayName,
                    emailAddress));

            email.To.Add(
                MailboxAddress.Parse(
                    toEmail));

            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = plainTextContent,
                HtmlBody = htmlContent
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(
                    host,
                    port,
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    emailAddress,
                    password);

                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);

                Console.WriteLine(
                    $"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Email sending failed: {ex.Message}");

                throw;
            }
        }
    }
}
