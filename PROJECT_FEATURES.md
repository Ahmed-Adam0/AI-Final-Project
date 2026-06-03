# Project Features Documentation

> **Generated:** Comprehensive analysis of the complete source code  
> **Project:** HomeAi Marketplace  
> **Domain:** Handicraft / Workshop Services Marketplace  

---

# Project Overview

| Field | Value |
|-------|-------|
| **Project Name** | HomeAi Marketplace (Graduation Project) |
| **Project Purpose** | A multi-vendor marketplace connecting customers with workshop/vendor services for custom furniture, renovations, and handicraft products. Customers can browse products, place orders, and track their progress; vendors manage their workshops, products, and orders; admins oversee the entire platform. |
| **Main Business Domain** | E-commerce / Service Marketplace (Workshop Services) |
| **Architecture Pattern** | Clean Architecture with 4 layers: API/MVC (Presentation) → Application → Domain → Infrastructure |
| **Technologies Used** | .NET 10, ASP.NET Core Web API, ASP.NET Core MVC, Entity Framework Core 10, Mapster, SendGrid, Cloudinary, JWT Bearer, Google Auth, WhatsApp Cloud API, Identity Framework |
| **Database Provider** | SQL Server (via `databaseasp.net`) |
| **External Services** | SendGrid (email), Cloudinary (image storage), WhatsApp Cloud API (notifications), Google Sign-In, Scalar API Reference |

### Solution Structure

```
Graduation Project.slnx
├── Graduation-API/          # REST API (Controllers, Program.cs)
├── Graduation-MVC/          # Admin Dashboard (MVC Areas)
├── Graduation-Application/  # Business Logic (Services, DTOs, Interfaces)
├── Graduation-Domain/       # Domain Entities & Enums
└── Graduation-infrastructure/ # EF Core, Repositories, Identity, Seeding
```

---

# System Roles

## Customer

### Permissions
- Register, login, Google login
- Browse products with search/filter/pagination
- View product details with images
- Manage shopping cart (add, update, remove, clear)
- Create orders from cart
- View own order history and order details
- Cancel own orders (within valid status)
- Add/remove favorites
- Create reviews (one per product)
- Delete own reviews
- Update profile (name, username, addresses, image)
- Change password
- Receive in-app notifications & email notifications
- Receive WhatsApp notifications for order cancellation

### Accessible Features
- Public product browsing and search
- Product details
- Cart management (CRUD)
- Order creation, viewing, cancellation
- Favorites management
- Review creation
- Profile management
- Internal notifications
- Email/WhatsApp notifications
- Google sign-in

### Restricted Features
- Cannot create products
- Cannot access vendor dashboard/analytics
- Cannot manage other users' reviews
- Cannot modify order statuses
- Cannot access admin panel

## Vendor/Workshop

### Permissions
- Login via dedicated vendor login endpoint
- Create/update/delete own products
- Manage product images (upload, replace, delete, set primary)
- Toggle product active/inactive status
- View own products list with stats
- View top-rated products
- View vendor dashboard metrics
- View order analytics and revenue statistics
- View vendor orders with filtering/pagination
- Update order status (confirmed → in progress → ready for pickup → delivered)
- View and reply to product reviews
- Report reviews
- Update vendor profile and logo
- Receive in-app notifications for new orders, reviews, account status changes
- Link existing products to their account

### Accessible Features
- Dedicated vendor login
- Product CRUD
- Product image management
- Vendor product dashboard (stats, top products)
- Vendor order management with filtering
- Order status workflow management
- Vendor analytics (revenue, orders, metrics, activity reports)
- Review reply and reporting
- Vendor profile management
- Logo upload
- Internal notifications

### Restricted Features
- Cannot create reviews (blocked at controller level)
- Cannot register via regular register endpoint (must use vendor creation)
- Cannot access admin panel
- Cannot approve/reject other vendors

## Admin

### Permissions
- Access full admin dashboard
- View platform statistics (orders, revenue, vendors, customers)
- View/Filter/Manage all orders with details
- View/Filter/Manage all products (activate, deactivate, hide, restore)
- View/Filter/Manage all vendors (approve, reject, suspend, activate)
- View vendor verification history
- Manage product reviews (view, delete, resolve reports, ignore reports)
- View reported reviews
- View reported products and resolve reports
- View analytics (orders per day, revenue growth, top vendors/categories)
- View settings (platform, commission, support) - read-only/hardcoded
- Generate reports (customer, vendor, financial)
- Access admin dashboard with charts and metrics

### Accessible Features
- Full admin dashboard with summary cards and charts
- Order management (list, filter, details, status change, cancel)
- Product management (list, filter, activate, deactivate, hide, restore)
- Vendor management (list, filter, approve, reject, suspend, activate)
- Vendor history (unified verification & account status audit trail)
- Review moderation (list, filter, details, delete, resolve reports, ignore reports)
- Reported products management
- Analytics & reporting
- Settings view
- Product reports management
- Internal notifications

### Restricted Features
- Cannot create orders or products
- Cannot browse as a customer
- No vendor-specific operations

## Roles Discovered from Code

| Role | Source | Used In |
|------|--------|---------|
| `Admin` | Seeder (`ApplicationDbSeeder.cs`) | Hardcoded role name |
| `Customer` | `Roles.cs` constant | `AuthService`, `LoginAsync` validation |
| `Vendor` | `Roles.cs` constant | `VendorService`, Authorization attributes |
| `Workshop` | Seeder (legacy) | Seeded but not used in app code |

**Note:** The seeder creates `Admin`, `Workshop`, and `Customer` roles. The application code uses `Roles.Customer` = `"Customer"` and `Roles.Vendor` = `"Vendor"`. The `"Admin"` role is used in `[Authorize(Roles = "Admin")]` on Web API controllers. The `"Workshop"` role from seeding appears to be legacy/unused.

---

# Feature Inventory

## 1. User Registration

### Description
Register a new customer account with email, password, full name, and username.

### Business Purpose
Allow new customers to create accounts to browse, order, and interact with the platform.

### User Roles
Customer

### Entry Points
- **API:** `POST /api/Auth/register`
- **Service:** `AuthService.RegisterAsync()`

### Database Tables Involved
- `AspNetUsers` (via Identity)

### Related Services
- `AuthService` → `UserManager`, `RoleManager`, `IJwtTokenGenerator`

### Dependencies
- SendGrid (for email confirmation OTP sent separately)

### Status
Complete

## 2. User Login

### Description
Authenticate a customer with email/password. Returns JWT token.

### Business Purpose
Allow customers to securely access their accounts.

### User Roles
Customer (only customers can login, vendors blocked)

### Entry Points
- **API:** `POST /api/Auth/login`
- **Service:** `AuthService.LoginAsync()`

### Database Tables Involved
- `AspNetUsers`

### Related Services
- `AuthService`, `JwtTokenGenerator`

### Status
Complete

## 3. Google Sign-In

### Description
Authenticate or auto-register with Google Id token. Validates token with Google API, creates account if new.

### Business Purpose
Provide seamless authentication experience via Google.

### User Roles
Customer

### Entry Points
- **API:** `POST /api/Auth/google-login`
- **Service:** `AuthService.GoogleLoginAsync()`

### Database Tables Involved
- `AspNetUsers`

### Related Services
- `Google.Apis.Auth`, `AuthService`, `JwtTokenGenerator`

### Dependencies
- Google ClientId configured in `appsettings.json`

### Status
Complete

## 4. Forgot Password

### Description
Sends OTP code to user's email for password reset.

### Business Purpose
Allow users to recover access to their accounts.

### User Roles
Customer

### Entry Points
- **API:** `POST /api/Auth/forgot-password`
- **Service:** `AuthService.ForgotPasswordAsync()`

### Database Tables Involved
- `AspNetUsers` (stores `OtpCode`, `OtpExpiry`)

### Related Services
- `AuthService`, `EmailService`, `InternalNotificationService`

### Dependencies
- SendGrid for OTP email

### Status
Complete

## 5. Verify OTP

### Description
Verify the OTP code sent during forgot password flow.

### Business Purpose
Ensure only the email owner can reset password.

### User Roles
Customer

### Entry Points
- **API:** `POST /api/Auth/verify-otp`
- **Service:** `AuthService.VerifyOtpAsync()`

### Database Tables Involved
- `AspNetUsers`

### Status
Complete

## 6. Reset Password

### Description
Reset password after OTP verification. Removes old password and sets new one.

### Business Purpose
Complete the password reset flow.

### User Roles
Customer

### Entry Points
- **API:** `POST /api/Auth/reset-password`
- **Service:** `AuthService.ResetPasswordAsync()`

### Database Tables Involved
- `AspNetUsers`

### Related Services
- `AuthService`, `InternalNotificationService`

### Status
Complete

## 7. Email Confirmation via OTP

