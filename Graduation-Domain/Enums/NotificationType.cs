namespace Graduation_domain.Enums
{
    public enum NotificationType
    {
        // Customer
        OrderPending,
        OrderConfirmed,
        OrderInProgress,
        OrderShipped,
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
        DeliveryDateProposed,
        DeliveryDateApproved,
        DeliveryDateRejected,
        MilestoneCreated,
        MilestonePaid,
        VendorMilestonePaid
    }
}

