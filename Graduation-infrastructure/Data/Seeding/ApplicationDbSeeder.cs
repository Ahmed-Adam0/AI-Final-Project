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

        // Clean up old flat categories that don't have any subcategories
        var emptyCategories = await context.Categories
            .Where(c => !context.SubCategories.Any(sc => sc.CategoryId == c.Id))
            .ToListAsync();
        if (emptyCategories.Any())
        {
            context.Categories.RemoveRange(emptyCategories);
            await context.SaveChangesAsync();
        }

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
        // 4. CATEGORIES, SUBCATEGORIES, PRODUCT TYPES
        // =========================
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { NameAr = "الأثاث", NameEn = "Furniture", ImageUrl = "" },
                new Category { NameAr = "الإضاءة", NameEn = "Lighting", ImageUrl = "" },
                new Category { NameAr = "الديكور", NameEn = "Decor", ImageUrl = "" },
                new Category { NameAr = "الستائر", NameEn = "Curtains", ImageUrl = "" },
                new Category { NameAr = "السجاد", NameEn = "Carpets", ImageUrl = "" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var furniture = categories[0];
            var lighting = categories[1];
            var decor = categories[2];
            var curtains = categories[3];
            var carpets = categories[4];

            // SubCategories
            var subCategories = new List<SubCategory>
            {
                new SubCategory { NameAr = "غرفة المعيشة", NameEn = "Living Room", CategoryId = furniture.Id },
                new SubCategory { NameAr = "غرفة النوم", NameEn = "Bedroom", CategoryId = furniture.Id },
                new SubCategory { NameAr = "غرفة الطعام", NameEn = "Dining Room", CategoryId = furniture.Id },
                new SubCategory { NameAr = "المكتب", NameEn = "Office", CategoryId = furniture.Id },
                new SubCategory { NameAr = "الخارجية", NameEn = "Outdoor", CategoryId = furniture.Id },

                new SubCategory { NameAr = "إضاءة داخلية", NameEn = "Indoor Lighting", CategoryId = lighting.Id },
                new SubCategory { NameAr = "إضاءة خارجية", NameEn = "Outdoor Lighting", CategoryId = lighting.Id },
                new SubCategory { NameAr = "إضاءة ذكية", NameEn = "Smart Lighting", CategoryId = lighting.Id },

                new SubCategory { NameAr = "ديكور حائط", NameEn = "Wall Decor", CategoryId = decor.Id },
                new SubCategory { NameAr = "إكسسوارات منزلية", NameEn = "Home Accessories", CategoryId = decor.Id },

                new SubCategory { NameAr = "الستائر", NameEn = "Curtains", CategoryId = curtains.Id },
                new SubCategory { NameAr = "السجاد", NameEn = "Carpets", CategoryId = carpets.Id }
            };

            context.SubCategories.AddRange(subCategories);
            await context.SaveChangesAsync();

            // ProductTypes
            var productTypes = new List<ProductType>
            {
                // Living Room
                new ProductType { NameAr = "أريكة", NameEn = "Sofa", SubCategoryId = subCategories[0].Id },
                new ProductType { NameAr = "أريكة زاوية", NameEn = "Corner Sofa", SubCategoryId = subCategories[0].Id },
                new ProductType { NameAr = "طاولة قهوة", NameEn = "Coffee Table", SubCategoryId = subCategories[0].Id },
                new ProductType { NameAr = "طاولة تلفزيون", NameEn = "TV Unit", SubCategoryId = subCategories[0].Id },
                new ProductType { NameAr = "كرسي ذراعين", NameEn = "Arm Chair", SubCategoryId = subCategories[0].Id },

                // Bedroom
                new ProductType { NameAr = "سرير", NameEn = "Bed", SubCategoryId = subCategories[1].Id },
                new ProductType { NameAr = "خزانة ملابس", NameEn = "Wardrobe", SubCategoryId = subCategories[1].Id },
                new ProductType { NameAr = "طاولة سرير جانبية", NameEn = "Nightstand", SubCategoryId = subCategories[1].Id },
                new ProductType { NameAr = "تسريحة", NameEn = "Dressing Table", SubCategoryId = subCategories[1].Id },

                // Office
                new ProductType { NameAr = "مكتب", NameEn = "Office Desk", SubCategoryId = subCategories[3].Id },
                new ProductType { NameAr = "كرسي مكتب", NameEn = "Office Chair", SubCategoryId = subCategories[3].Id },
                new ProductType { NameAr = "مكتبة كتب", NameEn = "Library", SubCategoryId = subCategories[3].Id },

                // Indoor Lighting
                new ProductType { NameAr = "نجفة", NameEn = "Chandelier", SubCategoryId = subCategories[5].Id },
                new ProductType { NameAr = "أباجورة طاولة", NameEn = "Table Lamp", SubCategoryId = subCategories[5].Id }
            };

            context.ProductTypes.AddRange(productTypes);
            await context.SaveChangesAsync();
        }

        var productTypeId = context.ProductTypes.First().Id;

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
                        ProductTypeId = productTypeId,
                        WorkshopId = workshopId,
                        BasePrice = 1000 + (i * 100),
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                    }
                );
            }

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