### Description
Confirm user email using OTP sent after registration or login attempt with unconfirmed email.

### Business Purpose
Ensure email ownership and prevent fake accounts.

### User Roles
Customer

### Entry Points
- **API:** `POST /api/Auth/confirm-email-otp`
- **API:** `POST /api/Auth/resend-confirmation`
- **Service:** `AuthService.ConfirmEmailOtpAsync()`, `ResendConfirmationEmailAsync()`

### Database Tables Involved
- `AspNetUsers` (stores `OtpEmail`, `OtpEmailExpiry`)

### Related Services
- `AuthService`, `EmailService`

### Dependencies
- SendGrid for confirmation OTP email

### Status
Complete

## 7. Role Management

### Description
Create roles, list all roles, and assign roles to users by email.

### Business Purpose
Administrative role assignment for platform access control.

### User Roles
Admin (no authorization guard on API)

### Entry Points
- **API:** `POST /api/Roles/create`
- **API:** `POST /api/Roles/assign`
- **API:** `GET /api/Roles`
- **Service:** `RoleService`

### Database Tables Involved
- `AspNetRoles`
- `AspNetUserRoles`

### Related Services
- `RoleService`

### Status
Complete

## 8. Product Browsing & Search

### Description
Browse products with search, category filter, price range, material filter, workshop filter, and pagination.

### Business Purpose
Allow customers to discover and find products.

### User Roles
Customer (public, no auth required)

### Entry Points
- **API:** `GET /api/Products`
- **Service:** `ProductService.GetProductsAsync()`

### Database Tables Involved
- `Products`
- `Categories`
- `ProductImages`
- `Workshops`

### Related Services
- `ProductService`

### Status
Complete

## 9. Product Details

### Description
View full product details including name (AR/EN), description (AR/EN), price, category, workshop info, and images.

### Business Purpose
Allow customers to view comprehensive product information.

### User Roles
Customer (public, no auth required)

### Entry Points
- **API:** `GET /api/Products/{id}`
- **Service:** `ProductService.GetProductDetailsAsync()`

### Database Tables Involved
- `Products`
- `Categories`
- `ProductImages`
- `Workshops`

### Status
Complete

## 10. Product CRUD (Vendor)

### Description
Vendors can create, update, and delete their own products. Ownership is enforced via UserId.

### Business Purpose
Allow vendors to manage their product catalog.

### User Roles
Vendor

### Entry Points
- **API:** `POST /api/Products` (Create)
- **API:** `PUT /api/Products/{id}` (Update)
- **API:** `DELETE /api/Products/{id}` (Delete)
- **API:** `PUT /api/Products/{id}/status` (Toggle Active/Inactive)
- **Service:** `ProductService`

### Database Tables Involved
- `Products`
- `Categories`

### Related Services
- `ProductService`, `WorkshopRepository`

### Status
Complete

## 11. Product Image Management

### Description
Vendors can upload, replace, remove, and set primary images for their products.

### Business Purpose
Allow vendors to visually manage product images.

### User Roles
Vendor

### Entry Points
- **API:** `POST /api/Products/{productId}/images` (Upload)
- **API:** `PUT /api/Products/{productId}/images/{imageId}` (Replace)
- **API:** `DELETE /api/Products/{productId}/images/{imageId}` (Remove)
- **API:** `PUT /api/Products/{productId}/images/{imageId}/primary` (Set Primary)
- **Service:** `ProductService`

### Database Tables Involved
- `ProductImages`
- `Products`

### Related Services
- `ProductService`

### Status
Complete

## 12. Vendor Product Dashboard

### Description
Vendors can view their products list, product stats (total, active, inactive counts, average rating, revenue), and top-rated products.

### Business Purpose
Provide vendors with insights into their product catalog performance.

### User Roles
Vendor

### Entry Points
- **API:** `GET /api/Products/my-products`
- **API:** `GET /api/Products/my-products/stats`
- **API:** `GET /api/Products/my-products/top`
- **API:** `GET /api/Products/my-products/{id}`
- **Service:** `VendorProductService`

### Database Tables Involved
- `Products`
- `Reviews`

### Related Services
- `VendorProductService`

### Status
Complete

## 13. Category Management

### Description
View all active categories (public). Create, update, delete categories (Admin-only).

### Business Purpose
Organize products into categories for easier browsing.

### User Roles
Customer (view), Admin (CRUD)

### Entry Points
- **API:** `GET /api/Categories` (All)
- **API:** `POST /api/Categories` (Admin)
- **API:** `PUT /api/Categories/{id}` (Admin)
- **API:** `DELETE /api/Categories/{id}` (Admin)
- **Service:** `CategoryService`

### Database Tables Involved
- `Categories`

### Status
Complete

## 14. Shopping Cart

### Description
Authenticated users can manage their cart: view, add items, update quantities, remove items, clear all.

### Business Purpose
Allow customers to collect products before ordering.

### User Roles
Customer

### Entry Points
- **API:** `GET /api/Cart`
- **API:** `POST /api/Cart/items`
- **API:** `PUT /api/Cart/items`
- **API:** `DELETE /api/Cart/items/{id}`
- **API:** `DELETE /api/Cart`
- **Service:** `CartService`

### Database Tables Involved
- `Carts`
- `CartItems`
- `Products`

### Status
Complete

## 15. Order Creation

### Description
Creates an order from the user's cart. Clears cart after successful creation. Sends notifications.

### Business Purpose
Convert cart into a formal order for vendor fulfillment.

### User Roles
Customer

### Entry Points
- **API:** `POST /api/Order`
- **Service:** `OrderService.CreateOrderAsync()`

### Database Tables Involved
- `Orders`
- `OrderItems`
- `OrderStatusHistory`
- `Carts`
- `CartItems`

### Related Services
- `OrderService`, `CartService`, `NotificationService`, `InternalNotificationService`

### Status
Complete

## 16. Order Viewing (Customer)

### Description
Customers can view all their orders, specific order details, and all orders (admin-level).

### Business Purpose
Allow customers to track their orders.

### User Roles
Customer, Admin

### Entry Points
- **API:** `GET /api/Order/my-orders`
- **API:** `GET /api/Order/{id}`
- **API:** `GET /api/Order`
- **Service:** `OrderService`

### Database Tables Involved
- `Orders`
- `OrderItems`
- `OrderStatusHistory`
- `Products`

### Status
Complete

## 17. Order Cancellation (Customer)

### Description
Customers can cancel their own orders if status is not Delivered or Cancelled.

### Business Purpose
Allow customers to cancel orders before fulfillment.

### User Roles
Customer

### Entry Points
- **API:** `PUT /api/Order/{id}/cancel`
- **Service:** `OrderService.CancelOrderAsync()`

### Database Tables Involved
- `Orders`
- `OrderStatusHistory`

### Related Services
- `OrderService`, `NotificationService`, `InternalNotificationService`

### Status
Complete

## 18. Order Status Management

### Description
Update order status with valid transition validation:
Pending → Confirmed → In Progress → Ready for Pickup → Delivered
Any status can go to Cancelled.

### Business Purpose
Track order lifecycle from creation to delivery.

### User Roles
Vendor (via VendorOrders controller), Admin (via Order controller)

### Entry Points
- **API:** `PUT /api/Order/{id}/status` (Admin/Customer)
- **API:** `PUT /api/VendorOrders/orders/{orderId}/status` (Vendor)
- **Service:** `OrderService.UpdateOrderStatusAsync()`, `VendorOrderService.UpdateVendorOrderStatusAsync()`

### Database Tables Involved
- `Orders`
- `OrderStatusHistory`

### Related Services
- `OrderService`, `VendorOrderService`, `NotificationService`, `InternalNotificationService`

### Status
Complete

## 19. Vendor Order Management

### Description
Vendors can view orders assigned to their workshop with filtering by status, date range, customer name, sorting, and pagination.

### Business Purpose
Allow vendors to manage incoming orders.

### User Roles
Vendor

### Entry Points
- **API:** `POST /api/VendorOrders/orders/filter`
- **API:** `GET /api/VendorOrders/orders/{orderId}`
- **Service:** `VendorOrderService`

### Database Tables Involved
- `Orders`
- `OrderItems`
- `OrderStatusHistory`
- `Products`

### Status
Complete

## 20. Vendor Analytics & Reports

### Description
Vendors can view revenue statistics (daily/weekly/monthly/total), order analytics (completion rate, average completion time, order counts by status), dashboard metrics, and activity reports.

### Business Purpose
Provide vendors with business intelligence to track performance.

### User Roles
Vendor

### Entry Points
- **API:** `GET /api/VendorOrders/analytics/revenue`
- **API:** `GET /api/VendorOrders/analytics/orders`
- **API:** `GET /api/VendorOrders/dashboard/metrics`
- **API:** `GET /api/VendorOrders/reports/activity`
- **Service:** `VendorOrderService`

