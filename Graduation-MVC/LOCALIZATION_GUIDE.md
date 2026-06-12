# 🌐 Admin Login Localization System - Documentation

## ✅ Overview
تم تطوير نظام محلي (Localization) متكامل للـ Admin Login يدعم اللغة الإنجليزية والعربية مع تفعيل RTL (Right-to-Left) تلقائياً.

---

## 📋 Components

### 1. **LocalizationService** (`Graduation-infrastructure/Localization/LocalizationService.cs`)
- **Responsibility**: إدارة قراءة واستخراج النصوص المترجمة من ملفات JSON
- **Priority**: 
  1. Cookie (الخيار السريع)
  2. Database (تفضيل المستخدم - إن كان مسجل دخول)
  3. Default: `en` (الافتراضي)

#### Methods:
- `GetCurrentCulture()` - الحصول على اللغة الحالية
- `Get(string key)` - الحصول على النص المترجم
- `SetCultureAsync(string culture)` - تعيين اللغة وحفظها في Cookie والقاعدة

### 2. **JSON Files** (`Graduation-infrastructure/Localization/`)
- `localize_en.json` - Texts بالإنجليزية
- `localize_ar.json` - Texts بالعربية

```json
{
  "AdminLogin": "Admin Login / تسجيل الدخول",
  "Email": "Email / البريد الإلكتروني",
  "Password": "Password / كلمة المرور",
  "RememberMe": "Remember me / تذكرني",
  "Login": "Login / دخول",
  "ForgotPassword": "Forgot password? / نسيت كلمة المرور؟",
  "Language": "Language / اللغة",
  "English": "English / إنجليزي",
  "Arabic": "Arabic / عربي"
}
```

### 3. **Layout** (`_LayoutAuth.cshtml`)
- تعين `lang` و `dir` ديناميكياً بناءً على اللغة الحالية
- تحميل RTL Bootstrap CSS تلقائياً للعربية
- Styling للـ RTL inputs

```html
<html lang="@htmlLang" dir="@htmlDir">
  @if (isArabic) {
	<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.rtl.min.css" />
  }
</html>
```

### 4. **Login View** (`Login.cshtml`)
- جميع النصوص تعتمد على `LocalizationService.Get()`
- Language Dropdown مع JavaScript فوري للتبديل
- RTL support تلقائي عبر Layout

### 5. **Controller Action** (`AccountController.SetLanguage`)
```csharp
[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> SetLanguage(string culture)
{
	await _localizationService.SetCultureAsync(culture);
	return NoContent();
}
```

---

## 🔄 Language Switching Flow

### Step 1️⃣: User clicks Language Button
```html
<button onclick="setLanguage('ar')">Arabic</button>
```

### Step 2️⃣: JavaScript Sets Cookie
```javascript
function setLanguage(culture) {
	// Set cookie for 1 year
	const expiryDate = new Date();
	expiryDate.setFullYear(expiryDate.getFullYear() + 1);
	document.cookie = `culture=${culture};expires=${expiryDate.toUTCString()};path=/;SameSite=Lax`;

	// Reload page
	setTimeout(() => { location.reload(); }, 100);
}
```

### Step 3️⃣: Backend Updates Database (if logged in)
- `SetCultureAsync()` calls `SetCultureAsync()` which updates user's `PreferredLanguage`

### Step 4️⃣: Page Reloads with New Language
- `LocalizationService.GetCurrentCulture()` reads the new cookie
- Layout loads RTL CSS if Arabic
- All text is rendered in the correct language

---

## 🎨 RTL Support

### Automatic RTL Features:
✅ HTML `dir="rtl"` attribute  
✅ Bootstrap RTL CSS (`bootstrap.rtl.min.css`)  
✅ Form inputs with `direction: rtl; text-align: right;`  
✅ Icon positioning adjusted  
✅ Text alignment flipped  

### CSS in Layout:
```css
@if (isArabic) {
	.form-control, .input-group-text, .btn { 
		direction: rtl; 
		text-align: right; 
	}
	input::placeholder { text-align: right; }
}
```

---

## 📝 Usage in Views

### Inject Service:
```html
@inject ILocalizationService LocalizationService
```

### Use in Templates:
```html
<label>@LocalizationService.Get("Email")</label>
<input placeholder="@LocalizationService.Get("Email")" />
```

### Get Current Culture:
```csharp
@{
	var currentCulture = LocalizationService.GetCurrentCulture();
	var isArabic = currentCulture == "ar";
}
```

---

## 🔐 DI Registration

In `ServiceMVC.cs`:
```csharp
services.AddScoped<ILocalizationService, LocalizationService>();
services.AddHttpContextAccessor();
```

---

## ✨ Key Benefits

| Feature | Benefit |
|---------|---------|
| **No RESX** | أبسط، أسهل للتحديث |
| **JSON Localization** | مرن وسهل التعديل |
| **Instant Switch** | تحويل لحظي بدون Submit |
| **RTL Auto** | دعم كامل للعربية |
| **Database Persistence** | تذكر تفضيل المستخدم |
| **Cookie Fallback** | عمل سريع بدون DB |

---

## 🚀 Testing

1. Navigate to Admin Login: `/Admin/Account/Login`
2. Click Language Dropdown
3. Select "Arabic"
4. ✅ Page should:
   - Switch to Arabic text
   - Change direction to RTL
   - Load RTL Bootstrap CSS
   - Reload smoothly

---

## 📂 File Structure

```
Graduation-infrastructure/Localization/
├── LocalizationService.cs          (Main Service)
├── localize_en.json                (English Texts)
└── localize_ar.json                (Arabic Texts)

Graduation-MVC/Areas/Admin/
├── Views/
│   ├── Account/
│   │   └── Login.cshtml            (Localized Login)
│   └── Shared/
│       └── _LayoutAuth.cshtml      (RTL Layout)
└── Controllers/
	└── AccountController.cs        (SetLanguage Action)
```

---

## ⚠️ Important Notes

- ❌ **NO hardcoded strings** in views
- ❌ **NO RESX files**
- ✅ **All text via** `LocalizationService.Get()`
- ✅ **Language preference saved** in Cookie + Database
- ✅ **RTL support** automatic for Arabic

---

**Status**: ✅ Ready for Production
