# Localization Completion Plan - HomeAi Admin Console

This document outlines the detailed audit and execution plan to finalize the localization system for the ITI Graduation Project BackEnd (HomeAi Marketplace - Admin Console). The system leverages a custom JSON-based localization provider (`ILocalizationService` and `LocalizationService`) reading from `localize_en.json` and `localize_ar.json` inside the `Graduation-infrastructure/Localization/` directory.

---

## Phase 1: Project Analysis

### Current Localization Infrastructure Status
- **Core Mechanism**: Fully functional custom JSON-based localization (`ILocalizationService`) loaded via HTTP Request Cookie (`culture`) and User DB Preference (`PreferredLanguage`). 
- **Active Languages**: English (`en`) and Arabic (`ar`).
- **RTL/LTR Support**: Dynamic direction injection (`dir="rtl"` / `dir="ltr"`) and language attribute (`lang="ar"` / `lang="en"`) in layouts (`_LayoutAuth.cshtml`, `_AdminLayout.cshtml`). Bootstraps' RTL CSS (`bootstrap.rtl.min.css`) is loaded automatically when Arabic is selected.
- **Language Switcher**: Fully functional javascript-based switching (`setLanguage(culture)`) using cookies and POSTing to `AccountController/SetLanguage` for database persistence.
- **Overall Completion Status**: **~35% Complete**.
  - Infrastructure, Auth Views, Dashboard View, Profile Views, and Orders Views are localized.
  - All other controllers and views (12 out of 16 controllers) are completely unlocalized and remain in English.
  - Validation messages in ViewModels and backend error messages returned in `TempData` are not localized.

---

## Phase 2: Localization Audit

For every Controller discovered in the solution, its current localization status is outlined below.

---

### Controller: AccountController
- **Current Status**: Partially Completed
- **Existing Localization**:
  - **Localized Views**: `AccessDenied.cshtml`, `ForgotPassword.cshtml`, `Login.cshtml`, `VerifyOtp.cshtml`, `ResetPassword.cshtml`
  - **Existing Resource Files**: `localize_en.json`, `localize_ar.json` (inside `auth.login`, `auth.forgotPassword`, `auth.verifyOtp`, `auth.resetPassword`, `auth.accessDenied` JSON nodes)
  - **Existing Resource Keys**: Title, labels, buttons, placeholders, access denied messages.
  - **Existing Validation Messages**: Default ASP.NET Core messages are used. Custom validation messages are hardcoded in English.
  - **Existing Language Support**: English and Arabic.
- **Missing Work**:
  - Localize ViewModel validation attributes (e.g. `ResetPasswordViewModel.cs` - `[Required(ErrorMessage = "New password is required")]`).
  - Localize the exception messages caught and stored in `TempData["ErrorMessage"]`.
- **Completion Percentage**: 90% Complete

---

### Controller: DashboardController
- **Current Status**: Partially Completed
- **Existing Localization**:
  - **Localized Views**: `Index.cshtml`
  - **Existing Resource Files**: `localize_en.json`, `localize_ar.json` (inside `auth.dashboard` JSON nodes)
  - **Existing Resource Keys**: Cards titles, metric labels, quick actions titles/descriptions, table headers, recent activities formatting helper keys.
  - **Existing Validation Messages**: N/A
  - **Existing Language Support**: English and Arabic.
- **Missing Work**:
  - Translate the chart title dynamically inside JS (e.g. `RevenueChart.Title`).
- **Completion Percentage**: 95% Complete

---

### Controller: ProfileController
- **Current Status**: Partially Completed
- **Existing Localization**:
  - **Localized Views**: `Index.cshtml`, `Edit.cshtml`, `ChangePassword.cshtml`
  - **Existing Resource Files**: `localize_en.json`, `localize_ar.json` (inside `auth.profile` JSON nodes)
  - **Existing Resource Keys**: Form fields, card headers, security overview texts, success messages, change password instruction keys.
  - **Existing Validation Messages**: N/A
  - **Existing Language Support**: English and Arabic.