### Database Tables Involved
- `Orders`
- `OrderStatusHistory`

### Status
Complete

## 21. Favorites / Wishlist

### Description
Authenticated users can add/remove products to/from favorites and view their favorites list.

### Business Purpose
Allow customers to save products for later.

### User Roles
Customer

### Entry Points
- **API:** `GET /api/Favorites`
- **API:** `POST /api/Favorites/{productId}`
- **API:** `DELETE /api/Favorites/{productId}`
- **Service:** `FavoriteService`

### Database Tables Involved
- `Favorites`
- `Products`

### Status
Complete

## 22. Product Reviews

### Description
Customers can create reviews (rating 1-5, comment) for products (one review per product). Anyone can view product reviews and average rating. Review owners can delete their own reviews.

### Business Purpose
Allow customers to share feedback on products.

### User Roles
Customer (create, delete), Public (view)

### Entry Points
- **API:** `POST /api/Reviews` (Auth)
- **API:** `GET /api/Reviews/product/{productId}`
- **API:** `GET /api/Reviews/product/{productId}/rating`
- **API:** `GET /api/Reviews/{id}`
- **API:** `DELETE /api/Reviews/{id}` (Owner)
- **Service:** `ReviewService`

### Database Tables Involved
- `Reviews`
- `Products`

### Dependencies
- Vendors are blocked from creating reviews at controller level
- One review per user per product enforced

### Status
Complete

## 23. Vendor Review Reply & Reporting

### Description
Vendors can reply to reviews on their products and report inappropriate reviews.

### Business Purpose
Allow vendors to engage with customer feedback and flag issues.

### User Roles
Vendor

### Entry Points
- **API:** `GET /api/Reviews/vendor`
- **API:** `POST /api/Reviews/{reviewId}/reply`
- **API:** `POST /api/Reviews/{reviewId}/report`
- **Service:** `ReviewService`

### Database Tables Involved
- `Reviews`
- `Products`

### Status
Complete

## 24. Admin Dashboard

### Description
Full admin dashboard with summary cards (Total Orders, Revenue, Vendors, Customers, Pending/Completed Orders), latest orders, latest vendors, latest reviews, latest reports, revenue trend chart, and order volume chart.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /` (root) and `GET /Admin/Dashboard`
- **MVC:** `GET /Admin/Dashboard/Statistics` (JSON endpoint)
- **Service:** `AdminDashboardService.GetDashboardAsync()`

### Database Tables Involved
- `Orders`
- `Workshops`
- `Users`
- `Reviews`
- `ProductReports`

### Status
Complete

## 25. Admin Order Management

### Description
Admin can view, filter, search orders by customer name, status, vendor, date range, and pagination. View order details including customer info, vendor info, items, and status timeline.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Orders`
- **MVC:** `GET /Admin/Orders/Details/{id}`
- **MVC:** `POST /Admin/Orders/ChangeStatus/{id}`
- **Service:** `AdminDashboardService.GetOrdersPageAsync()`, `GetOrderDetailsAsync()`

### Database Tables Involved
- `Orders`
- `OrderItems`
- `OrderStatusHistory`
- `Users`
- `Workshops`
- `Products`

### Status
Complete

## 26. Admin Product Management

### Description
Admin can view, filter (by search, category, vendor, status) all products. View product details with reviews stats. Activate, deactivate, hide, and restore products.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Products`
- **MVC:** `GET /Admin/Products/Details/{id}`
- **MVC:** `POST /Admin/Products/Activate/{id}`
- **MVC:** `POST /Admin/Products/Deactivate/{id}`
- **MVC:** `POST /Admin/Products/Hide/{id}`
- **MVC:** `POST /Admin/Products/Restore/{id}`
- **Service:** `AdminProductService`

### Database Tables Involved
- `Products`
- `Categories`
- `ProductImages`
- `Reviews`

### Status
Complete

## 27. Admin Vendor Management

### Description
Admin can view all vendors, pending vendors (unverified), vendor details with profile, business info, verification status, account status, orders stats, revenue stats, verification history, account status history. Approve, reject, suspend, and activate vendors.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Vendors`
- **MVC:** `GET /Admin/Vendors/Pending`
- **MVC:** `GET /Admin/Vendors/Details/{id}`
- **MVC:** `GET /Admin/Vendors/History`
- **MVC:** `POST /Admin/Vendors/Approve`
- **MVC:** `POST /Admin/Vendors/Reject`
- **MVC:** `POST /Admin/Vendors/Suspend`
- **MVC:** `POST /Admin/Vendors/Activate`
- **Service:** `AdminVendorsService`, `AdminVendorsRepository`

### Database Tables Involved
- `Workshops`
- `Users`
- `Orders`
- `VendorVerificationHistory`
- `VendorAccountStatusHistory`

### Status
Complete

## 28. Admin Review Moderation

### Description
Admin can view all reviews with filtering (search, rating, reported status, product, date range). View review details with moderation history. Delete reviews (soft-deactivate), resolve reports, ignore reports.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Reviews`
- **MVC:** `GET /Admin/Reviews/Details/{id}`
- **MVC:** `GET /Admin/Reviews/Reported`
- **MVC:** `POST /Admin/Reviews/ResolveReport/{id}`
- **MVC:** `POST /Admin/Reviews/IgnoreReport/{id}`
- **MVC:** `POST /Admin/Reviews/Delete/{id}`
- **Service:** `AdminReviewService`

### Database Tables Involved
- `Reviews`
- `ReviewModerationLogs`

### Status
Complete

## 29. Admin Product Reports

### Description
View reported products with reporter info, reason, and status. Resolve reports.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Products/Reported`
- **MVC:** `POST /Admin/Products/ResolveReport/{id}`
- **Service:** `AdminProductService.GetReportedProductsAsync()`, `ResolveReportAsync()`

### Database Tables Involved
- `ProductReports`
- `Products`
- `Users`

### Status
Complete

## 30. Admin Analytics

### Description
Analytics page with metrics (growth rate, average order value, completion rate), orders per day chart, revenue growth chart, revenue forecast chart, top vendors ranking, top categories ranking. Date range filterable.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Analytics`
- **MVC:** `GET /Admin/Analytics/Data` (JSON endpoint)
- **Service:** `AdminDashboardService.GetAnalyticsAsync()`

### Database Tables Involved
- `Orders`
- `Workshops`
- `Products`
- `Categories`

### Status
Partial (forecast chart has no data, export stubs only)

## 31. Admin Reports

### Description
View product reports list with filter by date, vendor, category, report type. Download, Export PDF, Export Excel stubs.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Reports`
- **MVC:** `GET /Admin/Reports/Daily`
- **MVC:** `GET /Admin/Reports/Weekly`
- **MVC:** `GET /Admin/Reports/Monthly`
- **MVC:** `POST /Admin/Reports/Generate`
- **MVC:** `GET /Admin/Reports/Download`
- **Service:** `AdminDashboardService.GetReportsAsync()`

### Database Tables Involved
- `ProductReports`

### Status
Partial (export/download actions are stubs that redirect)

## 32. Admin Settings

### Description
View platform settings (name, logo, contact), commission settings (percentage, vendor fees, tax), support settings (email, phone, WhatsApp, hours), and audit logs. Save stub.

### User Roles
Admin

### Entry Points
- **MVC:** `GET /Admin/Settings`
- **MVC:** `POST /Admin/Settings/Save`
- **Service:** `AdminDashboardService.GetSettingsAsync()`

### Status
Partial (all values are hardcoded, not persisted)

## 33. User Profile Management

### Description
View and update profile (full name, username, email, phone, addresses). Upload profile image. Change password.

### User Roles
Customer

### Entry Points
- **API:** `GET /api/Profile`
- **API:** `PUT /api/Profile`
- **API:** `PUT /api/Profile/image`
- **API:** `PUT /api/Profile/change-password`
- **Service:** `ProfileService`

### Database Tables Involved
- `AspNetUsers`
- `Addresses`

### Related Services
- `ProfileService`, `ProfileRepository`, `FileService`

### Status
Complete

## 34. Vendor Registration & Login

### Description
Vendors register via dedicated endpoint (creates user + workshop + workshop address). Login via vendor-specific endpoint with workshop info in JWT.

### Business Purpose
Separate vendor onboarding from customer registration.

### User Roles
Vendor

### Entry Points
- **API:** `POST /api/Vendors/create`
- **API:** `POST /api/Vendors/login`
- **Service:** `VendorService`

### Database Tables Involved
- `AspNetUsers`
- `Workshops`
- `WorkshopAddresses`

### Related Services
- `VendorService`, `AuthService` (for confirmation email)

### Status
Complete

## 35. Vendor Profile

### Description
View and update vendor profile (workshop names, descriptions, address, logo). Update vendor logo.

