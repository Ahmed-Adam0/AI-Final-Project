# Frontend Integration: Google Profile Control System (Angular/React)

This document provides instructions for integrating the backend-driven UI control system for user profiles. The backend now returns dynamic control flags to indicate if a profile belongs to a Google Sign-In user and whether fields like the email address should be editable.

---

## 1. API Contract Updates (`UserProfileDto`)

When fetching the user profile using `GET /api/profile`, the backend includes two new boolean flags in the response:
- `isGoogleUser` (boolean): `true` if the user registered and logs in via Google.
- `canEditEmail` (boolean): `true` if the user is a standard account, and `false` if they signed up via Google.

### Example JSON Payload:
```json
{
  "id": "user-uuid-12345",
  "fullName": "Jane Doe",
  "email": "janedoe@gmail.com",
  "profileImage": "https://lh3.googleusercontent.com/...",
  "preferredLanguage": "en",
  "phoneNumber": "+1234567890",
  "userName": "janedoe@gmail.com",
  "isGoogleUser": true,
  "canEditEmail": false
}
```

---

## 2. UI Binding Guidelines

### A. Disable Email Editing
You must dynamically disable or mark the email input field as read-only based on the `canEditEmail` flag.

#### Angular Example (Reactive Forms):
```typescript
// Inside your component.ts after fetching the profile:
this.profileForm = this.fb.group({
  fullName: [profile.fullName, Validators.required],
  email: [{ value: profile.email, disabled: !profile.canEditEmail }, [Validators.required, Validators.email]],
  phoneNumber: [profile.phoneNumber],
  preferredLanguage: [profile.preferredLanguage]
});
```

#### Angular Template (HTML):
```html
<div class="form-group">
  <label for="email">Email Address</label>
  <input type="email" id="email" formControlName="email" class="form-control" />
  <small *ngIf="!profileForm.get('email').enabled" class="text-muted">
    Registered via Google. Email cannot be edited.
  </small>
</div>
```

#### React Example (useState/Formik):
```jsx
// Disable using the standard readOnly or disabled attribute
<input
  type="email"
  value={profile.email}
  disabled={!profile.canEditEmail}
  className="form-control"
/>
```

---

## 3. Password Features Controls

Since Google accounts do not use local password validation or resets:
1. **Hide Change Password Buttons:** If `isGoogleUser === true`, do not render the "Change Password" section in the user profile dashboard.
2. **Handle Password Action Blocks:** The backend will return a `400 Bad Request` if a Google user manually attempts to call change password or password reset endpoints.

---

## 4. Error Handling
If a user tries to modify their email (e.g. by manipulating the DOM or directly submitting via API), the backend will reject the update with a `400 Bad Request` containing:
`"Google-authenticated users cannot change their email address manually."`

Ensure your interceptors or services display this error message directly to the user.
