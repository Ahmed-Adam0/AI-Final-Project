# Multi-Vendor Orders & Privacy Integration Documentation

This document contains integration guidelines, endpoint updates, and response schema changes related to the **Multi-Vendor Order splitting** and **Customer Privacy** implementation.

---

## 1. Important Structural Changes (Breaking Changes)

> [!WARNING]
> The `statusHistory` property in both **Customer** and **Vendor** order response models has changed from an **Array / List** to a **Single Object** (or `null` if empty).

---

## 2. API Endpoints for Vendors

### 2.1. Filter & Search Vendor Orders (Paginated List)
* **Endpoint**: `POST /api/VendorOrders/orders/filter`
* **Headers**: `Authorization: Bearer <token>`
* **Request Body Example**:
```json
{
  "status": "Pending",
  "startDate": null,
  "endDate": null,
  "customerName": null,
  "sortBy": "createdAt",
  "sortDescending": true,
  "pageNumber": 1,
  "pageSize": 10
}
```
* **Response Example (`data` contains only the items and calculations for this vendor)**:
```json
{
  "data": [
    {
      "id": 15,
      "masterOrderId": 8,
      "customerName": "Customer",          // Hardcoded placeholder for privacy
      "customerPhone": "01xxxxxxxxx",       // Masked phone number for privacy
      "totalPrice": 350.00,                 // Sum of this vendor's items only (Item.UnitPrice * Item.Quantity)
      "status": "Pending",                  // Specific vendor order status
      "address": "",                        // Empty string for privacy
      "notes": "",                          // Empty string for privacy
      "createdAt": "2026-06-19T14:35:00Z",
      "updatedAt": null,
      "itemCount": 2,                       // Total sum of quantities of items belonging to this vendor
      "items": [                            // List contains ONLY products belonging to this vendor
        {
          "productId": 5,
          "productName": "Wooden Desk",
          "unitPrice": 150.00,
          "quantity": 1,
          "total": 150.00
        },
        {
          "productId": 6,
          "productName": "Office Chair",
          "unitPrice": 200.00,
          "quantity": 1,
          "total": 200.00
        }
      ],
      "statusHistory": {                    // SINGLE OBJECT (Latest transition only)
        "id": 45,
        "oldStatus": "",
        "newStatus": "Pending",
        "createdAt": "2026-06-19T14:35:00Z"
      }
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

---

### 2.2. Get Single Vendor Order Details
* **Endpoint**: `GET /api/VendorOrders/orders/{orderId}`
* **Headers**: `Authorization: Bearer <token>`
* **Response Example**:
```json
{
  "id": 15,
  "masterOrderId": 8,
  "userId": "",
  "customerName": "Customer",              // Hardcoded placeholder for privacy
  "customerPhone": "01xxxxxxxxx",           // Masked phone number for privacy
  "totalPrice": 350.00,                     // Sum of this vendor's items only
  "status": "Pending",
  "address": "",                            // Empty string for privacy
  "notes": "",                              // Empty string for privacy
  "createdAt": "2026-06-19T14:35:00Z",
  "updatedAt": null,
  "items": [                                // List contains ONLY products belonging to this vendor
    {
      "productId": 5,
      "productName": "Wooden Desk",
      "unitPrice": 150.00,
      "quantity": 1,
      "total": 150.00
    },
    {
      "productId": 6,
      "productName": "Office Chair",
      "unitPrice": 200.00,
      "quantity": 1,
      "total": 200.00
    }
  ],
  "statusHistory": {                        // SINGLE OBJECT (Latest transition only)
    "id": 45,
    "oldStatus": "",
    "newStatus": "Pending",
    "createdAt": "2026-06-19T14:35:00Z"
  }
}
```

---

## 3. Customer-Facing Response Changes

The customer order retrieval endpoints (e.g. `GET /api/Orders` or `GET /api/Orders/{id}`) will also return the `statusHistory` as a **single object** containing the latest chronological state update across all split vendor orders.

* **Response example fragment (`statusHistory` is an object, not a list)**:
```json
{
  "id": 8,
  "totalPrice": 1500.00,
  "status": "In Progress",
  "statusHistory": {
    "id": 102,
    "oldStatus": "Pending",
    "newStatus": "Processing",
    "createdAt": "2026-06-19T18:28:20Z"
  },
  "paymentStatus": "Paid"
}
```