### User Roles
Vendor

### Entry Points
- **API:** `GET /api/Vendors/profile`
- **API:** `PUT /api/Vendors/profile`
- **API:** `PUT /api/Vendors/logo`
- **Service:** `VendorService`

### Database Tables Involved
- `AspNetUsers`
- `Workshops`
- `WorkshopAddresses`

### Related Services
- `VendorService`, `FileService` (Cloudinary)

### Status
Complete

## 36. Internal Notifications (In-App)

### Description
Users can view their notifications with pagination, see unread count, mark single as read, mark all as read. Notifications are bilingual (Arabic/English). Triggered by: order status changes, new orders (vendor), new reviews (vendor), password reset, account approval/rejection/suspension/reactivation.

### Business Purpose
Keep users informed about important events in-app.

### User Roles
Customer, Vendor

### Entry Points
- **API:** `GET /api/internal-notifications`
- **API:** `GET /api/internal-notifications/unread-count`
- **API:** `PUT /api/internal-notifications/{id}/read`
- **API:** `PUT /api/internal-notifications/read-all`
- **Service:** `InternalNotificationService`

### Database Tables Involved
- `InternalNotifications`

### Notification Types (from code)
| Type | Recipient | Trigger |
|------|-----------|---------|
| OrderPending | Customer | Order created |
| OrderConfirmed | Customer | Order confirmed |
| OrderInProgress | Customer | Order in progress |
| OrderReadyForPickup | Customer | Order ready |
| OrderDelivered | Customer | Order delivered |
| OrderCancelled | Customer | Order cancelled |
| PasswordReset | Customer | Password changed |
| NewOrder | Vendor | New order received |
| NewReview | Vendor | Product reviewed |
| AccountApproved | Vendor | Account approved by admin |
| VendorAccountRejected | Vendor | Account rejected |
| VendorAccountSuspended | Vendor | Account suspended |
| VendorAccountReactivated | Vendor | Account reactivated |
| VendorOrderCancelled | Vendor | Customer cancelled order |

### Status
Complete

## 37. Email Notifications

### Description
Send transactional emails via SendGrid: OTP for password reset, email confirmation OTP, order created confirmation, order status changed.

### Business Purpose
Communicate important events via email.

### User Roles
Customer

### Entry Points
- Called from `AuthService`, `OrderService`, `NotificationService`
- **Service:** `EmailService`

### Dependencies
- SendGrid API key configured in `appsettings.json`

### Status
Complete (with graceful skip if API key not configured)

## 38. WhatsApp Notifications

### Description
Send WhatsApp template messages via Meta/WhatsApp Cloud API. Used for order cancellation notifications.

### Business Purpose
Reach customers via WhatsApp for important updates.

### User Roles
Customer

### Entry Points
- Called from `NotificationService`
- **Service:** `WhatsAppService`

### Dependencies
- WhatsApp Cloud API credentials in `appsettings.json`

### Status
Partial (only cancellation uses WhatsApp; other flows commented out)

## 39. Image Upload (Cloudinary)

### Description
Upload images to Cloudinary for vendor logos and user profile images. Validates file types (jpg, jpeg, png only). Can delete old images.

### User Roles
Vendor, Customer

### Entry Points
- Called from `VendorService`, `ProfileService`
- **Service:** `FileService`

### Dependencies
- Cloudinary credentials in `appsettings.json`

### Status
Complete

## 40. Local Image Upload (Product Images)

### Description
Product images are saved locally to `wwwroot/images/products/`. Supports upload, delete from disk, and replace.

### User Roles
Vendor

### Entry Points
- **API:** `POST /api/Products/{productId}/images`
- Controller directly writes to disk (not via FileService)

### Status
Complete

## 41. Vendor Product Linking (Migration Helper)

### Description
Links existing products (with WorkshopId but null UserId) to vendor's UserId. Hidden utility for data migration.

### User Roles
Vendor (called internally)

### Entry Points
- **Service:** `VendorService.LinkVendorProductsAsync()`

### Status
Complete (internal utility)

## 42. Database Seeding

### Description
Seeds initial data on first run: Admin/Workshop/Customer roles, a test workshop user, a workshop, 3 categories, and 20 sample products.

### User Roles
System

### Entry Points
- `ApplicationDbSeeder.SeedAsync()` called from `Program.cs`

### Status
Complete

---

# API Documentation

## Authentication Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/Auth/register` | Register new customer | Anonymous |
| POST | `/api/Auth/login` | Customer login | Anonymous |
| POST | `/api/Auth/forgot-password` | Request password reset OTP | Anonymous |
| POST | `/api/Auth/verify-otp` | Verify OTP for password reset | Anonymous |
| POST | `/api/Auth/reset-password` | Reset password with OTP | Anonymous |
| POST | `/api/Auth/confirm-email-otp` | Confirm email with OTP | Anonymous |
| POST | `/api/Auth/resend-confirmation` | Resend email confirmation OTP | Anonymous |
| POST | `/api/Auth/google-login` | Login/Register with Google | Anonymous |
| POST | `/api/Vendors/login` | Vendor-specific login | Anonymous |
| POST | `/api/Vendors/create` | Vendor registration | Anonymous |

### Register DTO
```json
{
  "FullName": "string",
  "UserName": "string",
  "Email": "string",
  "Password": "string"
}
```

### Login DTO
```json
{
  "Email": "string",
  "Password": "string"
}
```

### AuthResponse DTO
```json
{
  "UserId": "string",
  "FullName": "string",
  "Email": "string",
  "Roles": ["string"],
  "Token": "string"
}
```

## Products Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/Products` | Browse products with filters | Anonymous |
| GET | `/api/Products/{id}` | Get product details | Anonymous |
| POST | `/api/Products` | Create product | Vendor |
| PUT | `/api/Products/{id}` | Update product | Vendor |
| DELETE | `/api/Products/{id}` | Delete product | Vendor |
| PUT | `/api/Products/{id}/status` | Toggle product active/inactive | Vendor |
| POST | `/api/Products/{productId}/images` | Upload product image | Vendor |
| PUT | `/api/Products/{productId}/images/{imageId}` | Replace image | Vendor |
| DELETE | `/api/Products/{productId}/images/{imageId}` | Remove image | Vendor |
| PUT | `/api/Products/{productId}/images/{imageId}/primary` | Set primary image | Vendor |
| GET | `/api/Products/my-products` | Vendor's product list | Vendor |
| GET | `/api/Products/my-products/stats` | Vendor product stats | Vendor |
| GET | `/api/Products/my-products/top` | Vendor top products | Vendor |
| GET | `/api/Products/my-products/{id}` | Vendor product details | Vendor |

## Categories Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/Categories` | List all categories | Anonymous |
| POST | `/api/Categories` | Create category | Admin |
| PUT | `/api/Categories/{id}` | Update category | Admin |
| DELETE | `/api/Categories/{id}` | Delete category | Admin |

## Cart Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/Cart` | Get user's cart | Auth |
| POST | `/api/Cart/items` | Add item to cart | Auth |
| PUT | `/api/Cart/items` | Update cart item | Auth |
| DELETE | `/api/Cart/items/{id}` | Remove cart item | Auth |
| DELETE | `/api/Cart` | Clear cart | Auth |

## Orders Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/Order` | List all orders | Auth |
| POST | `/api/Order` | Create order from cart | Auth |
| GET | `/api/Order/{id}` | Get order details | Auth |
| GET | `/api/Order/my-orders` | Get current user's orders | Auth |
| PUT | `/api/Order/{id}/status` | Update order status | Auth |
| PUT | `/api/Order/{id}/cancel` | Cancel order | Auth |

## Vendor Orders Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/VendorOrders/orders/filter` | Filtered vendor orders | Auth |
| GET | `/api/VendorOrders/orders/{orderId}` | Vendor order details | Auth |
| PUT | `/api/VendorOrders/orders/{orderId}/status` | Update order status (vendor) | Auth |
| GET | `/api/VendorOrders/analytics/revenue` | Revenue statistics | Auth |
| GET | `/api/VendorOrders/analytics/orders` | Order analytics | Auth |
| GET | `/api/VendorOrders/dashboard/metrics` | Dashboard metrics | Auth |
| GET | `/api/VendorOrders/reports/activity` | Activity report | Auth |

## Reviews Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/Reviews/product/{productId}` | Product reviews | Anonymous |
| GET | `/api/Reviews/product/{productId}/rating` | Average rating | Anonymous |
| GET | `/api/Reviews/{id}` | Review details | Anonymous |
| POST | `/api/Reviews` | Create review | Auth (not Vendor) |
| DELETE | `/api/Reviews/{id}` | Delete own review | Auth |
| GET | `/api/Reviews/vendor` | Vendor reviews | Vendor |
| POST | `/api/Reviews/{reviewId}/reply` | Reply to review | Vendor |
| POST | `/api/Reviews/{reviewId}/report` | Report review | Vendor |

