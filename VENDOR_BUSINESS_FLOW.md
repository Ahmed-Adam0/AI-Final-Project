# Furniture Marketplace: Vendor-Driven Architecture - Business & System Flow

## 1. Executive Summary
The platform is a multi-vendor furniture marketplace designed to empower independent vendors, workshops, and craftsmen to sell their configurable products directly to consumers. The architecture is strictly vendor-driven: the platform acts as an intermediary for transactions, discovery, and quality assurance, rather than an inventory owner. Vendors have complete autonomy over their product listings, including custom configurations and dynamic pricing.

## 2. Business Goals
* **Vendor Empowerment:** Provide vendors with a flexible system to define products with dynamic attributes that suit their specific manufacturing capabilities (e.g., custom dimensions, fabric choices).
* **Operational Efficiency:** Eliminate the need for administrators to manage product catalogs. Admin effort should be solely focused on moderation, platform safety, and quality assurance.
* **Customer Personalization:** Offer customers an engaging shopping experience where they can fully customize furniture pieces before purchase, with transparent dynamic pricing reflecting their choices.
* **Scalability:** Ensure the data model supports an infinite variety of product configurations without requiring schema changes or admin intervention.

## 3. Marketplace Ownership Model
* **Vendor Ownership:** All products are strictly owned by the vendor who created them. The platform does not host "generic" products. 
* **Complete Flexibility:** There are no globally enforced product attributes (like a universal "Color" dropdown). Vendors define their own attributes (e.g., "Wood Finish", "Upholstery Type") specific to each product.
* **Administrator Role:** Administrators do not create, edit, or manage products or attributes. Their role is restricted to overseeing the marketplace, reviewing submissions, and taking moderation actions (Approve/Reject/Hide) to enforce platform guidelines.

## 4. Vendor Workflow
### Product Creation Workflow
1. The vendor initiates the creation of a new product listing.
2. The vendor provides core details: Product Name, Description, Category, and a Base Price.
3. The vendor uploads images to represent the product.
4. The product is saved as a `Draft`.

### Attribute & Option Management Workflow
1. While in `Draft` state, the vendor can define custom attributes for the product (e.g., "Size", "Material").
2. For each attribute, the vendor defines selectable options (e.g., for "Size": "Small", "Medium", "Large").
3. The vendor can easily add, edit, or delete these options and attributes without platform restrictions.

### Pricing Management Workflow
1. The product has a mandatory `Base Price`.
2. As the vendor creates configurable options, they can assign a `Price Delta` (positive, negative, or zero) to each option. 
3. The vendor reviews the configuration to ensure the dynamic pricing aligns with their manufacturing costs.

### Product Submission Workflow
1. Once satisfied, the vendor submits the product for review.
2. The product transitions from `Draft` to `Submitted For Review`.
3. The product is locked for major edits while under review.

### Product Editing Workflow
1. For an `Approved` product, the vendor can make edits.
2. Minor edits (e.g., stock levels) may apply immediately.
3. Major edits (e.g., changing price or descriptions) transition the product to `Update Pending Review` or create a draft revision, requiring admin re-approval.

### Image Management Workflow
1. Vendors can upload multiple images per product.
2. They select one image as the primary display image.
3. Images can be added or removed dynamically.

## 5. Customer Workflow
1. **Browsing Products:** Customers explore the marketplace, filtering by categories, price ranges, or specific vendors. Only `Approved` products are visible.
2. **Product Details:** The customer selects a product and views its details, including the base price, descriptions, and images.
3. **Configuration Selection:** The customer interacts with the vendor-defined attributes. They select their preferred options (e.g., selecting "Oak Wood" and "Red Velvet").
4. **Dynamic Pricing:** As the customer changes their selections, the UI dynamically updates the final price by applying the price deltas of the selected options to the base price.
5. **Add to Cart:** The customer adds the configured product to their cart. The system captures a "snapshot" of the chosen configuration and the finalized price.
6. **Checkout & Order:** The order is placed. The specific configuration requested by the customer is permanently preserved on the order line item, ensuring the vendor knows exactly what to manufacture.

## 6. Admin Workflow
1. **Marketplace Monitoring:** Administrators use a dashboard to monitor platform activity, including products awaiting review, vendor registrations, and reported items.
2. **Product Review Process:** Admins access a moderation queue of products in the `Submitted For Review` or `Update Pending Review` state.
3. **Product Approval:** If the product meets platform standards, the admin approves it. The product transitions to `Approved` and becomes visible to customers.
4. **Product Rejection Process:** If the product violates policies, the admin rejects it, optionally providing a rejection reason. The product transitions to `Rejected` and is returned to the vendor for correction.
5. **Content Moderation:** Admins have the authority to forcibly hide or deactivate any `Approved` product if it is later found to violate terms of service.
6. **Vendor Oversight:** Admins can review vendor profiles, monitor vendor metrics, and suspend vendor accounts if necessary.

## 7. Product Lifecycle
The lifecycle of a product follows a strict state machine:

