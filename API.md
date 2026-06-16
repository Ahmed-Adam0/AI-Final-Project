# Vendor Marketplace API Documentation

> **Target Audience:** Frontend Development Team  
> **Version:** 1.0.0

---

## 1. INTRODUCTION

Welcome to the Vendor Marketplace API Documentation. This system is a **Vendor-Driven Marketplace** built using ASP.NET Core Web API with an Onion Architecture. 

### Architecture Summary
The backend follows Onion Architecture principles, ensuring a clean separation of concerns among the Domain, Application, Infrastructure, and Presentation (API/MVC) layers. However, as frontend consumers, you only interact with the Presentation layer (the RESTful APIs).

### Marketplace Behavior (Vendor-Driven & Instant Publish)
The marketplace is designed to empower vendors. A critical feature of this system is **instant publishing**:
- When a vendor creates a product, it is immediately live and visible to customers.
- There is **no approval workflow** or pending status.
- Administrators perform **post-moderation** only (they can hide/unhide products if they violate platform policies).
- Pricing is highly dynamic, calculated as `BasePrice + Options (PriceDelta)`.

---

## 2. AUTHENTICATION & AUTHORIZATION

All protected endpoints require a standard JSON Web Token (JWT) provided in the `Authorization` header.

**Header Format:**
```http
Authorization: Bearer <your_jwt_token>
```

### Roles
The system utilizes Role-Based Access Control (RBAC) with three primary roles:
1. **Customer**: Can browse products, manage their cart, and place orders.
2. **Vendor**: Can manage their own products, materials, and options.
3. **Admin**: Can perform post-moderation (hide/unhide) and view all platform data.

---

## 3. PRODUCT APIs

Products are the core entities in the system. **Remember: Products are instantly visible to the public upon creation.**

### `POST /api/products`
Creates a new product. (Requires **Vendor** role).

**Request Body:**
```json
{
  "name": "Ergonomic Office Chair",
  "description": "Adjustable chair with lumbar support.",
  "basePrice": 199.99,
  "productTypeId": 45
}
```

**Response (201 Created):**
```json
{
  "id": 101,
  "vendorId": 5,
  "name": "Ergonomic Office Chair",
  "description": "Adjustable chair with lumbar support.",
  "basePrice": 199.99,
  "productTypeId": 45,
  "isHidden": false,
  "createdAt": "2026-06-16T12:00:00Z"
}
```

### `GET /api/products`
Retrieves a paginated list of public products. (Publicly accessible).

**Request Parameters:**
- `page` (optional, default: 1)
- `pageSize` (optional, default: 10)
- `categoryId` (optional)
- `vendorId` (optional)

**Response (200 OK):**
```json
{
  "totalItems": 150,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 101,
      "vendorId": 5,
      "name": "Ergonomic Office Chair",
      "basePrice": 199.99
    }
  ]
}
```

### `GET /api/products/{id}`
Retrieves detailed information about a specific product, including available options. (Publicly accessible).

**Response (200 OK):**
```json
{
  "id": 101,
  "vendorId": 5,
  "name": "Ergonomic Office Chair",
  "description": "Adjustable chair with lumbar support.",
  "basePrice": 199.99,
  "productTypeId": 45,
  "materials": [
    {
      "materialId": 10,
      "name": "Fabric Type",
      "options": [
        { "id": 301, "name": "Standard Mesh", "priceDelta": 0 },
        { "id": 302, "name": "Premium Leather", "priceDelta": 50.00 }
      ]
    }
  ]
}
```

### `PUT /api/products/{id}`
Updates an existing product's details. (Requires **Vendor** role, must own the product).

**Request Body:**
```json
{
  "name": "Ergonomic Office Chair V2",
  "description": "Updated adjustable chair with better lumbar support.",
  "basePrice": 219.99
}
```

**Response (200 OK):**
```json
{
  "id": 101,
  "name": "Ergonomic Office Chair V2",
  "basePrice": 219.99
}
```

### `DELETE /api/products/{id}`
Deactivates or deletes a product. (Requires **Vendor** role, must own the product).