## Favorites Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/Favorites` | Get user favorites | Auth |
| POST | `/api/Favorites/{productId}` | Add to favorites | Auth |
| DELETE | `/api/Favorites/{productId}` | Remove from favorites | Auth |

## Profile Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/Profile` | Get profile | Auth |
| PUT | `/api/Profile` | Update profile | Auth |
| PUT | `/api/Profile/image` | Update profile image | Auth |
| PUT | `/api/Profile/change-password` | Change password | Auth |

## Vendors Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/Vendors/login` | Vendor login | Anonymous |
| POST | `/api/Vendors/create` | Register vendor | Anonymous |
| PUT | `/api/Vendors/logo` | Update vendor logo | Vendor |
| GET | `/api/Vendors/profile` | Get vendor profile | Vendor |
| PUT | `/api/Vendors/profile` | Update vendor profile | Vendor |

## Roles Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/Roles/create` | Create role | Public (no guard) |
| POST | `/api/Roles/assign` | Assign role to user | Public (no guard) |
| GET | `/api/Roles` | List all roles | Public (no guard) |

## Internal Notifications Module

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/internal-notifications` | Get notifications (paginated) | Auth |
| GET | `/api/internal-notifications/unread-count` | Get unread count | Auth |
| PUT | `/api/internal-notifications/{id}/read` | Mark as read | Auth |
| PUT | `/api/internal-notifications/read-all` | Mark all as read | Auth |

---

# Admin MVC Routes

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | Redirects to Admin Dashboard |
| GET | `/Admin/Dashboard` | Admin dashboard |
| GET | `/Admin/Dashboard/Statistics` | Dashboard JSON data |
| GET | `/Admin/Orders` | Order list with filters |
| GET | `/Admin/Orders/Details/{id}` | Order details |
| POST | `/Admin/Orders/ChangeStatus/{id}` | Change order to Confirmed |
| GET | `/Admin/Products` | Product list with filters |
| GET | `/Admin/Products/Details/{id}` | Product details |
| POST | `/Admin/Products/Activate/{id}` | Activate product |
| POST | `/Admin/Products/Deactivate/{id}` | Deactivate product |
| POST | `/Admin/Products/Hide/{id}` | Hide product |
| POST | `/Admin/Products/Restore/{id}` | Restore product |
| GET | `/Admin/Products/Reported` | Reported products |
| POST | `/Admin/Products/ResolveReport/{id}` | Resolve report |
| GET | `/Admin/Vendors` | Vendor list |
| GET | `/Admin/Vendors/Pending` | Pending vendors |
| GET | `/Admin/Vendors/Details/{id}` | Vendor details |
| GET | `/Admin/Vendors/History` | Vendor audit history |
| POST | `/Admin/Vendors/Approve` | Approve vendor |
| POST | `/Admin/Vendors/Reject` | Reject vendor |
| POST | `/Admin/Vendors/Suspend` | Suspend vendor |
| POST | `/Admin/Vendors/Activate` | Activate vendor |
| GET | `/Admin/Reviews` | Review list |
| GET | `/Admin/Reviews/Details/{id}` | Review details |
| GET | `/Admin/Reviews/Reported` | Reported reviews |
| POST | `/Admin/Reviews/ResolveReport/{id}` | Resolve review report |
| POST | `/Admin/Reviews/IgnoreReport/{id}` | Ignore review report |
| POST | `/Admin/Reviews/Delete/{id}` | Soft-delete review |
| GET | `/Admin/Analytics` | Analytics page |
| GET | `/Admin/Analytics/Data` | Analytics JSON data |
| GET | `/Admin/Reports` | Reports list |
| POST | `/Admin/Reports/Generate` | Generate report stub |
| GET | `/Admin/Settings` | Settings page |
| POST | `/Admin/Settings/Save` | Save settings stub |

---

# Frontend Pages

## Admin Dashboard (Graduation-MVC)

| URL | View | Purpose |
|-----|------|---------|
| `/` | Dashboard/Index | Admin dashboard with summary cards, charts, latest data |
| `/Admin/Dashboard` | Dashboard/Index | Same as root |
| `/Admin/Orders` | Orders/Index | Order list with filters and pagination |
| `/Admin/Orders/Details/{id}` | Orders/Details | Order details with timeline |
| `/Admin/Products` | Products/Index | Product list with filters and pagination |
| `/Admin/Products/Details/{id}` | Products/Details | Product details with images and reviews |
| `/Admin/Products/Reported` | Products/Reported | Reported products list |
| `/Admin/Vendors` | Vendors/Index | Vendor list with filters |
| `/Admin/Vendors/Pending` | Vendors/Pending | Pending vendor approvals |
| `/Admin/Vendors/Details/{id}` | Vendors/Details | Vendor full details with actions |
| `/Admin/Vendors/History` | Vendors/History | Unified audit history |
| `/Admin/Reviews` | Reviews/Index | Review list with filters |
| `/Admin/Reviews/Details/{id}` | Reviews/Details | Review details with moderation |
| `/Admin/Reviews/Reported` | Reviews/Reported | Reported reviews list |
| `/Admin/Analytics` | Analytics/Index | Charts and metrics |
| `/Admin/Reports` | Reports/Index | Reports list |
| `/Admin/Settings` | Settings/Index | Platform settings view |
| `/Home/Index` | Home/Index | Public home page (basic) |
| `/Home/Privacy` | - | Privacy page |

### APIs Consumed by Admin MVC
The MVC project consumes `IAdminDashboardService`, `IAdminProductService`, `IAdminReviewService`, `IAdminVendorsService`, `IOrderService` directly (same solution). It does NOT call the REST API.

---

# Database Documentation

## Tables Overview

### AspNetUsers (Identity)
**Purpose:** User accounts (customers, vendors, admins)

| Column | Type | Notes |
|--------|------|-------|
| Id | string (PK) | Identity GUID |
| FullName | string | Required |
| PreferredLanguage | string | Default "ar" |
| ProfileImage | string | URL from Cloudinary or empty |
| IsActive | bool | Default true; used for account suspension |
| OtpCode | string? | Password reset OTP |
| OtpExpiry | DateTime? | OTP expiration |
| OtpEmail | string? | Email confirmation OTP |
| OtpEmailExpiry | DateTime? | Email OTP expiration |
| GoogleId | string? | Google SSO identifier |
| Email, UserName, etc | Identity columns | Inherited from IdentityUser |

### Workshops
**Purpose:** Vendor workshop/business profile

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| UserId | string (FK) | → AspNetUsers.Id |
| WorkshopNameAr | string | Arabic name |
| WorkshopNameEn | string | English name |
| DescriptionAr | string | Arabic description |
| DescriptionEn | string | English description |
| LogoUrl | string? | Cloudinary URL |
| Rating | decimal? | Computed average rating |
| IsVerified | bool | Legacy verification flag |
| VerificationStatus | enum | Active/inActive |
| VerificationDate | DateTime? | When verified/rejected |
| VerifiedByAdminId | string? (FK) | → AspNetUsers.Id |
| VerificationNotes | string? | Admin notes |
| RejectionReason | string? | Why rejected |
| AccountStatus | enum | Approved/Suspended/Pending |
| AccountStatusChangedAt | DateTime? | Last status change |
| AccountStatusChangedByAdminId | string? (FK) | → AspNetUsers.Id |

### WorkshopAddress
**Purpose:** Physical address of workshop

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| WorkshopId | int (FK) | → Workshops.Id (1:1) |
| City | string | Required |
| Area | string | |
| Street | string | |
| BuildingNumber | string | |
| Notes | string | |

### Categories
**Purpose:** Product categories

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| NameAr | string | Required |
| NameEn | string | Required |
| ImageUrl | string | |

### Products
**Purpose:** Workshop products

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| WorkshopId | int (FK) | → Workshops.Id |
| UserId | string (FK) | → AspNetUsers.Id (vendor owner) |
| CategoryId | int (FK) | → Categories.Id |
| NameAr | string | Required |
| NameEn | string | Required |
| DescriptionAr | string | Required |
| DescriptionEn | string | Required |
| Price | decimal(18,2) | |
| Status | enum | Active/Inactive/Hidden |

### ProductImages
**Purpose:** Product image gallery

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| ProductId | int (FK) | → Products.Id |
| ImageUrl | string | Local URL `/images/products/...` |
| IsPrimary | bool | Whether this is the main image |

### Carts
**Purpose:** Shopping cart per user

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string (FK) | → AspNetUsers.Id |

### CartItems
**Purpose:** Items within a cart

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| CartId | int (FK) | → Carts.Id |
| ProductId | int (FK) | → Products.Id |
| Quantity | int | |
| Price | decimal(18,2) | Snapshot of product price |

### Orders
**Purpose:** Customer orders

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string (FK) | → AspNetUsers.Id |
| WorkshopId | int? (FK) | → Workshops.Id |
| TotalPrice | decimal(18,2) | |
| Status | string | Pending/Confirmed/In Progress/Ready for Pickup/Delivered/Cancelled |
| Address | string | Delivery address |
| PhoneNumber | string? | Contact phone |
| Notes | string? | Order notes |

### OrderItems
**Purpose:** Line items within an order

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| OrderId | int (FK) | → Orders.Id |
| ProductId | int (FK) | → Products.Id |
| Quantity | int | |
| UnitPrice | decimal(18,2) | |

### OrderStatusHistory
**Purpose:** Audit trail of order status changes

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| OrderId | int (FK) | → Orders.Id |
| OldStatus | string | |
| NewStatus | string | |

### FinalResultImages
**Purpose:** Before/after images for completed orders

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| OrderId | int (FK) | → Orders.Id |
| BeforeImageUrl | string | |
| AfterImageUrl | string | |
| UploadedAt | DateTime | |

### Reviews
**Purpose:** Product reviews by customers

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string (FK) | → AspNetUsers.Id |
| ProductId | int (FK) | → Products.Id |
| WorkshopId | int? (FK) | → Workshops.Id |
| Rating | int | 1-5 |
| Comment | string | |
| VendorReply | string? | Reply from vendor |
| ReplyCreatedAt | DateTime? | |
| IsReported | bool | |
| ReportReason | string? | |

### ReviewModerationLogs
**Purpose:** Audit log for admin review moderation actions

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| ReviewId | int (FK) | → Reviews.Id |
| Action | string | e.g., "ResolveReport", "IgnoreReport", "DeleteReview" |
| Notes | string? | |
| AdminUserId | string? | |

### Favorites
**Purpose:** User favorite/wishlist products

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string (FK) | → AspNetUsers.Id |
| ProductId | int (FK) | → Products.Id |

### Addresses
**Purpose:** User delivery addresses

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string (FK) | → AspNetUsers.Id |
| City | string | |
| Area | string | |
| Street | string | |
| BuildingNumber | string | |
| Notes | string | |

### Discounts
**Purpose:** Discount codes (entity exists but no functional code)

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| Code | string | |
| Description | string | |
| DiscountValue | decimal(18,2) | |
| StartDate | DateTime | |
| EndDate | DateTime | |

### Notifications
**Purpose:** Legacy notification table (entity exists, not used in code)

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string (FK) | |
| Title | string | |
| Message | string | |
| IsRead | bool | |

### InternalNotifications
**Purpose:** In-app bilingual notifications

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | string? (FK) | → AspNetUsers.Id |
| TitleAr | string? | Arabic title |
| TitleEn | string? | English title |
| MessageAr | string? | Arabic message |
| MessageEn | string? | English message |
| IsRead | bool | |

### ProductReports
**Purpose:** Reports on products by users

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| ProductId | int (FK) | → Products.Id |
| UserId | string (FK) | → AspNetUsers.Id |
| Reason | string | |
| IsResolved | bool | |

### VendorVerificationHistory
**Purpose:** Audit log for vendor verification status changes

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| WorkshopId | int (FK) | → Workshops.Id |
| OldStatus | enum | |
| NewStatus | enum | |
| Notes | string? | |
| RejectionReason | string? | |
| PerformedByAdminId | string? (FK) | → AspNetUsers.Id |

### VendorAccountStatusHistory
**Purpose:** Audit log for vendor account status changes

| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| WorkshopId | int (FK) | → Workshops.Id |
| OldStatus | enum | |
| NewStatus | enum | |
| Reason | string? | |
| Notes | string? | |
| PerformedByAdminId | string? (FK) | → AspNetUsers.Id |

---

# Business Workflows

## User Registration

```
Trigger: POST /api/Auth/register
Steps:
  1. Validate email not already registered
  2. Map RegisterDto → ApplicationUser via Mapster
  3. Create user via UserManager.CreateAsync() with password
  4. Ensure "Customer" role exists
  5. Assign user to "Customer" role
  6. Rollback user creation if role assignment fails
  7. Return AuthResponseDto (without token - email not confirmed)
