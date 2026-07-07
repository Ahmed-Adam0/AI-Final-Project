using System;
using System.Threading.Tasks;
using Graduation_Application.DTOs.NotificationDTO;
using Graduation_Application.IServices;
using Graduation_Application.Options;
using Graduation_domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Graduation_Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly WhatsAppNotificationSettings _whatsAppSettings;

        public NotificationService(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IWhatsAppService whatsAppService,
            IOptions<WhatsAppNotificationSettings> whatsAppSettings
        )
        {
            _userManager = userManager;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _whatsAppSettings = whatsAppSettings.Value;
        }

        public Task SendNotificationAsync(string userId, string message)
        {
            Console.WriteLine($"[Notification to User {userId}]: {message}");
            return Task.CompletedTask;
        }

        public async Task SendOrderConfirmationAsync(string userId, int orderId, decimal totalPrice)
        {
            await SendNotificationAsync(
                userId,
                $"Your order #{orderId} has been successfully created!"
            );

            var user = await ResolveUserAsync(userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendOrderCreatedEmailAsync(user.Email, orderId);
            }

            if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await _whatsAppService.SendTemplateAsync(
                    new WhatsAppNotificationRequest
                    {
                        To = user.PhoneNumber,
                        TemplateName = "flow_after_create_order",
                        LanguageCode = _whatsAppSettings.DefaultLanguageCode,
                        BodyParameters = new List<string>
                        {
                            user.FullName,
                            orderId.ToString(),
                            totalPrice.ToString(),
                        },
                    }
                );
            }
        }

        public async Task SendOrderStatusUpdateAsync(string userId, int orderId, string newStatus)
        {
            await SendNotificationAsync(
                userId,
                $"The status for your order #{orderId} has been updated to: {newStatus}."
            );

            var user = await ResolveUserAsync(userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendOrderStatusChangedEmailAsync(
                    user.Email,
                    orderId,
                    newStatus
                );
            }

            if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await _whatsAppService.SendTemplateAsync(
                    new WhatsAppNotificationRequest
                    {
                        To = user.PhoneNumber,
                        TemplateName = "flow_order_status_update",
                        LanguageCode = _whatsAppSettings.DefaultLanguageCode,
                        BodyParameters = new List<string>
                        {
                            user.FullName,
                            orderId.ToString(),
                            newStatus,
                        },
                    }
                );
            }
        }

        public async Task SendOrderCancellationAsync(string userId, int orderId)
        {
            await SendNotificationAsync(
                userId,
                $"Your order #{orderId} has been cancelled successfully."
            );

            var user = await ResolveUserAsync(userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendOrderStatusChangedEmailAsync(
                    user.Email,
                    orderId,
                    "Cancelled"
                );
            }

            //if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            //{
            //    await _whatsAppService.SendTemplateAsync(
            //        new WhatsAppNotificationRequest
            //        {
            //            To = user.PhoneNumber,
            //            TemplateName = _whatsAppSettings.DefaultTemplateName,
            //            LanguageCode = _whatsAppSettings.DefaultLanguageCode,
            //        }
            //    );
            //}

            if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await _whatsAppService.SendTemplateAsync(
                    new WhatsAppNotificationRequest
                    {
                        To = user.PhoneNumber,
                        TemplateName = "flow_order_status_update",
                        LanguageCode = _whatsAppSettings.DefaultLanguageCode,
                        BodyParameters = new List<string>
                        {
                            user.FullName,
                            orderId.ToString(),
                            "Cancelled",
                        },
                    }
                );
            }
        }

        public async Task SendVendorNewOrderAsync(string vendorUserId, int vendorOrderId, string customerName, decimal totalPrice)
        {
            var vendor = await ResolveUserAsync(vendorUserId);

            if (vendor != null && !string.IsNullOrWhiteSpace(vendor.PhoneNumber))
            {
                await _whatsAppService.SendTemplateAsync(
                    new WhatsAppNotificationRequest
                    {
                        To = vendor.PhoneNumber,
                        TemplateName = "flow_vendor_neworder",
                        LanguageCode = _whatsAppSettings.DefaultLanguageCode,
                        BodyParameters = new List<string>
                        {
                            vendor.FullName,
                            vendorOrderId.ToString(),
                            customerName,
                            totalPrice.ToString()
                        },
                    }
                );
            }
        }

        public async Task SendCustomerFirstPaymentAsync(string userId, int orderId, decimal amountPaid)
        {
            var user = await ResolveUserAsync(userId);

            if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await _whatsAppService.SendTemplateAsync(
                    new WhatsAppNotificationRequest
                    {
                        To = user.PhoneNumber,
                        TemplateName = "flow_after_payment",
                        LanguageCode = _whatsAppSettings.DefaultLanguageCode,
                        BodyParameters = new List<string>
                        {
                            user.FullName,
                            orderId.ToString(),
                            amountPaid.ToString()
                        },
                    }
                );
            }
        }

        public async Task SendVendorAfterPaymentAsync(string vendorUserId, int vendorOrderId, decimal amountPaid)
        {
            var vendor = await ResolveUserAsync(vendorUserId);

            if (vendor != null && !string.IsNullOrWhiteSpace(vendor.PhoneNumber))
            {
                await _whatsAppService.SendTemplateAsync(
                    new WhatsAppNotificationRequest
                    {
                        To = vendor.PhoneNumber,
                        TemplateName = "flow_after_payment_vendor",
                        LanguageCode = _whatsAppSettings.DefaultLanguageCode,
                        BodyParameters = new List<string>
                        {
                            vendor.FullName,
                            vendorOrderId.ToString(),
                            amountPaid.ToString()
                        },
                    }
                );
            }
        }

        private async Task<ApplicationUser?> ResolveUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                return user;
            }

            user = await _userManager.FindByEmailAsync(userId);
            if (user != null)
            {
                return user;
            }

            return await _userManager.FindByNameAsync(userId);
        }
    }
}
