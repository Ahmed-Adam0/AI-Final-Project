using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graduation_Application.DTOs.Common;
using Graduation_Application.DTOs.NotificationDTO;
using Graduation_Application.IRepositories;
using Graduation_Application.IServices;
using Graduation_domain.Entities;
using Mapster;

namespace Graduation_Application.Services
{
    public class InternalNotificationService : IInternalNotificationService
    {
        private readonly IInternalNotificationRepository _repository;

        public InternalNotificationService(IInternalNotificationRepository repository)
        {
            _repository = repository;
        }

        private static readonly Dictionary<NotificationType, (string TitleAr, string TitleEn, string MessageAr, string MessageEn)> NotificationTemplates 
            = new()
            {
                { 
                    NotificationType.OrderPending, 
                    (
                        "طلب جديد",
                        "New Order",
                        "تم استلام طلبك رقم {0} وهو قيد المراجعة",
                        "Your order #{0} has been received and is pending review"
                    ) 
                },
                { 
                    NotificationType.OrderConfirmed, 
                    (
                        "تم تأكيد الطلب",
                        "Order Confirmed",
                        "تم تأكيد طلبك رقم {0}",
                        "Your order #{0} has been confirmed"
                    ) 
                },
                { 
                    NotificationType.OrderInProgress, 
                    (
                        "جاري تنفيذ الطلب",
                        "Order In Progress",
                        "جاري العمل على طلبك رقم {0}",
                        "Your order #{0} is in progress"
                    ) 
                },
                { 
                    NotificationType.OrderReadyForPickup, 
                    (
                        "الطلب جاهز",
                        "Ready for Pickup",
                        "طلبك رقم {0} جاهز للاستلام",
                        "Your order #{0} is ready for pickup"
                    ) 
                },
                { 
                    NotificationType.OrderDelivered, 
                    (
                        "تم التسليم",
                        "Order Delivered",
                        "تم تسليم طلبك رقم {0} بنجاح",
                        "Your order #{0} has been delivered successfully"
                    ) 
                },
                { 
                    NotificationType.OrderCancelled, 
                    (
                        "تم إلغاء الطلب",
                        "Order Cancelled",
                        "تم إلغاء طلبك رقم {0}",
                        "Your order #{0} has been cancelled"
                    ) 
                },
                { 
                    NotificationType.PasswordReset, 
                    (
                        "تم تغيير كلمة المرور",
                        "Password Reset",
                        "تم إعادة تعيين كلمة المرور بنجاح",
                        "Your password has been reset successfully"
                    ) 
                },
                { 
                    NotificationType.NewOrder, 
                    (
                        "طلب جديد",
                        "New Order",
                        "لديك طلب جديد رقم {0}",
                        "You have a new order #{0}"
                    ) 
                },
                { 
                    NotificationType.NewReview, 
                    (
                        "تقييم جديد",
                        "New Review",
                        "قام عميل بتقييم منتجك",
                        "A customer has reviewed your product"
                    ) 
                },
                { 
                    NotificationType.AccountApproved, 
                    (
                        "تم تفعيل الحساب",
                        "Account Approved",
                        "تم تفعيل حسابك من الإدارة",
                        "Your account has been approved by the admin"
                    ) 
                },
                { 
                    NotificationType.VendorOrderCancelled, 
                    (
                        "تم إلغاء الطلب",
                        "Order Cancelled",
                        "تم إلغاء الطلب رقم {0} من قبل العميل",
                        "The customer has cancelled order #{0}"
                    ) 
                },
            };

        public async Task CreateAsync(string userId, NotificationType type, string? messageParams = null)
        {
            if (!NotificationTemplates.TryGetValue(type, out var template))
            {
                throw new ArgumentException($"Unknown notification type: {type}");
            }

            var (titleAr, titleEn, messageAr, messageEn) = template;

            // Format messages with parameters if provided
            if (!string.IsNullOrWhiteSpace(messageParams))
            {
                messageAr = string.Format(messageAr, messageParams);
                messageEn = string.Format(messageEn, messageParams);
            }

            var notification = new InternalNotification
            {
                UserId = userId,
                TitleAr = titleAr,
                TitleEn = titleEn,
                MessageAr = messageAr,
                MessageEn = messageEn,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
        }

        public async Task<PaginatedResult<InternalNotificationDto>> GetNotificationsAsync(string userId, string lang, int page, int pageSize)
        {
            var totalCount = await _repository.GetTotalCountAsync(userId);
            var notifications = await _repository.GetByUserIdAsync(userId, page, pageSize);

            var dtos = notifications.Select(n =>
            {
                var dto = n.Adapt<InternalNotificationDto>();
                dto.Title = lang == "ar" ? n.TitleAr : n.TitleEn;
                dto.Message = lang == "ar" ? n.MessageAr : n.MessageEn;
                return dto;
            }).ToList();

            return new PaginatedResult<InternalNotificationDto>(dtos, totalCount, page, pageSize);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _repository.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(string userId, int notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new ArgumentException($"Notification with ID {notificationId} not found");
            }

            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("Notification does not belong to this user");
            }

            notification.IsRead = true;
            await _repository.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _repository.GetByUserIdAsync(userId, 1, int.MaxValue);
            var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _repository.SaveChangesAsync();
        }
    }
}
