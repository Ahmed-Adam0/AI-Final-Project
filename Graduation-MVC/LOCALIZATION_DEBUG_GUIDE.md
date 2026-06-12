# 🔍 Localization Debug Guide

## How to Test the Fix

### Step 1: Build the Solution
```powershell
dotnet build
```

### Step 2: Run the Application
- Start the MVC application in Debug mode
- Navigate to: `/Admin/Account/Login`

### Step 3: Open Developer Tools
- Press **F12** to open DevTools
- Go to **Console** tab

### Step 4: Test Language Switch

#### Before Clicking:
- Look at console (should be clean initially)
- Observe current text (English)

#### Click Arabic Button:
1. You should see console logs:
   ```
   Language cookie set: ar culture=ar
   Reloading page with culture: ar
   (browser reloads)

   🍪 [LOCALIZATION] Culture from cookie: ar
   📂 [LOCALIZATION] Loading file: C:\...\localize_ar.json
   📋 [LOCALIZATION] Original JSON length: 1234, Cleaned: 1150
   ✅ [LOCALIZATION] Loaded 11 keys for culture: ar
   (... multiple ✓ Found entries ...)
   ```

2. Page should:
   - ✅ Change to Arabic text (لغة عربية)
   - ✅ Switch direction to RTL
   - ✅ Load RTL Bootstrap CSS

### Step 5: What to Look For

| Symptom | Cause |
|---------|-------|
| ❌ Texts stay English | 🔴 Cookie wasn't sent OR culture not detected |
| ❌ `❌ [LOCALIZATION] Key not found` | 🔴 JSON file isn't loaded or parsed |
| ❌ `💥 [LOCALIZATION] Error loading` | 🔴 JSON syntax error |
| ✅ All Arabic texts appear | ✅ Fix working! |

### Step 6: Debug Console Logs

Open **Output Window** in Visual Studio to see backend logs:

```
View → Output → Debug (dropdown)
```

You should see:
```
🍪 [LOCALIZATION] Culture from cookie: ar
📂 [LOCALIZATION] Loading file: ...
✅ [LOCALIZATION] Loaded 11 keys for culture: ar
✓ [LOCALIZATION] Found 'Email' in ar: البريد الإلكتروني
✓ [LOCALIZATION] Found 'Password' in ar: كلمة المرور
...
```

---

## Common Issues & Fixes

### Issue 1: Console Error in JavaScript
**Symptom**: Error message in browser console
**Solution**: Check `setLanguage()` function syntax - ensure backticks are correct

### Issue 2: Cookie Not Persisting
**Symptom**: Logs show "Culture from cookie: ar" but page stays English
**Solution**: JSON parsing failed. Check `[LOCALIZATION] Error loading` logs

### Issue 3: Page Doesn't Reload
**Symptom**: Cookie set but page doesn't change
**Solution**: Check if `setTimeout` and `location.reload()` are working. Try F5 manual refresh.

### Issue 4: RTL Works but Text is English
**Symptom**: `dir="rtl"` set correctly but text is English
**Solution**: This means culture detection failed. Check cookie is actually being sent in request.

---

## Step-by-Step Debug Trace

1. **User clicks Arabic button** (client-side)
   ```
   ✓ setLanguage('ar') called
   ✓ Cookie set: culture=ar
   ✓ location.reload() triggered
   ```

2. **Browser sends reload request** (with cookie header)
   ```
   Request Headers:
   Cookie: culture=ar
   ```

3. **Server receives request** (server-side)
   ```
   🍪 [LOCALIZATION] Culture from cookie: ar
   ```

4. **Load translations** (server-side)
   ```
   🔄 [LOCALIZATION] Cache miss for ar, loading...
   📂 [LOCALIZATION] Loading file: localize_ar.json
   ✅ [LOCALIZATION] Loaded 11 keys for culture: ar
   ```

5. **Render view** (server-side)
   ```
   ✓ [LOCALIZATION] Found 'Email' in ar: البريد الإلكتروني
   ✓ [LOCALIZATION] Found 'Password' in ar: كلمة المرور
   ```

6. **Page displays** (client-side)
   ```
   ✅ Page shows Arabic text
   ✅ RTL direction applied
   ```

---

## Network Tab Check

1. Open **DevTools → Network**
2. Set language to Arabic
3. Look for the **reload request** (usually `Login`)
4. Check **Request Headers → Cookie:**
   - Should show: `culture=ar` ✅

If cookie is missing, that's the problem!

---

## If Still Having Issues

Provide these logs:
1. Browser **Console output** after clicking language button
2. Visual Studio **Output window → Debug** logs
3. The **error message** if any

Then we can pinpoint the exact issue! 🎯
