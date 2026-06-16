# Vendor Materials Library Implementation Plan

This plan introduces a reusable configuration system for vendors to manage their own material libraries (e.g., Wood Type, Color, Fabric) and attach these material options to their products.

## Goal
Implement a `VendorMaterialGroup` and `VendorMaterialOption` system allowing a vendor to define reusable product attributes and price deltas. Products will reference these selected options, and dynamic pricing will apply these deltas on top of the product's base price.

## Proposed Changes

### Domain Layer (Graduation-Domain)
1. **New Entities**:
   - `VendorMaterialGroup`: Represents a grouping like "Wood Type".
     - `Id`, `WorkshopId`, `NameAr`, `NameEn`
   - `VendorMaterialOption`: Represents a choice like "Oak".
     - `Id`, `VendorMaterialGroupId`, `ValueAr`, `ValueEn`, `PriceDelta`
   - `ProductMaterialOption`: Many-to-many relationship mapping entity connecting a `Product` to the selected `VendorMaterialOption`s it supports.
     - `ProductId`, `VendorMaterialOptionId`

2. **Entity Updates**:
   - **`Workshop`**: Add `List<VendorMaterialGroup> MaterialGroups`
   - **`Product`**: Add `List<ProductMaterialOption> MaterialOptions`

> [!NOTE]
> The old `ProductAttribute` and `ProductAttributeValue` entities will be left intact for backward compatibility as per requirements, but won't be used for the new Vendor Materials Library. 

### Infrastructure Layer (Graduation-infrastructure)
1. **EF Core DB Context**:
   - Add `DbSet` for `VendorMaterialGroups`, `VendorMaterialOptions`, and `ProductMaterialOptions`.
   - Configure relationships in `OnModelCreating`.
2. **Migrations**:
   - Run `dotnet ef migrations add AddVendorMaterialsLibrary` to generate the SQL schema changes.

### Application Layer (Graduation-Application)
1. **DTOs**:
   - `VendorMaterialGroupDto`, `VendorMaterialOptionDto`
   - `CreateVendorMaterialGroupDto`, `CreateVendorMaterialOptionDto`
   - `ProductMaterialOptionDto` (to return assigned options on a product)
2. **Interfaces & Services**:
   - Add `IVendorMaterialService` and `VendorMaterialService` for CRUD operations on groups and options.
   - Update `IProductService` & `ProductService` to:
     - Accept an array of `VendorMaterialOptionId`s during Product creation and update.
     - Calculate `LivePrice` correctly using the sum of `PriceDelta`s if a CartItem or Order passes selected option IDs (this integrates into Cart calculation if the cart supports selecting specific options).
     - Include the selected materials when returning product details.
   - Update `CartService`:
     - Adjust `LivePrice` computation to be: `BasePrice + Sum(SelectedOptions.PriceDelta)`.

### API Layer (Graduation-API)
1. **`VendorMaterialsController`** (New):
   - `POST /api/vendor/material-groups`
   - `POST /api/vendor/material-groups/{id}/options`
   - `GET /api/vendor/material-groups`
   - `DELETE /api/vendor/material-groups/{id}`
   - `DELETE /api/vendor/material-groups/options/{id}`

2. **`ProductsController`** (Update):
   - Modify product creation/update DTO bindings to accept selected material option IDs.

### Admin MVC Views (Graduation-MVC)
- **Vendor Dashboard**:
  - Add a "Materials Library" page (`/Admin/VendorMaterials/Index`).
  - Add forms to create and manage `VendorMaterialGroup` and their `VendorMaterialOption`s.

## Open Questions

> [!WARNING]
> **Cart Options Selection**: When a customer adds an item to the cart, how do they specify which `VendorMaterialOption` they selected? Currently, `CartItem` has a `SelectedOptionsJson` field. Should we parse this JSON to find the selected option IDs to calculate the price delta, or should we add a `List<CartItemMaterialOption>` relational table to explicitly link the cart item to its selected materials for easier price computation? 

> [!WARNING]
> **Product Creation Flow**: The user mentioned "Product stores ONLY references to selected options". A product can have multiple options selected (e.g. Oak, Walnut, MDF) from the same group (e.g. Wood Type) to signify "Available in these materials". The customer then chooses ONE option per group. Is this the intended flow?

## Verification Plan
### Automated Tests
- Run `dotnet build` to ensure no compile errors.
- Ensure the Entity Framework migration succeeds.

### Manual Verification
- Test creating a Material Group and Option via the new controller.
- Test creating a Product with selected Material Options.
- Test adding the Product to a Cart and verify `TotalPrice` reflects the `BasePrice + PriceDelta`.
