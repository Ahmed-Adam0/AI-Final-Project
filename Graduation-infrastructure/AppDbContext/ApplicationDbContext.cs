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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder
                .Entity<Product>()
                .HasOne(p => p.Workshop)
                .WithMany(w => w.Products)
                .HasForeignKey(p => p.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add Product-User relationship for vendor ownership
            builder
                .Entity<Product>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

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

            builder
                .Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId);

            // FIX: use OrderId as the foreign key (not Id)
            builder
                .Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

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
            builder.Entity<CartItem>().Property(ci => ci.Price).HasColumnType("decimal(18,2)");
            builder
                .Entity<Discount>()
                .Property(d => d.DiscountValue)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<Workshop>().Property(w => w.Rating).HasColumnType("decimal(18,2)");

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
                entity.Property(x => x.Action).IsRequired().HasMaxLength(100);
                entity.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(x => x.EntityId).HasMaxLength(100);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            });
        }
    }
}
