# Vendor-Driven Marketplace Architecture Plan
## 1. Executive Summary
This document outlines the business logic, system flows, and user journeys for transitioning the furniture marketplace to a fully vendor-driven product architecture. In this new model, administrators step back from catalog creation and assume a moderation and oversight role. Vendors are empowered to create their own products from scratch, define custom configurable attributes without global restrictions, and manage their own pricing models. This pivot ensures a highly scalable and flexible marketplace where diverse furniture manufacturers can list products according to their unique specifications.
## 2. Business Goals
*   **Empower Vendors:** Provide vendors with complete autonomy to create, configure, and price their products without waiting for administrators to define global categories or attributes.
*   **Increase Flexibility:** Allow an unlimited variety of product configurations (e.g., custom dimensions, unique fabric types) that are specific to individual vendor offerings.
*   **Streamline Operations:** Shift the administrative burden from content generation to content moderation and quality assurance.
*   **Enhance Customer Experience:** Provide customers with clear, dynamic pricing that updates in real-time as they configure their desired furniture piece.
## 3. Marketplace Ownership Model
*   **Decentralized Product Creation:** The core product entity is owned entirely by the vendor who creates it. There is no central, admin-curated "Base Product" catalog that multiple vendors share.
*   **Vendor-Specific Configurations:** Attributes (e.g., Color, Material, Dimensions) are scoped to the specific product created by the vendor. There is no global attribute dictionary.
*   **Administrative Oversight:** Administrators hold the final authority over what appears on the public storefront. All newly created or significantly updated products must pass through an approval workflow before becoming visible to customers.
## 4. Vendor Workflow
1.  **Product Creation:** The vendor initiates a new product draft, providing core details: Product Name, Description, Category, Images, and Base Price.
2.  **Attribute Management:** The vendor defines custom attributes for the product (e.g., "Fabric Type", "Wood Finish").
3.  **Option Management:** For each attribute, the vendor adds specific options (e.g., for "Fabric Type", they add "Velvet", "Linen", "Cotton").
4.  **Pricing Management:** The vendor specifies a price delta (increase, decrease, or zero) for each option relative to the Base Price.
5.  **Product Submission:** Once the configuration is complete, the vendor submits the product draft for administrator review.
6.  **Product Editing:** If a product is rejected or needs updates, the vendor edits the draft and resubmits. Updating an already approved product may trigger a re-review depending on the severity of the changes.
## 5. Customer Workflow
1.  **Browsing:** Customers browse the public marketplace, which only displays "Approved" products.
2.  **Product Details:** Clicking a product opens the details page, showcasing the vendor's images, description, and base price.
3.  **Configuration:** The customer sees the specific configurable options defined by that vendor for that product.
4.  **Dynamic Pricing:** As the customer selects different options (e.g., changing from standard wood to premium oak), the UI dynamically updates the final price based on the vendor's price deltas.
5.  **Add to Cart:** The customer adds the configured product to their cart. The system captures the specific chosen configuration (the selected option IDs and their names at that moment).
6.  **Checkout & Order:** The final order preserves the exact configuration selected by the customer as an immutable snapshot, ensuring that post-purchase changes by the vendor do not affect historical orders.
## 6. Admin Workflow
1.  **Marketplace Monitoring:** Administrators use a dedicated dashboard to monitor the queue of "Submitted For Review" products.
2.  **Product Review:** The admin reviews the product's images, descriptions, and pricing to ensure they meet marketplace quality standards and do not violate policies.
3.  **Approval Process:** If the product meets all criteria, the admin marks it as "Approved," making it instantly live on the storefront.
4.  **Rejection Process:** If the product fails the review, the admin marks it as "Rejected" and must provide a mandatory rejection reason to guide the vendor.
5.  **Ongoing Moderation:** Administrators can retrospectively suspend or pull down approved products if subsequent violations are discovered or if the vendor is suspended.
## 7. Product Lifecycle
Every product goes through a strict state machine:
*   `Draft`: The initial state. The vendor is actively building the product. It is invisible to customers and admins.
*   `Submitted For Review`: The vendor has finished editing and submitted the product. It enters the admin queue. It remains invisible to customers.
*   `Approved`: The administrator has reviewed and accepted the product. It is now visible and purchasable by customers.
*   `Rejected`: The administrator has declined the product. It is returned to the vendor with feedback. It remains invisible to customers.
*   `Updated`: If a vendor edits an `Approved` product, it may revert to `Submitted For Review` (or a specific `Update Pending Review` state), temporarily hiding the changes or the entire product until re-approved.
## 8. Pricing Logic
*   **Base Price:** The foundational cost of the product in its default configuration.
*   **Price Deltas:** Modifiers attached to specific configurable options.
    *   *Positive Delta (+):* Increases the final price (e.g., Premium Leather +$200).
    *   *Negative Delta (-):* Decreases the final price (e.g., Unfinished Wood -$50).
    *   *Zero Delta (0):* No change to the base price (e.g., Standard Cotton +$0).