**Response (204 No Content):**
*(No body returned)*

---

## 4. VENDOR MATERIAL / OPTIONS APIs

Vendors can manage reusable attributes or materials that act as customizable options for their products. These options can alter the final price using a `PriceDelta`.

### `POST /api/materials`
Creates a new material group (e.g., "Wood Type", "Fabric Color"). (Requires **Vendor** role).

**Request Body:**
```json
{
  "name": "Wood Type"
}
```

**Response (201 Created):**
```json
{
  "id": 40,
  "vendorId": 5,
  "name": "Wood Type"
}
```

### `POST /api/materials/{materialId}/options`
Adds a specific option to a material group. (Requires **Vendor** role).

**Request Body:**
```json
{
  "name": "Premium Walnut Finish",
  "priceDelta": 150.00
}
```

**Response (201 Created):**
```json
{
  "id": 305,
  "materialId": 40,
  "name": "Premium Walnut Finish",
  "priceDelta": 150.00
}
```

### `GET /api/vendors/{vendorId}/materials`
Retrieves all materials and options for a specific vendor.

**Response (200 OK):**
```json
[
  {
    "id": 40,
    "name": "Wood Type",
    "options": [
      { "id": 305, "name": "Premium Walnut Finish", "priceDelta": 150.00 },
      { "id": 306, "name": "Standard Oak", "priceDelta": 0 }
    ]
  }
]
```

### `DELETE /api/options/{optionId}`
Deletes an option. (Requires **Vendor** role).

**Response (204 No Content):**
*(No body returned)*

---

## 5. CATEGORY APIs

The platform uses a strict hierarchy for categorization to facilitate AI suggestions and precise filtering.

**Hierarchy:** `Category` ➔ `SubCategory` ➔ `ProductType` ➔ `Product`

### `GET /api/categories`
Retrieves top-level categories.

**Response (200 OK):**
```json
[
  { "id": 1, "name": "Furniture" },
  { "id": 2, "name": "Electronics" }
]
```

### `GET /api/categories/{categoryId}/subcategories`
Retrieves subcategories belonging to a specific category.

**Response (200 OK):**
```json
[
  { "id": 10, "categoryId": 1, "name": "Living Room" },
  { "id": 11, "categoryId": 1, "name": "Office" }
]
```

### `GET /api/subcategories/{subCategoryId}/producttypes`
Retrieves product types belonging to a specific subcategory.

**Response (200 OK):**
```json
[
  { "id": 45, "subCategoryId": 11, "name": "Desks" },
  { "id": 46, "subCategoryId": 11, "name": "Chairs" }
]
```

---

## 6. CART APIs

The shopping cart relies on dynamic pricing based on the selected options. The cart stores the references (`OptionIds`) and calculates the total price on the fly.

### `POST /api/cart/items`
Adds a product to the cart with the user's chosen options. (Requires **Customer** role).

**Request Body:**
```json
{
  "productId": 101,
  "quantity": 2,
  "selectedOptionIds": [302] 
}
```

**Response (201 Created):**
```json
{
  "cartItemId": 501,
  "productId": 101,
  "quantity": 2,
  "selectedOptionIds": [302],
  "calculatedUnitPrice": 249.99,
  "totalPrice": 499.98
}
```

### `PUT /api/cart/items/{itemId}`
Updates the quantity or selected options for an existing cart item.

**Request Body:**
```json
{
  "quantity": 3,
  "selectedOptionIds": [301]
}
```

**Response (200 OK):**
```json
{
  "cartItemId": 501,
  "quantity": 3,
  "calculatedUnitPrice": 199.99,
  "totalPrice": 599.97
}
```

### `DELETE /api/cart/items/{itemId}`
Removes an item from the cart.

**Response (204 No Content):**
*(No body returned)*

### `GET /api/cart`
Retrieves the user's current cart, including computed dynamic prices.

**Response (200 OK):**
```json
{
  "cartId": 99,
  "totalCartValue": 599.97,
  "items": [
    {
      "cartItemId": 501,
      "productId": 101,
      "productName": "Ergonomic Office Chair",
      "quantity": 3,
      "calculatedUnitPrice": 199.99,
      "options": [
        { "name": "Standard Mesh", "priceDelta": 0 }
      ]
    }
  ]
}
```

