# 📢 Notification System & Language Preference — Frontend Integration Guide

> **الهدف:** هذا الملف يشرح لفريق الفرونت كل حاجة محتاجين يعرفوها عن نظام الإشعارات (Real-Time + REST) ونظام تحديد اللغة للمستخدم.

---

## 🗂️ جدول المحتويات

1. [نظرة عامة على المعمارية](#architecture-overview)
2. [نظام اللغة — Language Preference](#language-system)
3. [نظام الإشعارات — REST API](#notifications-rest-api)
4. [الإشعارات الفورية — SignalR](#signalr-real-time)
5. [أنواع الإشعارات](#notification-types)
6. [شكل البيانات — Data Shapes](#data-shapes)
7. [ملاحظات مهمة](#important-notes)

---

## 🏗️ Architecture Overview

```
┌──────────────────────────────────────────────────────────┐
│                      BACKEND                             │
│                                                          │
│  [Service Layer]                                         │
│    ├── LanguageUserService   → يحدد لغة المستخدم         │
│    └── InternalNotificationService                       │
│          ├── يحفظ الإشعار بالعربي + الإنجليزي في DB     │
│          ├── يجيب لغة المستخدم من DB (PreferredLanguage) │
│          ├── يبعت الإشعار باللغة الصح عبر SignalR        │
│          └── يرجّع بيانات REST بلغة المستخدم            │
│                                                          │
│  [Database]                                              │
│    ├── InternalNotification (TitleAr/TitleEn/MsgAr/MsgEn)│
│    └── ApplicationUser.PreferredLanguage ("ar" | "en")   │
└──────────────────────────────────────────────────────────┘
                          │
              REST API + SignalR Hub
                          │
┌──────────────────────────────────────────────────────────┐
│                     FRONTEND                             │
│   ├── يطلب /api/internal-notifications  → يستقبل بلغة User│
│   ├── يطلب /api/user-language PUT       → يغير اللغة     │
│   └── يتصل بـ SignalR Hub              → يستقبل Push     │
└──────────────────────────────────────────────────────────┘
```

> **القاعدة الأساسية:** البيكاند هو المسؤول **بالكامل** عن تحديد اللغة. الفرونت **لا يحتاج** يبعت header اللغة أو يختار. كل بيانات API والإشعارات بتيجي تلقائياً بلغة المستخدم المخزنة في DB.

---

## 🌐 Language System

### ما هو نظام اللغة؟

كل مستخدم له `PreferredLanguage` مخزنة في قاعدة البيانات (القيمة الافتراضية `"ar"`).
الفرونت ممكن يجيب اللغة الحالية أو يغيرها عبر API.

### اللغات المدعومة

| Code | اللغة   |
|------|---------|
| `ar` | العربية |
| `en` | English |

---

### 📌 Endpoints — Language

#### `GET /api/user-language`
**جيب اللغة الحالية للمستخدم**

- **Auth:** ✅ مطلوب (JWT Bearer Token)
- **Body:** لا يوجد

**Response `200 OK`:**
```json
{
  "language": "ar"
}
```

---

#### `PUT /api/user-language`
**غيّر لغة المستخدم**

- **Auth:** ✅ مطلوب (JWT Bearer Token)
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "language": "en"
}
```

| الحقل      | النوع    | القيم المسموحة     | وصف            |
|------------|----------|--------------------|----------------|
| `language` | `string` | `"ar"` أو `"en"`  | اللغة الجديدة  |

**Response `200 OK`:**
```json
{
  "message": "Language updated successfully",
  "language": "en"
}
```

**متى تستخدمه؟**
- لما المستخدم يضغط على زر تغيير اللغة في الإعدادات
- بعد ما يتم الـ PUT بنجاح، **كل** الإشعارات والبيانات القادمة هتيجي بالغة الجديدة تلقائياً

---

## 🔔 Notifications REST API

**Base URL:** `/api/internal-notifications`  
**Auth:** ✅ JWT Bearer Token مطلوب على كل الـ Endpoints

---

### `GET /api/internal-notifications`
**جيب إشعارات المستخدم (Paginated)**

**Query Parameters:**

| Parameter  | النوع  | الافتراضي | وصف              |
|------------|--------|-----------|------------------|
| `page`     | `int`  | `1`       | رقم الصفحة       |
| `pageSize` | `int`  | `10`      | عدد الإشعارات    |

**مثال:**
```
GET /api/internal-notifications?page=1&pageSize=10
```

**Response `200 OK`:**
```json
{
  "items": [
    {
      "id": 1,
      "userId": "abc-123",
      "title": "تم تأكيد الطلب",
      "message": "تم تأكيد طلبك رقم #456",
      "isRead": false,
      "createdAt": "2026-06-17T11:30:00Z"
    },
    {
      "id": 2,
      "userId": "abc-123",
      "title": "طلب جديد",
      "message": "تم استلام طلبك رقم #457 وهو قيد المراجعة",
      "isRead": true,
      "createdAt": "2026-06-16T09:15:00Z"
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3
}
```

> ⚠️ **ملاحظة:** الـ `title` و `message` بتيجوا **باللغة المخزنة للمستخدم** تلقائياً. مش محتاج تعمل أي حاجة.

---

### `GET /api/internal-notifications/unread-count`
**جيب عدد الإشعارات غير المقروءة**

**Response `200 OK`:**
```json
{
  "unreadCount": 5
}
```

**متى تستخدمه؟**
- لعرض الـ Badge على أيقونة الجرس 🔔

---

### `PUT /api/internal-notifications/{id}/read`
**علّم إشعار معين كمقروء**

| Parameter | النوع | وصف           |
|-----------|-------|---------------|
| `id`      | `int` | ID الإشعار    |

**مثال:**
```
PUT /api/internal-notifications/1/read
```

**Response `200 OK`:**
```json
{
  "message": "Notification marked as read"
}
```

**Errors:**
- `404 Not Found` → الإشعار مش موجود
- `401 Unauthorized` → الإشعار مش خاص بالمستخدم ده

---

### `PUT /api/internal-notifications/read-all`
**علّم كل الإشعارات كمقروءة**

**Response `200 OK`:**
```json
{
  "message": "All notifications marked as read"
}
```

---

## ⚡ SignalR Real-Time

### الاتصال بالـ Hub

**Hub URL:** `/hubs/notifications`

**مهم جداً:** الـ SignalR بيحتاج JWT token في الـ Query String مش في الـ Header.

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://YOUR_API_URL/hubs/notifications", {
    accessTokenFactory: () => localStorage.getItem("token") // أو أي مكان عندك الـ token
  })
  .withAutomaticReconnect()
  .build();
```

---

### الاستماع للإشعارات

بعد ما تعمل connect، اشترك على الـ event ده:

```javascript
connection.on("ReceiveNotification", (notification) => {
  console.log("New notification:", notification);
  // notification شكله:
  // {
  //   id: 1,
  //   title: "تم تأكيد الطلب",
  //   message: "تم تأكيد طلبك رقم #123",
  //   isRead: false,
  //   createdAt: "2026-06-17T11:30:00Z"
  // }
});

await connection.start();
```

---

### مثال كامل (React)

```javascript
import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

function useNotifications(token) {
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("https://YOUR_API_URL/hubs/notifications", {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveNotification", (notification) => {
      // أضف الإشعار الجديد للقائمة
      setNotifications((prev) => [notification, ...prev]);
      // زوّد عداد الغير مقروءة
      setUnreadCount((prev) => prev + 1);
    });

    connection.start().catch(console.error);

    return () => {
      connection.stop();
    };
  }, [token]);

  return { notifications, unreadCount };
}
```

---

### شكل بيانات SignalR Notification

```json
{
  "id": 7,
  "title": "تم إلغاء الطلب",
  "message": "تم إلغاء طلبك رقم #789",
  "isRead": false,
  "createdAt": "2026-06-17T14:00:00Z"
}
```

> ✅ البيانات بتيجي **باللغة الصح** للمستخدم تلقائياً.  
> ❌ مفيش `userId` في بيانات SignalR عشان الاتصال ده private لكل user.

---

## 📋 Notification Types

دي كل أنواع الإشعارات اللي ممكن توصل للمستخدم:

### إشعارات العميل (Customer)

| النوع                | العنوان (AR)       | العنوان (EN)         | الرسالة (AR)                              |
|---------------------|--------------------|----------------------|-------------------------------------------|
| `OrderPending`      | طلب جديد           | New Order            | تم استلام طلبك رقم #X وهو قيد المراجعة   |
| `OrderConfirmed`    | تم تأكيد الطلب    | Order Confirmed      | تم تأكيد طلبك رقم #X                     |
| `OrderInProgress`   | جاري تنفيذ الطلب   | Order In Progress    | جاري العمل على طلبك رقم #X               |
| `OrderReadyForPickup`| الطلب جاهز        | Ready for Pickup     | طلبك رقم #X جاهز للاستلام                |
| `OrderDelivered`    | تم التسليم         | Order Delivered      | تم تسليم طلبك رقم #X بنجاح              |
| `OrderCancelled`    | تم إلغاء الطلب    | Order Cancelled      | تم إلغاء طلبك رقم #X                    |
| `PasswordReset`     | تم تغيير كلمة المرور| Password Reset      | تم إعادة تعيين كلمة المرور بنجاح         |

### إشعارات البائع (Vendor)

| النوع                        | العنوان (AR)           | العنوان (EN)           | الرسالة (AR)                            |
|-----------------------------|------------------------|------------------------|------------------------------------------|
| `NewOrder`                  | طلب جديد               | New Order              | لديك طلب جديد رقم #X                    |
| `NewReview`                 | تقييم جديد             | New Review             | قام عميل بتقييم منتجك                    |
| `AccountApproved`           | تم تفعيل الحساب        | Account Approved       | تم تفعيل حسابك من الإدارة               |
| `VendorAccountRejected`     | تم رفض الحساب          | Account Rejected       | تم رفض طلب تفعيل حسابك من الإدارة      |
| `VendorAccountSuspended`    | تم إيقاف الحساب        | Account Suspended      | تم إيقاف حسابك مؤقتًا من الإدارة       |
| `VendorAccountReactivated`  | تم إعادة تفعيل الحساب  | Account Reactivated    | تم إعادة تفعيل حسابك من الإدارة        |
| `VendorOrderCancelled`      | تم إلغاء الطلب         | Order Cancelled        | تم إلغاء الطلب رقم #X من قبل العميل    |

---

## 📦 Data Shapes

### `InternalNotificationDto` (الشكل الكامل لإشعار واحد)

```typescript
interface InternalNotificationDto {
  id: number;
  userId: string;
  title: string;      // باللغة المناسبة للـ user
  message: string;    // باللغة المناسبة للـ user
  isRead: boolean;
  createdAt: string;  // ISO 8601 UTC
}
```

### `PaginatedResult<InternalNotificationDto>` (الصفحات)

```typescript
interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

### `UserLanguageDto`

```typescript
interface UserLanguageDto {
  language: "ar" | "en";
}
```

### `UpdateUserLanguageDto`

```typescript
interface UpdateUserLanguageDto {
  language: "ar" | "en";
}
```

---

## ⚠️ Important Notes

### 1. اللغة بتتحدد من البيكاند
الفرونت **لا يحتاج** يبعت أي header زي `Accept-Language`. البيكاند بيجيب لغة المستخدم من DB تلقائياً عند كل request.

### 2. تحديث الإشعارات بعد تغيير اللغة
لو المستخدم غير اللغة، الإشعارات القديمة اللي موجودة على الشاشة بتتحدث في الـ **Request القادم** لـ `GET /api/internal-notifications`. الـ SignalR بيبعت الإشعارات الجديدة بالغة الجديدة مباشرة.

### 3. Token في SignalR
الـ SignalR بيستخدم JWT token في الـ **Query String** وليس الـ Header، لكن مكتبة SignalR بتعمل ده تلقائياً لما تستخدم `accessTokenFactory`.

### 4. كل User بياخد إشعاراته بس
الـ SignalR Hub بيبعت لكل مستخدم إشعاراته الخاصة فقط (Group per userId). مفيش أي خطر إن User يشوف إشعارات User تاني.

### 5. الـ Pagination
- أقل قيمة لـ `page` هي `1`
- أقل قيمة لـ `pageSize` هي `1`
- لو بعتت `page=0` أو `pageSize=0` هيرجع `400 Bad Request`

---

## 🔐 Authentication Summary

كل الـ Endpoints محتاجة JWT Token في الـ Header:

```
Authorization: Bearer <your_jwt_token>
```

وفي SignalR، الـ Token بيتبعت في الـ Query String (المكتبة بتعمل ده تلقائياً):
```
wss://YOUR_API_URL/hubs/notifications?access_token=<your_jwt_token>
```

---

## 📡 Quick Reference

| الوظيفة                      | Method | URL                                       |
|-----------------------------|--------|-------------------------------------------|
| جيب لغة المستخدم             | GET    | `/api/user-language`                      |
| غيّر لغة المستخدم            | PUT    | `/api/user-language`                      |
| جيب الإشعارات (Paginated)   | GET    | `/api/internal-notifications`             |
| جيب عدد الغير مقروءة        | GET    | `/api/internal-notifications/unread-count`|
| علّم إشعار كمقروء            | PUT    | `/api/internal-notifications/{id}/read`   |
| علّم الكل كمقروء             | PUT    | `/api/internal-notifications/read-all`    |
| اتصال SignalR                | WS     | `/hubs/notifications`                     |
| Event الإشعار الفوري         | —      | `ReceiveNotification`                     |

---

*آخر تحديث: 17 يونيو 2026*
