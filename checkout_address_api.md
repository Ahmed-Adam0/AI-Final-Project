# Checkout API Address Integration Guide (Frontend)

To prevent duplicate customer addresses during checkout, the **Create Order** endpoint (`POST /api/Order`) has been updated to accept optional address identifiers.

---

## Endpoint Specification

- **HTTP Method:** `POST`
- **URL Path:** `/api/Order`
- **Authentication:** Required (`Bearer <Token>`)
- **Content-Type:** `application/json`

---

## Updated Request Body Schema (JSON)

We added two new **optional** fields to the payload to support reusing existing saved addresses:

| Field | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `firstName` | `string` | **Yes** | First name of the customer. |
| `lastName` | `string` | **Yes** | Last name of the customer. |
| `email` | `string` | **Yes** | Email address of the customer. |
| `phoneNumber` | `string` | **Yes** | Phone number of the customer. |
| `address` | `string` | **Yes** | Shipping address (used as fallback text if `addressId` is not found/provided). |
| **`addressId`** | `integer` | No | **[NEW]** The ID of the selected primary saved address from the customer's profile. |
| `secondaryAddress`| `string` | No | Optional additional address (used as fallback text if `secondaryAddressId` is not found/provided). |
| **`secondaryAddressId`**| `integer` | No | **[NEW]** The ID of the selected secondary saved address from the customer's profile. |
| `notes` | `string` | No | Optional delivery notes or instructions. |

---

## Frontend Integration Scenarios

### Scenario 1: Customer selects an existing saved address from their profile
When a customer selects a saved address from their profile list, the frontend should send its `id` in the `addressId` field. 

> [!NOTE]
> Even when sending `addressId`, the `address` string field is still required by the API schema (you can pass the selected address text or a descriptive string).

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+201234567890",
  "address": "Cairo, Nasr City, Abbas El Akkad St, Building 12",
  "addressId": 14,
  "notes": "Please ring the bell twice."
}
```

---

### Scenario 2: Customer enters a completely new address
When a customer inputs a brand-new address at checkout, omit the `addressId` field (or set it to `null`). The API will save the new address to the customer's saved addresses list automatically.

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+201234567890",
  "address": "Giza, Dokki, Tahrir St, Building 5",
  "notes": "Leave with receptionist."
}
```

---

### Scenario 3: Customer selects both primary and secondary saved addresses
If the customer specifies a secondary address from their saved addresses, pass the `secondaryAddressId` field.

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+201234567890",
  "address": "Cairo, Nasr City, Abbas El Akkad St, Building 12",
  "addressId": 14,
  "secondaryAddress": "Giza, Dokki, Tahrir St, Building 5",
  "secondaryAddressId": 18,
  "notes": ""
}
```