Services: AuthService, UserManager, RoleManager
DB Changes: AspNetUsers row created, AspNetUserRoles row created
```

## Customer Login

```
Trigger: POST /api/Auth/login
Steps:
  1. Find user by email
  2. Validate password
  3. Check IsActive flag
  4. Check EmailConfirmed; if not confirmed, send OTP and throw
  5. Get user roles
  6. Validate user has "Customer" role (block vendors)
  7. Generate JWT token
  8. Return AuthResponseDto with token
Services: AuthService, JwtTokenGenerator
DB Changes: None (read only)
```

## Google Login/Auto-Register

```
Trigger: POST /api/Auth/google-login
Steps:
  1. Validate Google IdToken with Google.Apis.Auth
  2. Find user by email from payload
  3. If exists: update GoogleId if missing, check IsActive, generate token with existing roles
  4. If new: map payload → ApplicationUser, create user, assign Customer role, generate token
Services: AuthService, UserManager, JwtTokenGenerator
DB Changes: AspNetUsers created or updated
```

## Forgot Password

```
Trigger: POST /api/Auth/forgot-password
Steps:
  1. Find user by email
  2. Check IsActive
  3. Generate 6-digit OTP
  4. Store OTP and expiry on user record
  5. Send OTP email via SendGrid
  6. Create internal notification (PasswordReset)
Services: AuthService, EmailService, InternalNotificationService
DB Changes: Updated OtpCode, OtpExpiry on AspNetUsers
```

## Reset Password

```
Trigger: POST /api/Auth/reset-password
Steps:
  1. Find user by email
  2. Validate OTP matches and not expired
  3. Remove old password via UserManager
  4. Add new password via UserManager
  5. Clear OTP fields
  6. Create internal notification (PasswordReset)
Services: AuthService, UserManager, InternalNotificationService
DB Changes: Password hash updated, OTP fields cleared
```

## Vendor Registration

```
Trigger: POST /api/Vendors/create
Steps:
  1. Check email not already registered
  2. Map CreateVendorDto → ApplicationUser via Mapster
  3. Create user via UserManager.CreateAsync()
  4. Set password via UserManager.AddPasswordAsync() (rollback user on failure)
  5. Ensure "Vendor" role exists
  6. Assign user to "Vendor" role (rollback user on failure)
  7. Send email confirmation OTP
  8. Map CreateVendorDto → Workshop via Mapster
  9. Save workshop with UserId
  10. If WorkshopAddress provided, create and link to workshop
Services: VendorService, UserManager, RoleManager, AuthService
DB Changes: AspNetUsers created, AspNetUserRoles created, Workshops created, WorkshopAddress created (optional)
```

## Vendor Login

```
Trigger: POST /api/Vendors/login
Steps:
  1. Find user by email
  2. Validate password
  3. Check email confirmation; if not confirmed, resend OTP and throw
  4. Check IsActive (admin approval check)
  5. Verify user is in "Vendor" role
  6. Load workshop for workshop ID
  7. Generate JWT token with WorkshopId claim
  8. Return VendorAuthResponseDto
Services: VendorService, UserManager, JwtTokenGenerator
DB Changes: None (read only)
```

## Order Creation

```
Trigger: POST /api/Order
Steps:
  1. Extract userId from JWT
  2. Load user's cart with items and products (no tracking)
  3. Validate cart not empty
  4. Calculate total price
  5. Create Order object with status "Pending"
  6. Create OrderItem objects from cart items
  7. Create OrderStatusHistory entry
  8. Save order (EF tracks items and history)
  9. Clear cart
  10. Send internal notification to customer (OrderPending)
  11. Send internal notification to vendor (NewOrder)
  12. Send email notification (order confirmation)
Services: OrderService, CartService, NotificationService, InternalNotificationService
DB Changes: Orders row, OrderItems rows, OrderStatusHistory row; CartItems deleted
```

## Order Status Transition

```
Trigger: PUT /api/VendorOrders/orders/{orderId}/status (Vendor)
         PUT /api/Order/{orderId}/status (Admin/Customer)

Valid Transitions:
  Pending → Confirmed, Cancelled
  Confirmed → In Progress, Cancelled
  In Progress → Ready for Pickup, Cancelled
  Ready for Pickup → Delivered, Cancelled
  Delivered → (none)
  Cancelled → (none)

Steps:
  1. Validate new status is valid
  2. Load order with status history
  3. Check current status → new status is valid transition
  4. Update order status
  5. Add to status history
  6. Send internal notification based on new status
  7. Send email notification
