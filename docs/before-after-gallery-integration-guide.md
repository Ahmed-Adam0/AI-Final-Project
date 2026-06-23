# Frontend Integration Guide: Before/After Inspiration Gallery

This document outlines the API contracts and workflow for implementing the **Before/After Inspiration Gallery** in the frontend client.

---

## 🔄 Feature Workflow

1. **Upload Submission**:
   * Authenticated customers can upload multiple before/after image pairs for a specific order they purchased.
   * On upload, submissions default to a **Pending Approval** state (`IsApproved = false`).
2. **Moderation**:
   * *Admin Note*: An administrator reviews the submissions inside the Admin Console. Once approved, `IsApproved` is changed to `true`.
3. **Public Gallery**:
   * The public gallery pulls only **Approved** room transformations for visitors to browse.

---

## 📡 API Endpoints

### 1. Upload Transformations (Secured)

Allows an authenticated customer to upload one or more before/after image pairs for an order.

* **Route**: `POST /api/inspirations`
* **Headers**:
  * `Authorization: Bearer <JWT_TOKEN>`
  * `Content-Type: multipart/form-data`
* **Request Payload (`FormData`)**:

| Key | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `OrderId` | `number (Integer)` | Yes | The ID of the order being reviewed. |
| `BeforeImages` | `File[]` | Yes | List of "Before" images. |
| `AfterImages` | `File[]` | Yes | List of "After" images. Must match the length of `BeforeImages`. |

#### Sample Request (`FormData` construction in JS):
```javascript
const formData = new FormData();
formData.append('OrderId', 1045);

// Mapped by index, e.g. BeforeImages[0] pairs with AfterImages[0]
beforeFiles.forEach(file => formData.append('BeforeImages', file));
afterFiles.forEach(file => formData.append('AfterImages', file));

http.post('/api/inspirations', formData);
```

#### Responses:
* **`200 OK`**:
  ```json
  {
    "message": "Inspiration photos uploaded successfully. Submissions are pending admin approval."
  }
  ```
* **`400 Bad Request`**: Mismatched file arrays length, empty lists, or file format issues.
  ```json
  {
    "message": "The number of Before images must match the number of After images."
  }
  ```
* **`401 Unauthorized`**: JWT is missing or expired.
* **`403 Forbidden`**: The order does not belong to the authenticated user.
* **`404 Not Found`**: The order ID provided does not exist.

---

### 2. Public Inspirations Gallery (Anonymous)

Retrieves paginated, approved before/after room transformation images.

* **Route**: `GET /api/inspirations`
* **Headers**: None (Anonymous Access)
* **Query Parameters**:
  * `pageNumber` (number, optional, default: `1`): The page to retrieve.
  * `pageSize` (number, optional, default: `4`): Count of items per page.

#### Response Payload (Exact Contract):
```json
{
  "data": [
    {
      "id": "12",
      "beforeImageUrl": "https://res.cloudinary.com/demo/image/upload/v12345/inspirations/before_room.jpg",
      "afterImageUrl": "https://res.cloudinary.com/demo/image/upload/v12345/inspirations/after_room.jpg"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 3,
    "totalCount": 11,
    "pageSize": 4
  }
}
```
