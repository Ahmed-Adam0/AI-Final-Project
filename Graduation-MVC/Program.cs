using Graduation_Application.IServices;
using Graduation_Application.Services;
using Graduation_infrastructure.ProgramService.ServicesMVC;
using Graduation_infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

namespace Graduation_MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =====================
            // SERVICES
            // =====================
            builder.Services.AddControllersWithViews();

            // Infrastructure (MVC services)
            builder.Services.AddInfrastructureMVC(builder.Configuration);

            // Register Category and File services
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
            builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IFaqService, FaqService>();
            builder.Services.AddScoped<IBannerService, BannerService>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            var app = builder.Build();

            // =====================
            // ERROR HANDLING
            // =====================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // =====================
            // PIPELINE
            // =====================
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Serve product images written by the API under Graduation-API/wwwroot (e.g. /images/products/...)
            var apiWwwRootSetting = builder.Configuration["ApiSettings:WwwRootPath"];
            var apiWwwRoot = string.IsNullOrWhiteSpace(apiWwwRootSetting)
                ? Path.GetFullPath(
                    Path.Combine(app.Environment.ContentRootPath, "..", "Graduation-API", "wwwroot")
                )
                : Path.GetFullPath(
                    Path.IsPathRooted(apiWwwRootSetting)
                        ? apiWwwRootSetting
                        : Path.Combine(app.Environment.ContentRootPath, apiWwwRootSetting)
                );

            if (Directory.Exists(apiWwwRoot))
            {
                app.UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(apiWwwRoot),
                        RequestPath = "",
                    }
                );
            }
            app.UseHttpsRedirection();


            var supportedCultures = new[] { "en", "ar" };

            app.Use(async (context, next) =>
            {
                var culture = context.Request.Cookies["culture"];

                if (string.IsNullOrEmpty(culture))
                {
                    culture = "en";
                }

                var cultureInfo = new CultureInfo(culture);

                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;

                await next();
            });
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // =====================
            // ROUTING (IMPORTANT)
            // =====================

            // Root route -> Admin Dashboard
            app.MapControllerRoute(
                name: "root",
                pattern: "",
                defaults: new { area = "Admin", controller = "Dashboard", action = "Index" }
            );

            // 🔥 Areas Route (Admin, etc.)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
            );

            // Default Route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}
