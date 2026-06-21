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
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
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
        public DbSet<VendorOrder> VendorOrders { get; set; }
        public DbSet<VendorOrderStatusHistory> VendorOrderStatusHistories { get; set; }
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

        // ── Vendor-Driven Catalog ──────────────────────────────────────────────────
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

        // ── Vendor Materials Library ───────────────────────────────────────────────
        public DbSet<VendorMaterialGroup> VendorMaterialGroups { get; set; }
        public DbSet<VendorMaterialOption> VendorMaterialOptions { get; set; }
        public DbSet<ProductMaterialOption> ProductMaterialOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Product (Vendor-owned) ────────────────────────────────────
             builder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.ProductType)
                      .WithMany(pt => pt.Products)
                      .HasForeignKey(p => p.ProductTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Workshop)
                      .WithMany(w => w.Products)
                      .HasForeignKey(p => p.WorkshopId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.BasePrice)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
            });

            // ── SubCategory ─────────────────────────────────────────────────
            builder.Entity<SubCategory>(entity =>
            {
                entity.ToTable("SubCategories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NameAr).HasMaxLength(100).IsRequired();
                entity.Property(e => e.NameEn).HasMaxLength(100).IsRequired();

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ProductType ─────────────────────────────────────────────────
            builder.Entity<ProductType>(entity =>
            {
                entity.ToTable("ProductTypes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NameAr).HasMaxLength(100).IsRequired();
                entity.Property(e => e.NameEn).HasMaxLength(100).IsRequired();

                entity.HasOne(e => e.SubCategory)
                      .WithMany(sc => sc.ProductTypes)
                      .HasForeignKey(e => e.SubCategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder
                .Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId);

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
                entity.Property(e => e.PriceDelta).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Attribute)
                      .WithMany(a => a.Values)
                      .HasForeignKey(e => e.AttributeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Vendor Materials Library ───────────────────────────────────────────
            builder.Entity<VendorMaterialGroup>(entity =>
            {
                entity.ToTable("VendorMaterialGroups");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NameAr).HasMaxLength(100).IsRequired();
                entity.Property(e => e.NameEn).HasMaxLength(100).IsRequired();

                entity.HasOne(e => e.Workshop)
                      .WithMany(w => w.MaterialGroups)
                      .HasForeignKey(e => e.WorkshopId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<VendorMaterialOption>(entity =>
            {
                entity.ToTable("VendorMaterialOptions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ValueAr).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ValueEn).HasMaxLength(200).IsRequired();
                entity.Property(e => e.PriceDelta).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Group)
                      .WithMany(g => g.Options)
                      .HasForeignKey(e => e.VendorMaterialGroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ProductMaterialOption>(entity =>
            {
                entity.ToTable("ProductMaterialOptions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PriceOption)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.MaterialOptions)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.VendorMaterialOption)
                      .WithMany(o => o.ProductMaterialOptions)
                      .HasForeignKey(e => e.VendorMaterialOptionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── CartItem ─────────────────────────
            builder.Entity<CartItem>(entity =>
            {
                entity.Property(e => e.CachedPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Cart)
                      .WithMany(c => c.Items)
                      .HasForeignKey(e => e.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
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

                entity.HasOne(e => e.VendorOrder)
                      .WithMany(vo => vo.Items)
                      .HasForeignKey(e => e.VendorOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Soft-reference: if a product is deleted, preserve the snapshot (set null)
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── VendorOrder Configuration ──────────────────────────────────────────
            builder.Entity<VendorOrder>(entity =>
            {
                entity.ToTable("VendorOrders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

                entity.HasOne(e => e.MasterOrder)
                      .WithMany(o => o.VendorOrders)
                      .HasForeignKey(e => e.MasterOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Workshop)
                      .WithMany()
                      .HasForeignKey(e => e.WorkshopId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder
                .Entity<VendorOrderStatusHistory>()
                .HasOne(h => h.VendorOrder)
                .WithMany(vo => vo.StatusHistory)
                .HasForeignKey(h => h.VendorOrderId);

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