---

## 7. ORDER APIs

When a cart is checked out, an order is created. 
**Crucial Concept - Snapshots:** Orders are immutable. The system takes a permanent snapshot of the product's `BasePrice`, the selected options' `PriceDelta`, and the final calculated total at the exact moment of checkout. Future changes to vendor prices will *not* affect past orders.

### `POST /api/orders`
Converts the active cart into a confirmed order. (Requires **Customer** role).

**Request Body:**
```json
{
  "shippingAddressId": 12,
  "paymentMethodId": 3
}
```

**Response (201 Created):**
```json
{
  "orderId": 9001,
  "status": "Pending",
  "totalAmount": 599.97
}
```

### `GET /api/orders`
Retrieves the logged-in user's order history.

**Response (200 OK):**
```json
[
  {
    "orderId": 9001,
    "orderDate": "2026-06-16T12:00:00Z",
    "totalAmount": 599.97,
    "status": "Pending"
  }
]
```

### `GET /api/orders/{orderId}`
Retrieves detailed snapshot data for a specific order.

**Response (200 OK):**
```json
{
  "orderId": 9001,
  "orderDate": "2026-06-16T12:00:00Z",
  "totalAmount": 599.97,
  "status": "Pending",
  "items": [
    {
      "productName": "Ergonomic Office Chair",
      "quantity": 3,
      "snapshotBasePrice": 199.99,
      "snapshotOptions": [
        { "name": "Standard Mesh", "priceDelta": 0 }
      ],
      "finalUnitPrice": 199.99,
      "totalItemPrice": 599.97
    }
  ]
}
```

---

## 8. ADMIN APIs

Administrators monitor the marketplace and handle violations via post-moderation. They do not approve products beforehand.

### `PATCH /api/admin/products/{id}/hide`
Hides a product from public view. (Requires **Admin** role).

**Request Body:**
```json
{
  "reason": "Violates terms of service regarding restricted materials."
}
```

**Response (200 OK):**
```json
{
  "id": 101,
  "isHidden": true
}
```

### `PATCH /api/admin/products/{id}/unhide`
Restores a hidden product to public view. (Requires **Admin** role).

**Response (200 OK):**
```json
{
  "id": 101,
  "isHidden": false
}
```

### `GET /api/admin/products`
Retrieves all products on the platform, including hidden ones, for moderation purposes. (Requires **Admin** role).

**Response (200 OK):**
```json
{
  "totalItems": 1000,
  "data": [
    {
      "id": 101,
      "vendorId": 5,
      "name": "Ergonomic Office Chair",
      "isHidden": true,
      "hideReason": "Violates terms of service regarding restricted materials."
    }
  ]
}
```

---

## 9. FILTERING & SEARCH

The `GET /api/products` endpoint supports extensive filtering via query parameters. 

**Supported Query Parameters:**
- `categoryId` (int)
- `subCategoryId` (int)
- `productTypeId` (int)
- `vendorId` (int)
- `minPrice` (decimal)
- `maxPrice` (decimal)
- `search` (string) - Searches product titles and descriptions.

**Filtering Strategy:** Provide these parameters in the query string. The backend uses an intersection (`AND` logic) for disparate filter types. 

---

## 10. BUSINESS RULES SUMMARY

To ensure frontend logic aligns perfectly with the backend, adhere to these business rules:

1. **Instant Publishing:** Never show a "Pending Approval" UI state for vendors. Products are live instantly.
2. **Post-Moderation:** Only admins can hide products. Vendors cannot see a "Waiting for Admin" status.
3. **Dynamic Pricing:** Always display the price dynamically based on `BasePrice + SUM(Option PriceDeltas)`.
4. **Order Immutability:** When viewing order history, display the snapshot prices returned by the Order API; do not attempt to recalculate prices using current product data.
5. **Categorization:** Force users/vendors to navigate the hierarchy: Category ➔ SubCategory ➔ ProductType.