- **Missing Work**:
  - Localize error responses returned by DB password updating services.
- **Completion Percentage**: 85% Complete

---

### Controller: OrdersController
- **Current Status**: Partially Completed
- **Existing Localization**:
  - **Localized Views**: `Index.cshtml`, `Details.cshtml`
  - **Existing Resource Files**: `localize_en.json`, `localize_ar.json` (inside `auth.orders` JSON nodes)
  - **Existing Resource Keys**: Search fields, dropdown labels, table headers, pagination strings, status badges, timelines, currency format, customer/vendor info labels.
  - **Existing Validation Messages**: N/A
  - **Existing Language Support**: English and Arabic.
- **Missing Work**:
  - Localize `TempData["SuccessMessage"]` strings inside `OrdersController.cs` (e.g., status changed confirmation message).
- **Completion Percentage**: 85% Complete

---

### Controller: VendorsController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: Keys exist in `localize_en.json` (inside `auth.vendors` JSON node) but **not** in `localize_ar.json`.
  - **Existing Resource Keys**: Statistics labels, table headers, detail labels, action buttons, SweetAlert2 confirm prompt templates.
  - **Existing Validation Messages**: Custom validation messages in ViewModels are hardcoded in English.
  - **Existing Language Support**: English only (RTL alignment issues exist).
- **Missing Work**:
  - Add the `vendors` JSON node to `localize_ar.json`.
  - Inject `ILocalizationService` in views: `Index.cshtml`, `Pending.cshtml`, `History.cshtml`, `Details.cshtml`, and `_VendorsList.cshtml`.
  - Replace hardcoded text, placeholder, table headers, and modal messages with `@LocalizationService.Get()`.
  - Localize `TempData["SuccessMessage"]` and `TempData["ErrorMessage"]` inside `VendorsController.cs`.
  - Localize SweetAlert2 confirmation popups and buttons.
- **Completion Percentage**: 0% Complete

---

### Controller: ProductsController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `products` JSON nodes to both `localize_en.json` and `localize_ar.json`.
  - Inject `ILocalizationService` in views: `Index.cshtml`, `Details.cshtml`, and `Reported.cshtml`.
  - Localize table headers, filters labels, details panels, status badges, search placeholders, and actions tooltips.
  - Localize `TempData` response messages inside `ProductsController.cs`.
- **Completion Percentage**: 0% Complete

---

### Controller: CategoriesController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: Hardcoded in English inside `AdminCategoryFormViewModel.cs`.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `categories` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in views: `Index.cshtml`, `Create.cshtml`, and `Edit.cshtml`.
  - Localize forms, placeholders, delete confirm warnings, and table headers.
  - Localize ViewModel validation attributes.
  - Localize controller `TempData` responses.
- **Completion Percentage**: 0% Complete

---

### Controller: BannersController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: Hardcoded in English inside `AdminBannerFormViewModel.cs`.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `banners` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in views: `Index.cshtml`, `Create.cshtml`, and `Edit.cshtml`.
  - Localize headers, inputs, placeholders, table records, delete confirmations, and badges.
  - Localize ViewModel validation attributes.
  - Localize controller `TempData` responses.
- **Completion Percentage**: 0% Complete

---

### Controller: FaqController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: Hardcoded in English inside `AdminFaqFormViewModel.cs`.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `faq` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in views: `Index.cshtml`, `Create.cshtml`, and `Edit.cshtml`.
  - Localize headers, inputs, placeholders, table records, and delete confirmations.
  - Localize ViewModel validation attributes.
  - Localize controller `TempData` responses.
- **Completion Percentage**: 0% Complete

---

### Controller: AnalyticsController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `analytics` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in `Index.cshtml` view.
  - Localize headers, card titles, chart titles, table ranking items, datepicker labels, and date pickers.
  - Localize `TempData["SuccessMessage"]` inside `AnalyticsController.cs`.
