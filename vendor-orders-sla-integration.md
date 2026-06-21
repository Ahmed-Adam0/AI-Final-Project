# دليل تكامل الواجهة الأمامية (Frontend Integration Guide)
## نظام جدولة التوصيل والإلغاء التلقائي (Delivery Date Negotiation & SLA Auto-Cancellation)

يوضح هذا المستند جميع التغييرات والتحديثات الخاصة بالواجهة الخلفية (Backend API) لتسهيل ربطها مع الـ Frontend.

---

### 1. حالات الطلب الجديدة (Vendor Order Statuses)
تم تحديث حالات طلب البائع (`VendorOrderStatus`) لتشمل حالات التفاوض والتجهيز التالية:
* **`Pending`**: الطلب بانتظار تقديم مقترح تاريخ التوصيل من البائع.
* **`AwaitingCustomerApproval`**: قدم البائع مقترح تاريخ التوصيل وبانتظار موافقة أو رفض العميل.
* **`Confirmed`**: وافق العميل على تاريخ التوصيل وبدأ العمل على الطلب.
* **`InProgress`**: الطلب قيد التصنيع/التجهيز.
* **`ReadyForPickup`**: الطلب جاهز للاستلام.
* **`Delivered`**: تم تسليم الطلب بنجاح (حالة نهائية).
* **`Cancelled`**: تم إلغاء الطلب (حالة نهائية).

---

### 2. دورة حياة الطلب والتفاوض (Workflow Lifecycle)

1. **إنشاء الطلب:** يبدأ الطلب بحالة `Pending` ويسجل تاريخ الإنشاء `CreatedAt`.
2. **مقترح البائع:** يقوم البائع بتقديم مقترح تاريخ التوصيل، لتتحول حالة الطلب إلى `AwaitingCustomerApproval`.
3. **قرار العميل:**
   * **القبول (Approve):** تتحول حالة الطلب إلى `Confirmed`.
   * **الرفض (Reject):** **يتم إلغاء الطلب فورًا وبشكل نهائي** وتتحول حالته إلى `Cancelled`.
4. **اتفاقية مستوى الخدمة (48h Global SLA):** 
   * أي طلب لا يتم تسليمه (`Delivered`) أو إلغاؤه (`Cancelled`) خلال **48 ساعة** من وقت إنشائه (`CreatedAt`) سيقوم النظام **بإلغائه تلقائيًا**.

---

### 3. نقاط الاتصال البرمجية (API Endpoints)

#### أ. تقديم مقترح تاريخ التوصيل (خاص بالبائع)
* **المسار (Route):** `PUT /api/VendorOrders/orders/{orderId}/propose-date`
* **المعاملات (URL Params):** 
  * `orderId` (int) - رقم طلب البائع.
* **جسم الطلب (Request Body):**
  ```json
  {
    "estimatedDeliveryDate": "2026-06-25T12:00:00Z"
  }
  ```
* **الرد الناجح (Response - 200 OK):**
  ```json
  {
    "message": "Delivery date proposed successfully."
  }
  ```

---

#### ب. قبول مقترح تاريخ التوصيل (خاص بالعميل)
* **المسار (Route):** `PUT /api/Order/vendor-orders/{vendorOrderId}/approve`
* **المعاملات (URL Params):**
  * `vendorOrderId` (int) - رقم طلب البائع.
* **الرد الناجح (Response - 200 OK):**
  ```json
  {
    "message": "Delivery date approved successfully."
  }
  ```

---

#### ج. رفض مقترح تاريخ التوصيل (خاص بالعميل)
* **المسار (Route):** `PUT /api/Order/vendor-orders/{vendorOrderId}/reject`
* **المعاملات (URL Params):**
  * `vendorOrderId` (int) - رقم طلب البائع.
* **الرد الناجح (Response - 200 OK):** (ملاحظة: هذا الإجراء يلغي الطلب تلقائيًا)
  ```json
  {
    "message": "Schedule rejected. Order cancelled."
  }
  ```

---

### 4. الإشعارات الفورية (SignalR & Notifications)
يعتمد النظام على SignalR لبث الإشعارات وتحديث الحالات فورًا عبر مسار الـ Hub التالي: `/hubs/notifications`.

أنواع الإشعارات المرسلة (`NotificationType`):
* **`DeliveryDateProposed`**: يُرسل للعميل عندما يقترح البائع تاريخًا للتوصيل.
* **`DeliveryDateApproved`**: يُرسل للبائع عندما يوافق العميل على التاريخ المقترح.
* **`DeliveryDateRejected`**: يُرسل للبائع عند قيام العميل برفض مقترح تاريخ التوصيل (وإلغاء الطلب).
* **`OrderCancelled`**: يُرسل للعميل عند إلغاء الطلب تلقائيًا نتيجة انتهاء مهلة الـ 48 ساعة.
* **`VendorOrderCancelled`**: يُرسل للبائع عند إلغاء الطلب تلقائيًا نتيجة انتهاء مهلة الـ 48 ساعة.

#### شكل البيانات المرسلة عبر SignalR:
```json
{
  "id": 123,
  "userId": "customer-or-vendor-user-id",
  "title": "تم إلغاء الطلب / Order Cancelled",
  "message": "Your order #15 has been cancelled / تم إلغاء طلبك رقم 15",
  "isRead": false,
  "createdAt": "2026-06-21T21:48:30Z"
}
```

---

### 5. تحديث نماذج البيانات (DTO Modifications)
تم إضافة الحقل التالي في استجابة تفاصيل الطلب لمعرفة تاريخ التوصيل المقترح:
* **`estimatedDeliveryDate`**: (DateTime?, nullable) - تاريخ التوصيل المتوقع إن وجد.
