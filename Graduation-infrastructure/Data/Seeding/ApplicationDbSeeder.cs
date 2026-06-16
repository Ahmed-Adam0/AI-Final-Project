using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context
    )
    {
        await context.Database.MigrateAsync();

        // =========================
        // 1. ROLES
        // =========================
        string[] roles = { "Admin", "Workshop", "Customer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // =========================
        // 2. USERS (WORKSHOP USER)
        // =========================
        var workshopUser = await userManager.FindByEmailAsync("workshop@test.com");

        if (workshopUser == null)
        {
            workshopUser = new ApplicationUser
            {
                UserName = "workshop@test.com",
                Email = "workshop@test.com",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(workshopUser, "Password123!");
            await userManager.AddToRoleAsync(workshopUser, "Workshop");
        }

        // =========================
        // 3. WORKSHOPS
        // =========================
        if (!context.Workshops.Any())
        {
            var workshop = new Workshop
            {
                WorkshopNameAr = "ورشة الأثاث الحديث",
                WorkshopNameEn = "Modern Furniture Workshop",
                DescriptionAr = "أفضل ورشة أثاث",
                DescriptionEn = "Best furniture workshop",
                IsVerified = true,
                IsActive = true,
                UserId = workshopUser.Id,
            };

            context.Workshops.Add(workshop);
            await context.SaveChangesAsync();

            // Add WorkshopAddress
            var workshopAddress = new WorkshopAddress
            {
                WorkshopId = workshop.Id,
                City = "Cairo",
                Area = "Downtown",
                Street = "Main Street",
                BuildingNumber = "123",
                Notes = "Near the main square",
                IsActive = true
            };

            context.WorkshopAddresses.Add(workshopAddress);
            await context.SaveChangesAsync();
        }

        var workshopId = context.Workshops.First().Id;

        // =========================
        // 4. CATEGORIES
        // =========================
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { NameAr = "غرف نوم", NameEn = "Bedroom" },
                new Category { NameAr = "غرف معيشة", NameEn = "Living Room" },
                new Category { NameAr = "مكاتب", NameEn = "Office" }
            );

            await context.SaveChangesAsync();
        }

        var categoryId = context.Categories.First().Id;

        // =========================
        // 5. PRODUCTS (20 PRODUCTS)
        // =========================
        if (!context.Products.Any())
        {
            var products = new List<Product>();

            for (int i = 1; i <= 20; i++)
            {
                products.Add(
                    new Product
                    {
                        NameAr = $"منتج {i}",
                        NameEn = $"Product {i}",
                        DescriptionAr = "وصف المنتج",
                        DescriptionEn = "Product description",
                        CategoryId = categoryId,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                    }
                );
            }

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Seed listings and variants for each product
            foreach (var product in products)
            {
                var listing = new VendorProductListing
                {
                    ProductId = product.Id,
                    WorkshopId = workshopId,
                    BasePrice = 1000 + (product.Id * 100),
                    IsAvailable = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.VendorProductListings.Add(listing);
                await context.SaveChangesAsync();

                var variant = new ProductVariant
                {
                    ListingId = listing.Id,
                    PriceDelta = 0m,
                    CurrentPrice = listing.BasePrice,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.ProductVariants.Add(variant);
            }
            await context.SaveChangesAsync();
        }
    }
}