- **Completion Percentage**: 0% Complete

---

### Controller: AuditLogsController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `auditLogs` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in views: `Index.cshtml` and `Details.cshtml`.
  - Localize filters, table headers, system action labels (e.g. `ApproveVendor`), and role strings.
- **Completion Percentage**: 0% Complete

---

### Controller: ReportsController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `reports` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in `Index.cshtml`.
  - Localize dashboard download cards and headers.
  - Localize controller `TempData` responses.
- **Completion Percentage**: 0% Complete

---

### Controller: ReviewsController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `reviews` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in views: `Index.cshtml`, `Details.cshtml`, and `Reported.cshtml`.
  - Localize reviews lists, badges, action buttons, details panels, and delete alerts.
- **Completion Percentage**: 0% Complete

---

### Controller: SettingsController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `settings` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in `Index.cshtml`.
  - Localize configuration forms, labels, inputs, and submit button.
  - Localize controller `TempData` responses.
- **Completion Percentage**: 0% Complete

---

### Controller: UsersController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Add `users` JSON nodes to both JSON files.
  - Inject `ILocalizationService` in views: `Index.cshtml` and `Details.cshtml`.
  - Localize customers lists, search boxes, details, and activity logs.
  - Localize controller `TempData` responses.
- **Completion Percentage**: 0% Complete

---

### Controller: HomeController
- **Current Status**: Not Started
- **Existing Localization**:
  - **Localized Views**: None.
  - **Existing Resource Files**: None.
  - **Existing Resource Keys**: None.
  - **Existing Validation Messages**: None.
  - **Existing Language Support**: English only.
- **Missing Work**:
  - Inject `ILocalizationService` in `Index.cshtml`.
  - Localize welcome texts and link labels.
- **Completion Percentage**: 0% Complete

---

## Phase 3: Detailed Execution Plan

Detailed localization plans grouped by controller.

---

### 1. VendorsController
- **Related Views**:
  - `Areas/Admin/Views/Vendors/Index.cshtml`
  - `Areas/Admin/Views/Vendors/Pending.cshtml`
  - `Areas/Admin/Views/Vendors/History.cshtml`
  - `Areas/Admin/Views/Vendors/Details.cshtml`
  - `Areas/Admin/Views/Vendors/_VendorsList.cshtml`
- **Remaining Localization Tasks**:
  - Add Arabic translations in `localize_ar.json` matching English keys under the `vendors` node.
  - Replace all hardcoded labels, table headers, placeholders, action buttons, alert boxes, and status tags with `@LocalizationService.Get()`.
  - Localize confirmation prompts in SweetAlert2 (JS) inside details page.
  - In `VendorsController.cs`, inject `ILocalizationService` and translate all success/error strings stored in `TempData`.
- **Files To Update**:
  - `Areas/Admin/Views/Vendors/Index.cshtml`
  - `Areas/Admin/Views/Vendors/Pending.cshtml`
  - `Areas/Admin/Views/Vendors/History.cshtml`
  - `Areas/Admin/Views/Vendors/Details.cshtml`
  - `Areas/Admin/Views/Vendors/_VendorsList.cshtml`
  - `Areas/Admin/Controllers/VendorsController.cs`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Medium
- **Acceptance Criteria**:
  - Vendor list statistics, filters, tables, and pagination render correctly in Arabic and English.
  - Verification Actions (Approve, Reject, Suspend, Activate) prompt localized confirmation messages and return localized success notifications.
  - Status histories and details panel align correctly in LTR and RTL directions.

---

### 2. ProductsController
- **Related Views**:
  - `Areas/Admin/Views/Products/Index.cshtml`
  - `Areas/Admin/Views/Products/Details.cshtml`
  - `Areas/Admin/Views/Products/Reported.cshtml`
