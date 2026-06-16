using Graduation_domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Graduation_infrastructure.AppDbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<WorkshopAddress> WorkshopAddresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewModerationLog> ReviewModerationLogs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
        public DbSet<FinalResultImage> FinalResultImages { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; }
        public DbSet<InternalNotification> InternalNotifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ProductReport> ProductReports { get; set; }
        public DbSet<VendorVerificationHistory> VendorVerificationHistory { get; set; }
        public DbSet<VendorAccountStatusHistory> VendorAccountStatusHistory { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<Banner> Banners { get; set; }

        // ── Multi-Vendor Catalog ──────────────────────────────────────────────────
        public DbSet<VendorProductListing> VendorProductListings { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductVariantAttributeValue> ProductVariantAttributeValues { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Product (vendor-agnostic base) ────────────────────────────────────
            builder
                .Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);

            builder
                .Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId);

            // ── VendorProductListing ──────────────────────────────────────────────
            builder.Entity<VendorProductListing>(entity =>
            {
                entity.ToTable("VendorProductListings");
                entity.HasKey(e => e.Id);

                // Enforce: one workshop can create only one listing per product
                entity.HasIndex(e => new { e.ProductId, e.WorkshopId }).IsUnique();

                entity.Property(e => e.BasePrice)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.VendorListings)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Workshop)
                      .WithMany(w => w.VendorListings)
                      .HasForeignKey(e => e.WorkshopId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ProductAttribute ─────────────────────────────────────────────────
            builder.Entity<ProductAttribute>(entity =>
            {
                entity.ToTable("ProductAttributes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NameAr).HasMaxLength(100).IsRequired();
                entity.Property(e => e.NameEn).HasMaxLength(100).IsRequired();

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Attributes)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ProductAttributeValue ────────────────────────────────────────────
            builder.Entity<ProductAttributeValue>(entity =>
            {
                entity.ToTable("ProductAttributeValues");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ValueAr).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ValueEn).HasMaxLength(200).IsRequired();

                entity.HasOne(e => e.Attribute)
                      .WithMany(a => a.Values)
                      .HasForeignKey(e => e.AttributeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ProductVariant ───────────────────────────────────────────────────
            builder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("ProductVariants");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PriceDelta).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VariantImageUrl).HasMaxLength(1000);

                entity.HasOne(e => e.Listing)
                      .WithMany(l => l.Variants)
                      .HasForeignKey(e => e.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ProductVariantAttributeValue (explicit many-to-many join) ────────
            builder.Entity<ProductVariantAttributeValue>(entity =>
            {
                entity.ToTable("ProductVariantAttributeValues");

                // Composite primary key — no surrogate Id needed
                entity.HasKey(e => new { e.VariantId, e.AttributeValueId });

                entity.HasOne(e => e.Variant)
                      .WithMany(v => v.VariantAttributeValues)
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AttributeValue)
                      .WithMany(av => av.VariantAttributeValues)
                      .HasForeignKey(e => e.AttributeValueId)
                      .OnDelete(DeleteBehavior.Restrict); // Don't cascade-delete attribute values
            });

            // ── CartItem (now references ProductVariant) ─────────────────────────
            builder.Entity<CartItem>(entity =>
            {
                entity.Property(e => e.CachedPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Cart)
                      .WithMany(c => c.Items)
                      .HasForeignKey(e => e.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ProductVariant)
                      .WithMany()
                      .HasForeignKey(e => e.ProductVariantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── OrderItem (snapshot pattern) ──────────────────────────────────────
            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.SnapshotUnitPrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.SnapshotProductNameAr).HasMaxLength(300).IsRequired();
                entity.Property(e => e.SnapshotProductNameEn).HasMaxLength(300).IsRequired();
                entity.Property(e => e.SnapshotVendorName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.SnapshotAttributesJson).HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Soft-reference: if a variant is deleted, preserve the snapshot (set null)
                entity.HasOne(e => e.ProductVariant)
                      .WithMany(v => v.OrderItems)
                      .HasForeignKey(e => e.ProductVariantId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder
                .Entity<OrderStatusHistory>()
                .HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId);

            builder
                .Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Review>()
                .HasOne(r => r.Workshop)
                .WithMany(w => w.Reviews)
                .HasForeignKey(r => r.WorkshopId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Entity<ReviewModerationLog>()
                .HasOne(l => l.Review)
                .WithMany()
                .HasForeignKey(l => l.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<WorkshopAddress>()
                .HasOne(wa => wa.Workshop)
                .WithOne(w => w.WorkshopAddress)
                .HasForeignKey<WorkshopAddress>(wa => wa.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Workshop>()
                .HasOne(w => w.VerifiedByAdmin)
                .WithMany()
                .HasForeignKey(w => w.VerifiedByAdminId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<Workshop>()
                .HasOne(w => w.AccountStatusChangedByAdmin)
                .WithMany()
                .HasForeignKey(w => w.AccountStatusChangedByAdminId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<VendorVerificationHistory>()
                .HasOne(h => h.Workshop)
                .WithMany(w => w.VerificationHistory)
                .HasForeignKey(h => h.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<VendorVerificationHistory>()
                .HasOne(h => h.PerformedByAdmin)
                .WithMany()
                .HasForeignKey(h => h.PerformedByAdminId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<VendorAccountStatusHistory>()
                .HasOne(h => h.Workshop)
                .WithMany(w => w.AccountStatusHistory)
                .HasForeignKey(h => h.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<VendorAccountStatusHistory>()
                .HasOne(h => h.PerformedByAdmin)
                .WithMany()
                .HasForeignKey(h => h.PerformedByAdminId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision configuration to avoid truncation
            builder.Entity<Discount>().Property(d => d.DiscountValue).HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Workshop>().Property(w => w.Rating).HasColumnType("decimal(18,2)");
            // Note: CartItem.CachedPrice, OrderItem snapshot prices, VendorProductListing.BasePrice,
            // and ProductVariant prices are configured in their respective entity blocks above.

            builder
                .Entity<InternalNotification>()
                .HasOne(n => n.User)
                .WithMany(u => u.internalNotifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PaymentTransaction>(entity =>
            {
                entity.Property(pt => pt.Amount).HasColumnType("decimal(18,2)");
                entity.Property(pt => pt.Currency).HasMaxLength(10);
                entity.Property(pt => pt.TransactionId).HasMaxLength(100);
                entity.Property(pt => pt.PaymentToken).HasMaxLength(4000);
                entity.Property(pt => pt.FailureReason).HasMaxLength(500);
                entity.Property(pt => pt.Status).HasConversion<string>().HasMaxLength(20);
            });

            builder.Entity<PaymentWebhookLog>(entity =>
            {
                entity.Property(wl => wl.Provider).HasMaxLength(100).IsRequired();
                entity.Property(wl => wl.Payload).HasColumnType("nvarchar(max)").IsRequired();
                entity.Property(wl => wl.ErrorMessage).HasMaxLength(1000);
            });

            // ProductReport relationships
            builder
                .Entity<ProductReport>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<ProductReport>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ActivityLog>(entity =>
            {
                entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);
                entity.Property(x => x.UserName).IsRequired().HasMaxLength(150);
                entity.Property(x => x.UserRole).IsRequired().HasMaxLength(50);
                entity.Property(x => x.UserRoleAr).HasMaxLength(50);
                entity.Property(x => x.Action).IsRequired().HasMaxLength(100);
                entity.Property(x => x.ActionAr).HasMaxLength(100);
                entity.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(x => x.EntityTypeAr).HasMaxLength(50);
                entity.Property(x => x.EntityId).HasMaxLength(100);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);
                entity.Property(x => x.DescriptionAr).HasMaxLength(1000);
            });
        }
    }
}
