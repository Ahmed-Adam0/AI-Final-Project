# دليل ربط الـ Real-time Notifications (SignalR) مع Angular

هذا الدليل يوضح لمطور واجهة المستخدم (Frontend) كيفية ربط نظام الإشعارات الفورية (SignalR) في تطبيق Angular مع الـ Backend.

---

## 1. تثبيت المكتبة المطلوبة

يجب تثبيت حزمة SignalR الرسمية الخاصة بـ Microsoft:

```bash
npm install @microsoft/signalr
```

---

## 2. إعداد خدمة الإشعارات (SignalR Service)

قم بإنشاء خدمة (Service) في Angular لإدارة دورة حياة الاتصال مع الـ Hub ومتابعة الأحداث.

### تفاصيل الاتصال:
- **رابط الـ Hub (Hub URL)**: `/hubs/notifications`
- **التوثيق (Authentication)**: يجب تمرير رمز الـ JWT (Token) عبر خاصية `accessTokenFactory`.
- **دورة حياة الاتصال (Connection Lifecycle)**:
  - **عند تسجيل الدخول (Login)**: يتم بدء الاتصال بالـ Hub.
  - **عند تسجيل الخروج (Logout)**: يتم إيقاف الاتصال وتنظيف البيانات.

### مثال لكود الخدمة في Angular (TypeScript):

```typescript
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

// واجهة تمثل هيكل الإشعار المستلم
export interface RealtimeNotification {
  title: string;
  message: string;
  createdAt: string; // ISO date string
}

@Injectable({
  providedIn: 'root'
})
export class NotificationHubService {
  private hubConnection: signalR.HubConnection | null = null;
  
  // مصدر بيانات للإشعارات الحالية لتسهيل عرضها في المكونات (Components)
  private notificationSubject = new BehaviorSubject<RealtimeNotification[]>([]);
  public notifications$: Observable<RealtimeNotification[]> = this.notificationSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * تشغيل الاتصال مع الـ SignalR Hub.
   * استدعي هذه الدالة مباشرة بعد نجاح تسجيل الدخول (Login).
   */
  public startConnection(): void {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return; // الاتصال نشط بالفعل
    }

    const token = localStorage.getItem('token');
    if (!token) {
      console.warn('تعذر بدء اتصال SignalR: لم يتم العثور على JWT token في localStorage.');
      return;
    }

    // إعداد الاتصال بالـ Hub وتفعيل إعادة الاتصال التلقائي
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    // الاستماع لحدث استقبال إشعار جديد
    this.hubConnection.on('ReceiveNotification', (data: RealtimeNotification) => {
      console.log('تم استقبال إشعار جديد:', data);
      
      // تحديث القائمة الحالية بإضافة الإشعار الجديد في البداية
      const currentList = this.notificationSubject.value;
      this.notificationSubject.next([data, ...currentList]);

      // هنا يمكنك إظهار Toast notification للمستخدم أو تشغيل صوت تنبيه أو تحديث عداد جرس الإشعارات
    });

    // بدء تشغيل الاتصال
    this.hubConnection
      .start()
      .then(() => console.log('تم الاتصال بـ SignalR بنجاح.'))
      .catch((err) => console.error('خطأ أثناء بدء اتصال SignalR: ', err));
  }

  /**
   * إيقاف الاتصال بالـ Hub.
   * استدعي هذه الدالة عند تسجيل الخروج (Logout) أو تدمير المكون.
   */
  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => {
        console.log('تم إيقاف اتصال SignalR.');
        this.hubConnection = null;
        this.notificationSubject.next([]); // تفريغ قائمة الإشعارات
      });
    }
  }

  /**
   * جلب الإشعارات القديمة المخزنة في قاعدة البيانات.
   * استدعي هذه الدالة فور تسجيل الدخول لعرض تاريخ الإشعارات القديمة بجرس الإشعارات.
   */
  public loadOldNotifications(page: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get<any>(`/api/internal-notifications?page=${page}&pageSize=${pageSize}`);
  }
}
```

---

## 3. خطوات الربط العملي في الواجهة

### الخطوة الأولى: عند تسجيل الدخول الناجح
1. قم بحفظ رمز الـ JWT في الـ `localStorage`.
2. استدعِ دالة `loadOldNotifications()` لجلب الإشعارات القديمة من الـ API وعرضها في جرس الإشعارات.
3. استدعِ دالة `startConnection()` لتفعيل الاستماع الفوري للإشعارات الجديدة.

### الخطوة الثانية: استقبال الإشعارات الفورية
- يقوم الـ Backend بإرسال الإشعارات الجديدة للعميل عبر الحدث المسمى: **`"ReceiveNotification"`**.
- **شكل البيانات المرسلة (Payload JSON)**:
  ```json
  {
    "title": "عنوان الإشعار",
    "message": "نص الإشعار والتفاصيل",
    "createdAt": "2026-06-14T21:55:00Z"
  }
  ```

### الخطوة الثالثة: عند تسجيل الخروج (Logout)
1. استدعِ دالة `stopConnection()` لقطع الاتصال بالخادم بشكل صحيح.
2. قم بحذف الـ token من الـ `localStorage`.

---

## 4. روابط الـ REST API الإضافية الخاصة بالإشعارات

يمكنك استخدام المسارات التالية لإدارة حالة الإشعارات للمستخدم:
- **جلب الإشعارات (مع الترقيم الصفحي)**: `GET /api/internal-notifications?page=1&pageSize=10`
- **جلب عدد الإشعارات غير المقروءة**: `GET /api/internal-notifications/unread-count`
- **تحديد إشعار معين كمقروء**: `PUT /api/internal-notifications/{id}/read`
- **تحديد كل الإشعارات كمقروءة**: `PUT /api/internal-notifications/read-all`

---

## 5. ملاحظات هامة للمطور

- **توجيه الإشعارات**: يتم ربط الاتصال تلقائياً بمجموعة (Group) خاصة بالـ `userId` المستخرج من الـ JWT token، لذلك تصل الإشعارات فقط للمستخدم المعني دون غيره.
- **التوثيق عبر الـ WebSockets**: متصفحات الويب لا تدعم إرسال Custom Headers أثناء مصافحة الـ WebSockets، لذلك تمرر مكتبة SignalR الـ Token تلقائياً كمتغير في الرابط (`access_token?`). وقد تم إعداد الـ Backend للتعامل مع هذا السلوك بشكل كامل وتلقائي.