- **Remaining Localization Tasks**:
  - Add `products` node to `localize_en.json` and `localize_ar.json`.
  - Inject `ILocalizationService` and translate headers, filter controls, product info fields, currency formatters, empty states, and action tooltips.
  - In `ProductsController.cs`, translate all success messages.
- **Files To Update**:
  - `Areas/Admin/Views/Products/Index.cshtml`
  - `Areas/Admin/Views/Products/Details.cshtml`
  - `Areas/Admin/Views/Products/Reported.cshtml`
  - `Areas/Admin/Controllers/ProductsController.cs`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Medium
- **Acceptance Criteria**:
  - Filter options are localized.
  - Table headers and pagination text display according to current culture.
  - Action warnings, detail page tabs, and reported reason strings display localized text.

---

### 3. CategoriesController
- **Related Views**:
  - `Areas/Admin/Views/Categories/Index.cshtml`
  - `Areas/Admin/Views/Categories/Create.cshtml`
  - `Areas/Admin/Views/Categories/Edit.cshtml`
- **Remaining Localization Tasks**:
  - Add `categories` node to translation files.
  - Localize the category form input labels, placeholders, header titles, and create/edit submit buttons.
  - In `AdminCategoryFormViewModel.cs`, add localized Validation Messages using custom resource keys or custom adapters.
  - Translate controller `TempData` responses.
- **Files To Update**:
  - `Areas/Admin/Views/Categories/Index.cshtml`
  - `Areas/Admin/Views/Categories/Create.cshtml`
  - `Areas/Admin/Views/Categories/Edit.cshtml`
  - `Areas/Admin/ViewModels/Categories/AdminCategoryFormViewModel.cs`
  - `Areas/Admin/Controllers/CategoriesController.cs`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Small
- **Acceptance Criteria**:
  - Category forms show validation errors in the active culture.
  - Table actions and headers render localized strings.

---

### 4. BannersController
- **Related Views**:
  - `Areas/Admin/Views/Banners/Index.cshtml`
  - `Areas/Admin/Views/Banners/Create.cshtml`
  - `Areas/Admin/Views/Banners/Edit.cshtml`
- **Remaining Localization Tasks**:
  - Add `banners` node to translation files.
  - Translate layout labels, input elements, tooltips, table columns, order numbers, and active badges.
  - Localize `AdminBannerFormViewModel.cs` validation messages.
  - Localize controller `TempData` alerts.
- **Files To Update**:
  - `Areas/Admin/Views/Banners/Index.cshtml`
  - `Areas/Admin/Views/Banners/Create.cshtml`
  - `Areas/Admin/Views/Banners/Edit.cshtml`
  - `Areas/Admin/ViewModels/Banners/AdminBannerFormViewModel.cs`
  - `Areas/Admin/Controllers/BannersController.cs`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Small
- **Acceptance Criteria**:
  - Banner lists, actions, validation errors, and success popups are localized.

---

### 5. FaqController
- **Related Views**:
  - `Areas/Admin/Views/Faq/Index.cshtml`
  - `Areas/Admin/Views/Faq/Create.cshtml`
  - `Areas/Admin/Views/Faq/Edit.cshtml`
- **Remaining Localization Tasks**:
  - Add `faq` node to JSON files.
  - Translate grid headers, form controls, and title attributes.
  - Localize validation messages in `AdminFaqFormViewModel.cs`.
  - Translate FAQ CRUD feedback in `FaqController.cs`.
- **Files To Update**:
  - `Areas/Admin/Views/Faq/Index.cshtml`
  - `Areas/Admin/Views/Faq/Create.cshtml`
  - `Areas/Admin/Views/Faq/Edit.cshtml`
  - `Areas/Admin/ViewModels/FAQ/AdminFaqFormViewModel.cs`
  - `Areas/Admin/Controllers/FaqController.cs`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Small
- **Acceptance Criteria**:
  - FAQ Index list and edit forms translate completely.

---

### 6. AnalyticsController
- **Related Views**:
  - `Areas/Admin/Views/Analytics/Index.cshtml`
