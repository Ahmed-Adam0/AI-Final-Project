# توثيق تعديلات بيانات العميل في طلبات البائع (Vendor Orders)

تم تحديث الـ APIs الخاصة بطلبات البائع (Vendor Orders) في الباكيند لترجع بيانات العميل الحقيقية المستلمة أثناء عملية الدفع (Checkout) مع تطبيق قواعد خصوصية على رقم الهاتف.

---

## 1. الحقول المضافة/المحدثة في الاستجابة (API Response Fields)

في الـ DTOs التالية:
* `VendorOrderDashboardDto` (الخاص بقائمة الطلبات وفلترتها)
* `VendorOrderDetailsDto` (الخاص بتفاصيل الطلب الفردي)

تم ربط الحقول التالية بالقيم الحقيقية من قاعدة البيانات:

| اسم الحقل في الاستجابة (JSON Property) | نوع البيانات (Type) | الوصف (Description) |
| :--- | :--- | :--- |
| `customerName` | `string` | الاسم الكامل للعميل (دمج `FirstName` و `LastName`). وفي حال عدم إدخالهما، يتم الرجوع لـ `FullName` الخاص بحسابه كقيمة احتياطية. |
| `address` | `string` | عنوان التوصيل الفعلي الذي أدخله العميل أثناء الدفع. |
| `notes` | `string` | الملاحظات المرفقة بالطلب من قبل العميل. |
| `customerPhone` | `string` | رقم هاتف العميل (**يخضع لقواعد الخصوصية أدناه**). |

---

## 2. قاعدة خصوصية رقم الهاتف (Phone Number Privacy Rule)

لحماية خصوصية العميل، **لا يتم إرجاع رقم الهاتف للبائع إلا عند شحن الطلب**.

* **إذا كانت حالة الطلب (`status`) هي `Shipped` أو `Delivered`**:
  سيقوم الباكيند بإرجاع رقم الهاتف الفعلي للعميل في الحقل `customerPhone`.
* **إذا كانت حالة الطلب أي حالة أخرى (مثل `Pending`, `Confirmed`, `InProgress`, `Cancelled` ...إلخ)**:
  سيقوم الباكيند بإرجاع نص فارغ `""` في حقل `customerPhone`.

---

## 3. الـ APIs المتأثرة بالتعديل (Affected Endpoints)

تم تطبيق التعديلات بالكامل على مسارات الـ API التالية:

1. **جلب وتصفية الطلبات للبائع:**
   * **المسار:** `POST /api/VendorOrders/orders/filter`
   * **الـ DTO المرجوع:** قائمة من `VendorOrderDashboardDto`

2. **جلب تفاصيل طلب معين للبائع:**
   * **المسار:** `GET /api/VendorOrders/orders/{orderId}`
   * **الـ DTO المرجوع:** `VendorOrderDetailsDto`

3. **تقرير النشاط للطلبات:**
   * **المسار:** `GET /api/VendorOrders/reports/activity`
   * **الـ DTO المرجوع:** يحتوي على `CustomerName` المحدث.

---

## 4. ملاحظات لمطور الفرونت إند (Frontend Integration Notes)

* **التعامل مع الواجهة (UI Bindings):**
  الواجهة الأمامية الحالية مهيأة بالفعل لاستقبال هذه الحقول وعرضها:
  * يتم عرض الاسم عبر `order.customer.fullName`.
  * يتم عرض العنوان عبر `order.shippingAddress.addressLine1`.
  * يتم عرض رقم الهاتف عبر `order.customer.phone`.
* **حالة إخفاء الرقم:**
  بما أن الباكيند يرجع نصاً فارغاً `""` عندما لا يكون الطلب مشحوناً، فإن كود الـ HTML الحالي بالـ Angular سيتكفل بإخفائه تلقائياً بسبب شرط التحقق من القيمة:
  ```html
  @if (order()!.customer.phone) {
    <div class="vod-info-secondary">{{ order()!.customer.phone }}</div>
  }
  ```
