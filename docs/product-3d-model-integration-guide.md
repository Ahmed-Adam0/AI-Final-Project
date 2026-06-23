# Product 3D Model Integration Guide (Frontend)

This guide documents the endpoints and payloads for integrating 3D model support (`.glb`, `.gltf`, `.fbx`, `.obj`) for products.

---

## 1. Retrieve Product Details (GET)

Use this endpoint to fetch product details. If the product has an associated 3D model, its URL will be returned in the `product3DModelUrl` property. If not, it will return `null`.

* **Endpoint**: `GET /api/products/{id}`
* **Headers**: `Accept: application/json`

### Example Response:

```json
{
  "id": 1,
  "nameAr": "كرسي مكتب مريح",
  "nameEn": "Comfortable Office Chair",
  "descriptionAr": "كرسي طبي مريح للظهر...",
  "descriptionEn": "Orthopedic office chair...",
  "basePrice": 3500.00,
  "productTypeId": 2,
  "images": [
    {
      "id": 42,
      "imageUrl": "/images/products/chair.jpg",
      "isPrimary": true
    }
  ],
  // New Property: 3D Model URL (null if not available)
  "product3DModelUrl": "https://res.cloudinary.com/your-cloudinary-name/raw/upload/v1234567890/products/3d-models/chair_model.glb"
}
```

---

## 2. Upload/Set 3D Model (POST)

* **Access**: Vendors/Workshops only (requires Bearer token).
* **Endpoint**: `POST /api/products/{productId}/3d-model`
* **Content-Type**: `multipart/form-data`

### Request Payload:

Pass the 3D model file as a form-data parameter:

| Key | Type | Description |
|:---|:---|:---|
| `file` | `File` | The 3D model file. Allowed extensions: `.glb` (preferred), `.gltf`, `.fbx`, `.obj`. Max size ~20MB. |

### Example Request (JavaScript/TypeScript):

```typescript
const formData = new FormData();
formData.append('file', glbFile); // glbFile is a File object from input

const response = await fetch(`/api/products/${productId}/3d-model`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const data = await response.json();
console.log(data.product3DModelUrl);
```

### Example Response (Success - 200 OK):

```json
{
  "product3DModelUrl": "https://res.cloudinary.com/your-cloudinary-name/raw/upload/v1234567890/products/3d-models/chair_model.glb"
}
```

---

## 3. Delete 3D Model (DELETE)

* **Access**: Vendors/Workshops only (requires Bearer token).
* **Endpoint**: `DELETE /api/products/{productId}/3d-model`

### Example Request (JavaScript/TypeScript):

```typescript
const response = await fetch(`/api/products/${productId}/3d-model`, {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

if (response.ok) {
  console.log("3D model removed successfully");
}
```

### Example Response (Success - 200 OK):

```json
{
  "message": "3D model removed successfully"
}
```