- **Remaining Localization Tasks**:
  - Add `analytics` node to JSON files.
  - Localize summary titles, trend statements, chart names, rank metrics, and table indicators.
  - Localize export success alerts in `AnalyticsController.cs`.
- **Files To Update**:
  - `Areas/Admin/Views/Analytics/Index.cshtml`
  - `Areas/Admin/Controllers/AnalyticsController.cs`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Medium
- **Acceptance Criteria**:
  - Date ranges, data tables, and chart indicators render localized strings.

---

### 7. AuditLogsController
- **Related Views**:
  - `Areas/Admin/Views/AuditLogs/Index.cshtml`
  - `Areas/Admin/Views/AuditLogs/Details.cshtml`
- **Remaining Localization Tasks**:
  - Add `auditLogs` node to translation files.
  - Translate filters labels, date pickers, table grids, logs detail labels, role names, and action strings.
- **Files To Update**:
  - `Areas/Admin/Views/AuditLogs/Index.cshtml`
  - `Areas/Admin/Views/AuditLogs/Details.cshtml`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Medium
- **Acceptance Criteria**:
  - Search fields, actions dropdowns, and details logs list correctly translate values.

---

### 8. ReportsController
- **Related Views**:
  - `Areas/Admin/Views/Reports/Index.cshtml`
- **Remaining Localization Tasks**:
  - Add `reports` node to JSON files.
  - Translate panel layouts and reports titles.
- **Files To Update**:
  - `Areas/Admin/Views/Reports/Index.cshtml`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Small
- **Acceptance Criteria**:
  - Reports panel reads correctly in both English and Arabic.

---

### 9. ReviewsController
- **Related Views**:
  - `Areas/Admin/Views/Reviews/Index.cshtml`
  - `Areas/Admin/Views/Reviews/Details.cshtml`
  - `Areas/Admin/Views/Reviews/Reported.cshtml`
- **Remaining Localization Tasks**:
  - Add `reviews` node to translation files.
  - Translate columns headers, actions labels, search inputs, modal titles, and delete confirmation boxes.
- **Files To Update**:
  - `Areas/Admin/Views/Reviews/Index.cshtml`
  - `Areas/Admin/Views/Reviews/Details.cshtml`
  - `Areas/Admin/Views/Reviews/Reported.cshtml`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Medium
- **Acceptance Criteria**:
  - All review index pages, reports lists, and comment listings translate dynamically.

---

### 10. SettingsController
- **Related Views**:
  - `Areas/Admin/Views/Settings/Index.cshtml`
- **Remaining Localization Tasks**:
  - Add `settings` node to translation files.
  - Localize configuration forms, input fields, labels, placeholders, and buttons.
- **Files To Update**:
  - `Areas/Admin/Views/Settings/Index.cshtml`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Small
- **Acceptance Criteria**:
  - Configuration parameters are fully translated.

---

### 11. UsersController
- **Related Views**:
  - `Areas/Admin/Views/Users/Index.cshtml`
  - `Areas/Admin/Views/Users/Details.cshtml`
- **Remaining Localization Tasks**:
  - Add `users` node to translation files.
  - Localize headers, data grids, status badges, details tags, and actions.
- **Files To Update**:
  - `Areas/Admin/Views/Users/Index.cshtml`
  - `Areas/Admin/Views/Users/Details.cshtml`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Medium
- **Acceptance Criteria**:
  - User records, profiles details, and activation badges render localized text.

---

### 12. HomeController
- **Related Views**:
  - `Views/Home/Index.cshtml`
- **Remaining Localization Tasks**:
  - Translate the landing page welcoming message and link texts.
- **Files To Update**:
  - `Views/Home/Index.cshtml`
  - `Graduation-infrastructure/Localization/localize_en.json`
  - `Graduation-infrastructure/Localization/localize_ar.json`
- **Estimated Effort**: Small
- **Acceptance Criteria**:
  - Landing welcoming layout translates completely.