*   **Final Price Calculation:** `Final Price = Base Price + Sum(Selected Option Deltas)`
## 9. Business Rules
*   A product must have a Base Price greater than zero.
*   A product cannot be submitted for review without at least one primary image.
*   If a product has configurable attributes, the customer *must* select an option for every attribute before adding it to the cart.
*   A vendor cannot modify an order once it is placed; the configuration snapshot is immutable.
*   Only "Approved" products can be added to a cart or appear in search results.
## 10. Validation Rules
*   **Product Name:** Required, minimum 3 characters, maximum 100 characters.
*   **Images:** At least 1, maximum 10 per product. File types restricted to JPG, PNG, WEBP.
*   **Attributes:** Attribute names must be unique within a single product.
*   **Options:** Option names must be unique within their parent attribute.
*   **Rejection:** A rejection action by an admin strictly requires a non-empty "Reason" text field.
## 11. Edge Cases
*   **Vendor Deletes an Option:** If a vendor deletes an option that is currently in a customer's cart, the cart item becomes invalid. The system must notify the customer upon checkout that the configuration is no longer available.
*   **Price Updates During Checkout:** If a vendor updates the base price or deltas while a customer is checking out, the system must either honor the cart price for a limited window or prompt the user to accept the new price.
*   **Concurrent Editing:** If an admin is reviewing a product while a vendor is editing it, the system should lock the product or ensure the admin is reviewing the latest timestamped version.
## 12. Risks and Considerations
*   **Data Consistency in Orders:** Because attributes are vendor-defined and can be changed or deleted, it is absolutely critical to use a snapshot pattern for Order Items. Do not rely on relational links to dynamic attribute tables for historical orders.
*   **Catalog Searchability:** Without global, standardized attributes (e.g., a universal "Color" dictionary), building marketplace-wide filters (like "Show me all Red sofas") becomes challenging. The system may require an AI-driven tagging system or a mapping layer to map vendor-custom attributes to global filter facets in the future.
*   **Review Bottlenecks:** A heavily vendor-driven system can result in hundreds of products submitted daily. The admin review dashboard must be highly optimized for bulk actions and quick visual scanning.
## 13. Recommended Implementation Order
1.  **Core Product & Vendor Linking:** Establish the base Product entity owned directly by the Vendor, stripping out the old central catalog dependencies.
2.  **Product Lifecycle State Machine:** Implement the Draft -> Submitted -> Approved/Rejected statuses and admin review APIs.
3.  **Dynamic Attributes & Options:** Build the flexible attribute system scoped to the individual product.
4.  **Pricing Engine:** Implement the Base Price + Option Delta calculation logic.
5.  **Cart & Order Snapshotting:** Refactor the cart and checkout flows to capture vendor-defined configurations as immutable JSON snapshots.
6.  **Admin Moderation Dashboard:** Build the UI/APIs for admins to review and approve/reject products.
## 14. Phase-by-Phase Execution Plan
### Phase 1: Foundation & Data Migration
*   Design the new database schema for Vendor-owned Products, Custom Attributes, and Options.
*   Plan migration strategy for any existing data (if applicable) to the new structure.
### Phase 2: Vendor Product Management APIs
*   Develop APIs for vendors to create, read, update, and delete products, attributes, and options.
*   Implement the product submission workflow (status changes).
### Phase 3: Admin Moderation APIs
*   Develop APIs for admins to fetch products pending review.
*   Develop endpoints to Approve and Reject (with reason) products.
### Phase 4: Storefront & Customer Experience
*   Update the public product catalog APIs to only return Approved products.
*   Implement the dynamic pricing calculation on the backend to serve configured prices.
*   Update Cart and Order APIs to handle custom configurations and store them as snapshots.
### Phase 5: UI/UX Implementation (Frontend)
*   Build the Vendor Dashboard for flexible product creation.
*   Build the Admin Moderation Dashboard.
*   Update the Customer Storefront to handle dynamic attribute selection and real-time price updates.
