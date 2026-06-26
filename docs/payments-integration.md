# دليل تكامل واجهة المستخدم (Frontend Integration Guide)
## نظام الدفع المؤجل وتجميع الطلبات (Deferred Payment & MasterOrder Aggregation)

يوضح هذا المستند التعديلات والخطوات البرمجية اللازمة لربط الواجهة الأمامية (Frontend) مع التحديثات الجديدة لنظام الدفع والتفاوض.

---

### 1. الحالات الجديدة لطلب البائع (Updated VendorOrder Statuses)

تم تعديل دورة حياة طلبات البائع لتشمل مرحلة "انتظار الدفع" `PendingPayment` واستبدال حالة `ReadyForPickup` بحالة `Shipped` لتصبح كالتالي:

1. **`Pending`**: الطلب بانتظار تقديم مقترح تاريخ التوصيل من البائع (لم يدفع العميل بعد).
2. **`AwaitingCustomerApproval`**: البائع قدم مقترح التوصيل وبانتظار قرار العميل.
3. **`PendingPayment`**: وافق العميل على تاريخ التوصيل وبانتظار إتمام الدفع.
4. **`Confirmed`**: تم سداد قيمة الطلب بنجاح وهو الآن جاهز لبدء العمل.
5. **`InProgress`**: الطلب قيد التصنيع والتحضير.
6. **`Shipped`**: تم شحن الطلب وتسليمه لشركة الشحن.
7. **`Delivered`**: تم تسليم الطلب للعميل نهائياً.
8. **`Cancelled`**: تم إلغاء الطلب (سواء لرفض العميل أو انتهاء مهلة الـ 48 ساعة SLA).

---

### 2. دورة العمل المحدثة (System Flow)

```text
[Checkout (Pending)] 
       ↓
[Vendor Proposes Date (AwaitingCustomerApproval)]
       ↓
[Customer Approves Date (PendingPayment)]
       ↓
[Customer clicks 'Pay' -> Aggregates and creates Paymob Session]
       ↓
[Payment Success Callback (Confirmed)]
       ↓
[Fulfillment: InProgress -> Shipped -> Delivered]
```

* **ملحوظة هامة للفرونت إند:** عند قيام العميل بإنشاء الطلب (Checkout) عبر الأكشن `POST /api/Order` **لا يتم طلب الدفع** وتعود قيمة حقل `paymentUrl` بـ `null`.

---

### 3. نقاط الاتصال البرمجية (API Endpoints)

#### أ. الاستعلام عن تفاصيل طلبات العميل
عند طلب تفاصيل الطلب عبر الأكشن `GET /api/Order/{id}` أو `GET /api/Order/my-orders`:
* يمكنك تمكين زر دفع الرصيد المتبقي (Pay Remaining Balance) إذا كان هناك رصيد متبقي غير مدفوع لطلب واحد أو أكثر من طلبات البائعين المؤهلة (في حالات `Confirmed` أو `InProgress` أو `Shipped` أو `Delivered`).


---

#### ب. سداد الرصيد المتبقي للطلب الرئيسي (Pay Master Order Remaining Balance)
عندما يرغب العميل في سداد المبلغ المتبقي بالكامل أو جزء منه، يتم استدعاء الأكشن التالي للحصول على رابط بوابة الدفع Paymob:

* **المسار (Route):** `POST /api/payments/paymob/initiate-masterorder`
* **التفويض (Headers):** `Authorization: Bearer <JWT_TOKEN>`
* **جسم الطلب (Request Body - JSON):**
  ```json
  {
    "masterOrderId": 12,               // رقم الطلب الرئيسي (Master Order ID)
    "amount": 500.00                   // اختياري: قيمة الدفعة الجزئية المراد سدادها. في حال عدم إرسالها، سيتم دفع كامل المبلغ المتبقي لطلبات البائعين المؤهلة.
  }
  ```
* **الرد الناجح (Response - 200 OK):**
  ```json
  {
    "paymentUrl": "https://accept.paymob.com/api/acceptance/iframes/12345?payment_token=xxxxxx..."
  }
  ```
  *(يجب إعادة توجيه العميل إلى الـ `paymentUrl` لإتمام الدفع).*

---

### 4. طريقة حساب قيمة الفاتورة وتوزيع الدفع
يقوم النظام تلقائياً وبشكل آمن على السيرفر بفلترة وحساب القيمة الإجمالية المتبقية والغير مدفوعة لطلبات البائعين المؤهلة (التي تجاوزت مرحلة الموافقة وأصبحت حالتها: `Confirmed` أو `InProgress` أو `Shipped` أو `Delivered`).
* **استثناء الطلبات غير المؤهلة:** يتم تماماً استثناء أي طلبات بائع ما زالت في حالة `Pending` أو `AwaitingCustomerApproval` أو `PendingPayment`.
* **توزيع الدفعة:** عند سداد المبلغ (سواءً كان كاملاً أو جزءاً مخصصاً عبر حقل `amount`)، يتم توزيع القيمة المدفوعة على الأقساط والطلبات المؤهلة بحسب الأولوية (أقساط الشحن `Shipped` أولاً، ثم أقساط التسليم `Delivered` ثانياً). وفي حال سداد جزء لا يغطي كامل القسط، يتم تقسيم القسط تلقائياً.