---

## Phase 4: Shared Components Plan

A complete plan for all shared modules, layouts, errors, and validation mechanisms.

---

### _Layout.cshtml (Main Site Layout)
- **Current Status**: Not Started
- **Missing Work**:
  - Inject `ILocalizationService`.
  - Fetch culture, inject LTR/RTL dir and lang attributes dynamically in `<html>`.
  - Localize brand header, navigation links ("Home", "Products"), and copywriter footer.
- **Files To Modify**:
  - `Views/Shared/_Layout.cshtml`

---

### Navbar (_Navbar.cshtml)
- **Current Status**: Completed
- **Missing Work**: None.
- **Files To Modify**: None.

---

### Sidebar (_Sidebar.cshtml)
- **Current Status**: Completed
- **Missing Work**: None.
- **Files To Modify**: None.

---

### Footer (Embedded in Layouts)
- **Current Status**: Not Started
- **Missing Work**:
  - Extract and localize copyright texts and built-by indicators inside `_AdminLayout.cshtml` (line 83) and `_Layout.cshtml` (line 40-44).
- **Files To Modify**:
  - `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
  - `Views/Shared/_Layout.cshtml`

---

### Shared Components
- **Current Status**: Not Started
- **Missing Work**:
  - **`_Filters.cshtml`**: Localize button texts "Reset" (line 56) and "Apply Filters" (line 60). Make sure filter fields' labels and placeholders are translated before rendering.
  - **`_StatusBadge.cshtml`**: Localize `displayText` dynamically (using key prefix `badge.status.`) to translate statuses like "Active", "Inactive", "Approved", "Suspended" into Arabic/English.
- **Files To Modify**:
  - `Areas/Admin/Views/Shared/_Filters.cshtml`
  - `Areas/Admin/Views/Shared/_StatusBadge.cshtml`

---

### Partial Views
- **Current Status**: Not Started
- **Missing Work**:
  - **`_Pagination.cshtml`**: Convert page numbers to Arabic-Indic digits in Arabic culture using a formatting utility. Ensure arrow icons flip depending on HTML dir.
  - **`_SearchBar.cshtml`**: Ensure search placeholders and input labels display localized values.
- **Files To Modify**:
  - `Areas/Admin/Views/Shared/_Pagination.cshtml`
  - `Areas/Admin/Views/Shared/_SearchBar.cshtml`

---

### Error Pages
- **Current Status**: Not Started
- **Missing Work**:
  - **`Error.cshtml`**: Inject `ILocalizationService` and translate error description, request details, and developer warning messages.
- **Files To Modify**:
  - `Views/Shared/Error.cshtml`

---

### Validation Messages
- **Current Status**: Not Started
- **Missing Work**:
  - Setup translation adapters/custom tags to read validation messages from the JSON provider, or reference custom localization properties instead of hardcoded strings in ViewModels (`Areas/Admin/ViewModels`).
- **Files To Modify**:
  - ViewModels in `Areas/Admin/ViewModels` (`Banners`, `Categories`, `FAQ`, `Auth`).

---

### Toast Notifications
- **Current Status**: Not Started (handled via TempData / SweetAlert2 / Alerts)
- **Missing Work**:
  - Localize the alerts and notifications layout helper tags or SweetAlert2 parameters in views (e.g. `Vendors/Details.cshtml`).
- **Files To Modify**:
  - `Areas/Admin/Views/Shared/_Alerts.cshtml`
  - JavaScript sections in views that utilize alert dialogues.

---

## Phase 5: Language Switcher Review

The language switcher uses client-side cookie injection alongside a database preference endpoint.

### Verification Matrix
- **Culture Cookie**: Reads cookie `"culture"`; verified to support `en` and `ar`. SameSite option has been removed to allow instant transmission.
- **RequestLocalization**: Standard MVC localization parses cookies middleware-level (inside `Program.cs` lines 80-95), setting `CultureInfo.CurrentCulture` and `CultureInfo.CurrentUICulture` correctly.
- **Language Persistence**: When logged in, updates `ApplicationUser.PreferredLanguage` in DB. When logged out, relies on cookie persistence.
- **Arabic / English Support**: Fully verified.
- **RTL / LTR Switching**: Fully functional; loads the corresponding RTL Bootstrap and custom styles.

### Remaining Infrastructure Tasks
1. Map dynamic route parameters correctly during language updates to prevent lost parameters upon reload.
2. Setup a fall-back fallback middleware if the database preference query crashes on startup.

---

## Phase 6: RTL/LTR Audit

### Alignment & Styling Issues
1. **Forms Alignment**: Input fields in Arabic culture require `text-align: right;` and `direction: rtl;`. While `_LayoutAuth.cshtml` contains this fix, `_AdminLayout.cshtml` lacks proper default form control styles for Arabic.
2. **Table Alignment**: Numeric values (prices, counts) and table headers need to align to the right in RTL. Tables currently default to standard Bootstrap layouts which can lead to columns text overlapping in Arabic.
3. **Sidebar / Navigation Problems**: The admin sidebar is offset to the left. In RTL, it must shift to the right. Correct overrides are defined in `_AdminLayout.cshtml` but need thorough validation.
4. **Modals Position**: Modal popup buttons and footers need to flip orientation in RTL.

### Files Requiring Styling Overrides
- `Graduation-MVC/Areas/Admin/Views/Shared/_AdminStyles.cshtml` (add core RTL overrides for forms, datatables, and cards)
- `Graduation-MVC/Areas/Admin/Views/Shared/_AdminLayout.cshtml`

---

## Phase 7: Execution Roadmap

To ensure a solid workflow, follow this strict completion path:

### Phase 1: Localization Infrastructure & Shared Layouts
1. Standardize form and table RTL styles inside `_AdminStyles.cshtml` and `_AdminLayout.cshtml`.
2. Translate all hardcoded labels in `_Filters.cshtml`, `_StatusBadge.cshtml`, `_Pagination.cshtml`, and `_AdminLayout.cshtml` footer.
3. Localize default error pages (`Error.cshtml` and root `_Layout.cshtml`).

### Phase 2: Category & FAQ Localization (Low Complexity)
1. Localize `CategoriesController` (Form ViewModels, Views, and controller actions).
2. Localize `FaqController` (Form ViewModels, Views, and controller actions).

### Phase 3: Banners & Settings Localization (Low Complexity)
1. Localize `BannersController` (ViewModels, Views, and controller actions).
2. Localize `SettingsController` and `ReportsController`.

### Phase 4: AuditLogs & Analytics (Medium Complexity)
1. Localize `AuditLogsController` (translate action logs list and detail tables).
2. Localize `AnalyticsController` (translate chart descriptors and reports tables).

### Phase 5: Reviews & Users (Medium Complexity)
1. Localize `ReviewsController` (moderation logs, reported views).
2. Localize `UsersController` (customer lists, details).

### Phase 6: Vendors Localization (High Complexity due to Verification flows)
1. Localize `VendorsController` (Statistics cards, Filter dropdowns, verification histories, modals, and SweetAlert2 prompt triggers).

### Phase 7: Final Validation
1. Verify database culture updates.
2. Perform visual checks of LTR/RTL alignments across all modules.

---

## Phase 8: Progress Checklist

- [ ] Localization Infrastructure & RTL Styles Verified
- [ ] Shared Layout & Components Completed
- [ ] HomeController Completed
- [ ] CategoriesController Completed
- [ ] FaqController Completed
- [ ] BannersController Completed
- [ ] SettingsController Completed
- [ ] ReportsController Completed
- [ ] AuditLogsController Completed
- [ ] AnalyticsController Completed
- [ ] ReviewsController Completed
- [ ] UsersController Completed
- [ ] VendorsController Completed