* **Draft:** The initial state. The vendor is actively building the product configuration. Not visible to customers or admins (except for system audits).
* **Submitted For Review:** The vendor has finished editing and submitted the product. It enters the admin moderation queue. Edits are locked.
* **Approved:** The admin has reviewed and accepted the product. It is now live on the marketplace and visible to customers.
* **Rejected:** The admin has declined the submission. The product returns to the vendor with feedback. The vendor must make changes and resubmit.
* **Update Pending Review:** An already `Approved` product that has undergone significant modifications by the vendor. The original version may remain live while the updates await admin approval.

**State Transitions:**
* `Draft` -> `Submitted For Review`
* `Submitted For Review` -> `Approved`
* `Submitted For Review` -> `Rejected`
* `Rejected` -> `Submitted For Review` (after vendor edits)
* `Approved` -> `Update Pending Review` (upon major vendor edits)
* `Update Pending Review` -> `Approved`

## 8. Pricing Logic
Pricing is fully dynamic and configuration-driven.
* **Base Price:** Every product has a starting price defined by the vendor.
* **Price Deltas:** Every configurable option has an associated delta value.
  * *Example:* Base Price = $1000. 
  * Option A (Standard Fabric) Delta = $0.
  * Option B (Premium Leather) Delta = +$200.
* **Final Calculation:** `Final Price = Base Price + Sum(Selected Option Deltas)`.
* **Discounts:** If platform discounts or vendor sales are active, they are applied to the `Final Price`.

## 9. Business Rules
* Vendors cannot publish products directly to the marketplace; all items require admin approval.
* Administrators cannot edit a vendor's product details, attributes, or prices. They can only approve, reject, or hide the product.
* Products must have at least one image and a valid base price greater than zero to be submitted for review.
* When a customer places an order, the specific configuration and price are snapshotted. Subsequent changes to the product by the vendor do not affect historical orders.
* Vendors can temporarily deactivate an `Approved` product (e.g., if they are unable to fulfill orders) without losing the approval status.

## 10. Validation Rules
* **Attributes:** A product can have zero or many attributes. If an attribute is created, it must have at least one option.
* **Options:** Option price deltas can be positive, negative, or zero, but the resulting final price of any valid configuration must not fall below a minimum threshold (e.g., $0 or a platform minimum).
* **Categories:** Every product must be assigned to an existing, active platform category.
* **Pricing:** Base price must be strictly greater than 0.

## 11. Edge Cases
* **Vendor Deletes an Option in an Active Cart:** If a vendor deletes a configuration option while a customer has it in their cart, the cart item must be invalidated, and the customer must be notified during checkout.
* **Price Changes During Checkout:** If a vendor modifies the base price or option deltas while a customer is actively checking out, the system should either honor the snapshotted cart price for a limited time (e.g., 30 minutes) or require the customer to accept the new price.
* **Admin Hides an Ordered Product:** If an admin hides a product that is currently part of an active, unfulfilled order, the order processing continues normally, but the product link becomes inaccessible.

## 12. Risks and Considerations
* **Complex Data Queries:** Storing dynamic attributes means filtering products by specific traits (e.g., "Show me all Red sofas") becomes technically challenging. The platform must rely on broad categories or implement an advanced search index (like Elasticsearch) in the future.
* **Review Bottleneck:** If the vendor base grows rapidly, the manual admin review process could become a bottleneck. The business may need to consider "Trusted Vendor" statuses that bypass manual review.
* **Inconsistent User Experience:** Because vendors define their own attributes, one vendor might use "Color" while another uses "Tint". This limits the platform's ability to create standardized global filters.

## 13. Recommended Implementation Order
1. **Core Domain Foundation:** Establish the basic `Product` entity with `BasePrice`, `WorkshopId`, and `ProductStatus`.
2. **Dynamic Configuration Layer:** Implement the data models and APIs for custom `Attributes` and `Options` with `PriceDeltas`.
3. **Vendor Management Portal:** Build the UI/workflows for vendors to create, configure, and manage their products.
4. **Admin Moderation System:** Develop the admin dashboard, review queues, and state transition logic.
5. **Customer Experience & Cart:** Implement product browsing, dynamic price calculation on the frontend, and the cart snapshotting logic.
6. **Order Processing:** Ensure the final order generation perfectly captures the custom configurations.

## 14. Phase-by-Phase Execution Plan

### Phase 1: Database & API Foundation
* Define the database schema for Products, Attributes, and Options.
* Create the RESTful APIs for Vendor CRUD operations on products.
* Implement the state machine logic for `ProductStatus`.

### Phase 2: Vendor Dashboard
* Build the frontend interfaces for vendors to manage their catalogs.
* Implement the dynamic attribute builder interface.
* Create the image upload and management workflow.

### Phase 3: Admin Portal
* Build the moderation queue interface.
* Implement Approve/Reject actions with optional feedback forms.
* Create the marketplace overview dashboard.

### Phase 4: Customer Storefront
* Develop the product listing and detail pages.
* Implement the interactive configuration selector with real-time price updates.
* Build the shopping cart functionality ensuring snapshot integrity.

### Phase 5: Testing & Launch
* Perform end-to-end testing of the full lifecycle (Create -> Review -> Approve -> Purchase).
* Test edge cases (price updates during checkout, attribute deletions).
* Finalize platform terms of service regarding vendor responsibilities.
