using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_domain.Entities
{
    public class ActivityLog : BaseEntity<int>
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserRole { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EntityId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [NotMapped]
        public string UserRoleAr => UserRole switch
        {
            "SuperAdmin" => "مشرف عام",
            "Customer" => "عميل",
            "Vendor" => "بائع",
            _ => UserRole,
        };

        [NotMapped]
        public string ActionAr => Action switch
        {
            "CreateOrder" => "إنشاء طلب",
            "CancelOrder" => "إلغاء طلب",
            "SuspendUser" => "إيقاف مستخدم",
            "ActivateUser" => "تفعيل مستخدم",
            "DeleteUser" => "حذف مستخدم",
            "DeleteProduct" => "حذف منتج",
            "ApproveVendor" => "قبول بائع",
            "RejectVendor" => "رفض بائع",
            "UpdateOrderStatus" => "تحديث حالة الطلب",
            "CreateReview" => "إضافة تقييم",
            _ => Action,
        };

        [NotMapped]
        public string EntityTypeAr => EntityType switch
        {
            "User" => "مستخدم",
            "Vendor" => "بائع",
            "Product" => "منتج",
            "Order" => "طلب",
            "Review" => "تقييم",
            _ => EntityType,
        };
    }
}
