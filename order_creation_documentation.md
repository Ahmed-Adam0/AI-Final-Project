# Create Order API Documentation (Frontend Integration)

This document describes the updated payload requirements for the **Create Order** endpoint (`POST /api/Order`).

---

## Endpoint Specification

- **HTTP Method:** `POST`
- **URL Path:** `/api/Order`
- **Authentication:** Required (`Bearer <Token>`)
- **Content-Type:** `application/json`

---

## Request Body Schema (JSON)

| Field | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `firstName` | `string` | **Yes** | First name of the customer. Stored in the `Orders` table. |
| `lastName` | `string` | **Yes** | Last name of the customer. Stored in the `Orders` table. |
| `email` | `string` | **Yes** | Email address of the customer. Stored in the `Orders` table. |
| `phoneNumber` | `string` | **Yes** | Phone number of the customer. Stored in the `Orders` table. |
| `address` | `string` | **Yes** | Primary delivery address. Stored as a new record in the `Addresses` table and on the Order. |
| `secondaryAddress`| `string` | No | Optional additional address. Stored as a new record in the `Addresses` table. |
| `notes` | `string` | No | Optional delivery notes or instructions. |

### Example Request Payload

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+2 Egypt numbers / 01234567890",
  "address": "Cairo, Nasr City, Abbas El Akkad St, Building 12",
  "secondaryAddress": "Giza, Dokki, Tahrir St, Building 5",
  "notes": "Please ring the bell twice."
}
```

---

## Response Body Schema (JSON)

- **HTTP Status Code:** `200 OK` (on success) or `400 BadRequest` (on validation/logic error)

### Example Success Response

```json
{
  "message": "Order created successfully",
  "data": {
    "id": 18,
    "userId": "87c4da9d-efbc-4d87-bc5b-43958742ba3b",
    "totalPrice": 1250.00,
    "status": "Pending",
    "address": "Cairo, Nasr City, Abbas El Akkad St, Building 12",
    "phoneNumber": "+2 Egypt numbers / 01234567890",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "notes": "Please ring the bell twice.",
    "createdAt": "2026-06-22T18:31:02.483Z",
    "vendorOrders": [
      {
        "id": 34,
        "status": "Pending",
        "estimatedDeliveryDate": null,
        "canApprove": false,
        "totalPrice": 1250.00,
        "items": [
          {
            "id": 45,
            "productId": 8,
            "productNameEn": "Wooden Chair",
            "productNameAr": "كرسي خشبي",
            "status": "Pending",
            "unitPrice": 625.00,
            "quantity": 2,
            "attributes": []
          }
        ]
      }
    ],
    "statusHistory": null,
    "paymentUrl": null,
    "paymentStatus": "Unpaid"
  }
}
```