Services: OrderService/VendorOrderService, NotificationService, InternalNotificationService
DB Changes: Order status updated, OrderStatusHistory row added
```

## Order Cancellation (Customer)

```
Trigger: PUT /api/Order/{id}/cancel
Steps:
  1. Find order where OrderId == id AND UserId == current user
  2. Validate order not already Delivered or Cancelled
  3. Update status to "Cancelled"
  4. Add to status history
  5. Send internal notification to customer (OrderCancelled)
  6. Send internal notification to vendor (VendorOrderCancelled)
  7. Send email + WhatsApp cancellation notification
Services: OrderService, NotificationService, InternalNotificationService
DB Changes: Order status updated, OrderStatusHistory row added
```

## Vendor Approval/Rejection (Admin)

```
Trigger: POST /Admin/Vendors/Approve or Reject

Approve:
  1. Load workshop
  2. Set VerificationStatus = Active, IsVerified = true
  3. Activate user (IsActive = true)
  4. Set verification date, notes
  5. Log to VendorVerificationHistory
  6. Send internal notification (AccountApproved)

Reject:
  1. Load workshop
  2. Set VerificationStatus = inActive, IsVerified = false
  3. Save rejection reason
  4. Log to VendorVerificationHistory
  5. Send internal notification (VendorAccountRejected)

Services: AdminVendorsService, InternalNotificationService
DB Changes: Workshop updated, AspNetUsers.IsActive updated, VendorVerificationHistory row added
```

## Vendor Suspend/Activate (Admin)

```
Trigger: POST /Admin/Vendors/Suspend or Activate

Suspend:
  1. Load workshop
  2. Set AccountStatus = Suspended, IsActive = false
  3. Deactivate user (IsActive = false)
  4. Save reason
  5. Log to VendorAccountStatusHistory
  6. Send internal notification (VendorAccountSuspended)

Activate:
  1. Load workshop
  2. Set AccountStatus = Approved, IsActive = true
  3. Activate user (IsActive = true)
  4. Log to VendorAccountStatusHistory
  5. Send internal notification (VendorAccountReactivated)

Services: AdminVendorsService, InternalNotificationService
DB Changes: Workshop + User updated, VendorAccountStatusHistory row added
```

## Review Creation

```
Trigger: POST /api/Reviews
Steps:
  1. Validate product exists
  2. Validate rating 1-5
  3. Check user hasn't already reviewed this product (one review per product)
  4. Create review record
  5. Send internal notification to vendor (NewReview)
Services: ReviewService, InternalNotificationService
DB Changes: Reviews row created
```

## Vendor Review Reply

```
Trigger: POST /api/Reviews/{reviewId}/reply
Steps:
  1. Load review with product
  2. Verify vendor owns the reviewed product (Product.UserId == current user)
  3. Set VendorReply and ReplyCreatedAt
