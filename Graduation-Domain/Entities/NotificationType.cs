namespace Graduation_domain.Entities
{
    public enum NotificationType
    {
        // Customer
        OrderPending,
        OrderConfirmed,
        OrderInProgress,
        OrderReadyForPickup,
        OrderDelivered,
        OrderCancelled,
        PasswordReset,
        // Vendor
        NewOrder,
        NewReview,
        AccountApproved,
        VendorOrderCancelled,
    }
}
