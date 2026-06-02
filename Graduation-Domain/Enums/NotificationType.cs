namespace Graduation_domain.Enums
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
        VendorAccountRejected,
        VendorAccountSuspended,
        VendorAccountReactivated,
        VendorOrderCancelled,
    }
}