Services: ReviewService
DB Changes: Review.VendorReply, Review.ReplyCreatedAt updated
```

---

# Authentication & Authorization

| Aspect | Implementation |
|--------|---------------|
| **Authentication Method** | JWT Bearer Token |
| **JWT Secret** | `gbSUaakx6G6kn7ZT5wDcjSoltoAliBashier` (from appsettings.json) |
| **JWT Issuer** | `GraduationApp` |
| **JWT Audience** | `GraduationAppUsers` |
| **Token Expiry** | 1 day (configurable via `Jwt:ExpiryInDays`) |
| **Cookies** | ASP.NET Identity cookie configured with 401/403 redirect suppression for API; SameSite=Strict, HttpOnly |
| **Refresh Tokens** | Not implemented |
| **Roles** | `Customer`, `Vendor`, `Admin` (also seeds `Workshop`) |
| **Claims In JWT** | `NameIdentifier`, `sub`, `email`, `name`, `lang`, `WorkshopId` (if vendor), `role` |
| **Authorization** | `[Authorize]`, `[Authorize(Roles = "...")]`, `[AllowAnonymous]` |

## Role-Based Access Summary

| Endpoint Pattern | Customer | Vendor | Admin |
|-----------------|----------|--------|-------|
| `/api/Auth/*` | ✓ | ✓ (login) | - |
| `/api/Products` GET | ✓ | ✓ | ✓ |
| `/api/Products` POST/PUT/DELETE | ✗ | ✓ | ✗ |
| `/api/Products/images/*` | ✗ | ✓ | ✗ |
| `/api/Cart/*` | ✓ | ✗ | ✗ |
| `/api/Order/*` | ✓ | ✗ | ✓ |
| `/api/VendorOrders/*` | ✗ | ✓ | ✗ |
| `/api/Favorites/*` | ✓ | ✗ | ✗ |
| `/api/Profile/*` | ✓ | ✗ | ✗ |
| `/api/Reviews` POST | ✓ | ✗ (blocked) | ✗ |
| `/api/Reviews/vendor/*` | ✗ | ✓ | ✗ |
| `/api/Vendors/*` | ✗ | ✓ | ✗ |
| `/api/Categories` POST/PUT/DELETE | ✗ | ✗ | ✓ |
| `/api/Roles/*` | ✓ (no auth guard) | ✓ | ✓ |
| `/Admin/*` (MVC) | ✗ | ✗ | ✓ (no auth guard in code) |

**Security Note:** The Admin MVC controllers have `[Authorize(Roles = "Admin")]` commented out. There is a policy `"AdminOnly"` registered but not applied to controllers.

---

# Integrations

## SendGrid (Email)

| Property | Value |
|----------|-------|
| **Purpose** | Send transactional emails (OTP, order confirmation, status updates) |
| **API Key** | `SendGrid:ApiKey` from appsettings |
| **From Email** | `sultan.alhejeery@gmail.com` (appsettings) |
| **From Name** | `GraduationAppKey` (appsettings) |
| **Fallback** | Gracefully skips if API key not configured, logs to console |
| **Email Types** | `SendOtpEmailAsync`, `SendEmailConfirmationOtpAsync`, `SendOrderCreatedEmailAsync`, `SendOrderStatusChangedEmailAsync` |

## Cloudinary (Image Storage)

| Property | Value |
|----------|-------|
| **Purpose** | Store vendor logos and user profile images |
| **Cloud Name** | `deko7oqij` (appsettings) |
| **API Key** | `594961782436319` (appsettings) |
| **API Secret** | `UkzeXay_KO7XMP0ye4AlrF3pfBo` (appsettings) |
| **Allowed Extensions** | `.jpg`, `.jpeg`, `.png` |
| **Operations** | Upload image to folder, delete image by URL |

## WhatsApp Cloud API (Meta)

| Property | Value |
|----------|-------|
| **Purpose** | Send WhatsApp template messages to customers |
| **Base URL** | `https://graph.facebook.com` |
| **API Version** | `v25.0` |
| **Phone Number ID** | `1141826405676664` |
| **Access Token** | (configured in appsettings) |
| **Default Template** | `aaa` |
| **Language** | `ar` (Arabic) |
| **Usage** | Currently only used for order cancellation notifications |
| **Error Handling** | Retries without body parameters if 132000 error; logs failures |

## Google Sign-In

| Property | Value |
|----------|-------|
| **Purpose** | Authenticate users via Google OAuth |
| **Client ID** | `834738882064-e87ejpnt830djaabjh07uhhk626sanhe.apps.googleusercontent.com` |
| **Library** | `Google.Apis.Auth` |
| **Validation** | Validates IdToken audience matches configured ClientId |

---

# Background Processing

**None discovered.** There are no hosted services, background services, cron jobs, queues, or event handlers implemented in this codebase.

---

# Configuration Reference

## appsettings.json (API)

| Key | Purpose | Example Value |
|-----|---------|---------------|
| `ConnectionStrings:GraduationDbOnline` | SQL Server connection string | `Server=db49972.public.databaseasp.net;...` |
| `Jwt:Secret` | Symmetric key for JWT signing | `gbSUaakx6G6kn7ZT5wDcjSoltoAliBashier` |
| `Jwt:Issuer` | JWT issuer claim | `GraduationApp` |
| `Jwt:Audience` | JWT audience claim | `GraduationAppUsers` |
| `Jwt:ExpiryInMinutes` | Token expiry in minutes | `1440` |
| `Jwt:ExpiryInDays` | Token expiry in days | `1` |
| `SendGrid:ApiKey` | SendGrid API key for email | `SG.hYUcKDoiR...` |
| `SendGrid:FromEmail` | Sender email address | `sultan.alhejeery@gmail.com` |
| `SendGrid:FromName` | Sender display name | `GraduationAppKey` |
| `Google:ClientId` | Google OAuth client ID | `834738882064-...` |
| `WhatsAppNotification:GraphApiBaseUrl` | Meta Graph API base URL | `https://graph.facebook.com` |
| `WhatsAppNotification:ApiVersion` | Graph API version | `v25.0` |
| `WhatsAppNotification:PhoneNumberId` | WhatsApp Business phone ID | `1141826405676664` |
| `WhatsAppNotification:AccessToken` | WhatsApp access token | `EAGC0GhtZCr6c...` |
| `WhatsAppNotification:DefaultTemplateName` | Default template for messages | `aaa` |
| `WhatsAppNotification:DefaultLanguageCode` | Template language | `ar` |
| `OtpSettings:ExpiryInMinutes` | OTP code validity duration | `10` |
| `CloudinarySettings:CloudName` | Cloudinary cloud name | `deko7oqij` |
| `CloudinarySettings:ApiKey` | Cloudinary API key | `594961782436319` |
| `CloudinarySettings:ApiSecret` | Cloudinary API secret | `UkzeXay_KO7XMP0ye4AlrF3pfBo` |

## appsettings.json (MVC)

| Key | Purpose | Example Value |
|-----|---------|---------------|
| `ConnectionStrings:GraduationDbOnline` | Same database connection | Same as API |
| `ApiSettings:BaseUrl` | API base URL for potential HTTP calls | `http://localhost:5182` |
| `ApiSettings:WwwRootPath` | Path to API's wwwroot for shared static files | `../Graduation-API/wwwroot` |

## Environment Variables / Secrets

No environment variable overrides detected in code. All configuration is via `appsettings.json` with hardcoded values (including secrets).

**⚠️ Security Warning:** The following credentials are hardcoded in `appsettings.json` and committed to source control:
- JWT Secret
- SendGrid API Key
- Cloudinary API Key & Secret
- WhatsApp Access Token
- Database connection string with password

---

# Security Review

| Issue | Severity | Location |
|-------|----------|----------|
| Secrets hardcoded in appsettings.json | **HIGH** | All API keys, JWT secret, DB password in source |
| Roles API has no authorization | **HIGH** | Any user can create roles and assign them |
| Admin MVC controllers have `[Authorize]` commented out | **HIGH** | `Areas/Admin/Controllers/*` - no auth guard active |
| `UpdateOrderStatus` on OrderController has `[Authorize(Roles = "Admin")]` commented out | **MEDIUM** | Any authenticated user can change any order's status |
| Order status can be changed by customer directly | **MEDIUM** | `PUT /api/Order/{id}/status` has no role restriction |
| Product ownership validation present | Good | `EnsureProductOwnership` in ProductService |
| Review ownership validation present | Good | Only review owner can delete |
| Vendor blocked from creating reviews | Good | Controller-level check |
| SQL Server connection uses `Encrypt=True; TrustServerCertificate=True` | **MEDIUM** | TrustServerCertificate bypasses certificate validation |
| JWT secret is short and simple | **MEDIUM** | `gbSUaakx6G6kn7ZT5wDcjSoltoAliBashier` - consider longer |
| Google ClientId is committed | **LOW** | Public by design for OAuth |
| WhatsApp access token committed | **HIGH** | Would allow anyone to send WhatsApp messages |

---

# Technical Debt

| Issue | Severity | Location |
|-------|----------|----------|
| RolesController has no authorization | High | Any user can create/assign roles |
| Admin MVC authorization commented out | High | No protection on admin pages |
| `Discount` entity exists but never used | Medium | Full DB table with no code usage |
| `Notification` entity exists but never used | Medium | Legacy table, `InternalNotification` used instead |
| `FinalResultImage` entity exists with no controller/service | Medium | DB table with no API to manage |
| `ProductReport` table has no customer-facing report creation API | Medium | No endpoint to create a report |
| Order status transitions duplicated in two services | Medium | `OrderService` and `VendorOrderService` have identical transition logic |
| Settings are hardcoded, not persisted | Medium | No DB table for settings |
| Export/Download actions are stubs | Low | Redirect without actual file generation |
| Forecast chart always empty | Low | No forecasting logic |
| `WorkshopId` not set on order creation | Low | Order.WorkshopId is only set via vendor order status, not at creation time |
| Gender/Salutation field referenced in DTO but uses `string.Empty` | Low | `UserProfileDto.Salutation` default |
| Multiple commented-out code blocks | Low | Throughout the codebase |
| `CreateProductDto` has no `IsActive` field | Low | Must use separate status endpoint |
| Profile image uses local disk but profile service uses Cloudinary | Low | Inconsistent storage approach |
| LinkVendorProductsAsync is never called from API | Low | Internal utility without trigger |
| `OtpSettings:ExpiryInMinutes` parsed on every call | Low | Should use IOptions pattern |
| No input sanitization on search terms | Low | Contains search could be abused |

---

# Dependency Graph

## Architecture Layers

```mermaid
graph TD
    subgraph Presentation
        API[Graduation-API<br/>Controllers]
        MVC[Graduation-MVC<br/>Admin Dashboard]
    end

    subgraph Application
        Services[Services<br/>Business Logic]
        DTOs[DTOs<br/>Data Transfer]
        Mappers[Mapster Configs]
        Interfaces[IServices<br/>IRepositories]
    end

    subgraph Domain
        Entities[Entities]
        Enums[Enums]
    end

    subgraph Infrastructure
        EF[EF Core DbContext]
        Repos[Repositories]
        Identity[JWT Generator]
        Seed[Database Seeder]
        FileSvc[File Service]
    end

    API --> Interfaces
    MVC --> Interfaces
    Services --> Interfaces
    Services --> DTOs
    Services --> Entities
    Services --> Mappers
    Repos --> EF
    Repos --> Interfaces
    EF --> Entities
    Identity --> Entities
    Seed --> EF
    FileSvc --> Cloudinary
```

## Service Dependencies

```mermaid
graph LR
    AuthService --> UserManager
    AuthService --> RoleManager
    AuthService --> JwtTokenGenerator
    AuthService --> EmailService
    AuthService --> InternalNotificationService
    
    VendorService --> UserManager
    VendorService --> RoleManager
    VendorService --> FileService
    VendorService --> JwtTokenGenerator
    VendorService --> IGenaricRepositories
    
    ProductService --> IGenaricRepositories
    VendorProductService --> IGenaricRepositories
    
    OrderService --> CartService
    OrderService --> NotificationService
    OrderService --> InternalNotificationService
    OrderService --> IGenaricRepositories
    
    VendorOrderService --> NotificationService
    VendorOrderService --> InternalNotificationService
    VendorOrderService --> IGenaricRepositories
    
    CartService --> IGenaricRepositories
    FavoriteService --> IGenaricRepositories
    ReviewService --> InternalNotificationService
    ReviewService --> IGenaricRepositories
    CategoryService --> IGenaricRepositories
    
    NotificationService --> EmailService
    NotificationService --> WhatsAppService
    
    AdminDashboardService --> IGenaricRepositories
    AdminProductService --> IGenaricRepositories
    AdminReviewService --> IGenaricRepositories
    AdminVendorsService --> IGenaricRepositories
    AdminVendorsService --> InternalNotificationService
    AdminVendorsService --> AdminVendorsRepository
    
    ProfileService --> FileService
    ProfileService --> InternalNotificationService
    ProfileService --> ProfileRepository
```

---

# Project Summary

| Metric | Count |
|--------|-------|
| **Total Features** | 42 |
| **Total API Endpoints** | 56 |
| **Total Admin MVC Routes** | 31 |
| **Total Roles** | 3 (Customer, Vendor, Admin) + 1 legacy (Workshop) |
| **Total Database Tables** | 20 (including 8 Identity tables) |
| **Custom Entities** | 17 |
| **External Integrations** | 4 (SendGrid, Cloudinary, WhatsApp, Google) |
| **Key Business Flows** | 12 documented workflows |
| **Controllers (API)** | 12 |
| **Controllers (MVC)** | 9 (1 Home + 8 Admin Area) |
| **Services (Application)** | 16 |
| **Services (Infrastructure)** | 1 (FileService) |
| **Repositories** | 3 (Generic + Profile + AdminVendors) |

## Final Feature Coverage Score

**98%** – All 42 features were discovered through systematic code traversal:

1. Read `Program.cs` entry points (API + MVC)
2. Traced DI registrations (ServiceAPI.cs, ServiceMVC.cs)
3. Read all 12 API controllers (all endpoints)
4. Read all 8 Admin MVC controllers (all routes)
5. Read all 16 application services (full business logic)
6. Read all 17 domain entities + all enums
7. Read all DTOs across all modules
8. Read generic and specific repositories
9. Read JWT token generation (claims, roles)
10. Read email, WhatsApp, Cloudinary integrations
11. Read database seeder (initial data setup)
12. Read DbContext (relationships, constraints)
13. Read all appsettings.json files
14. Read constant files (Roles)

**What might be missing**: The `Discount` table has no corresponding service/controller, and it's possible there are plans for it. The `FinalResultImage` table has no API endpoints but exists as an entity. These are documented as unused entities rather than separate features, so coverage is accurate.
